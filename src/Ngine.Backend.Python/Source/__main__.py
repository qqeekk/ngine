from sklearn.preprocessing import *
from sklearn.model_selection import train_test_split
import pandas as pd
import numpy as np
import cv2
import argparse
import yaml, re
import sys, os
from itertools import *
from tensorflow import keras
from tensorflow.keras import layers, models
# from keras import layers, models


def pairwise(iterable):
    "s -> (s0,s1), (s1,s2), (s2, s3), ..."
    a, b = tee(iterable)
    next(b, None)
    return zip(a, b)

def replace_tuple_value(tp, index, value):
    ls = list(tp)
    ls[index] = value
    return tuple(ls)


# d | d:d |: | d: | :d
prop_regex = r"^(?P<type>(lin|cats|cons|img)):(\$(?P<ref>\w+)(\[(?P<start>\d+)?(?P<range>:(?P<last>\d+)?)?\])?)$"

mappings_by_file_name = dict() # string -> bool -> id -> [shape, df, train, test -> train_x, test_x]
#mappings_by_dir_name = dict() # string -> bool -> id -> [shape, train, test -> train_x, test_x]

def convert_df_col_to_categorical(df, train, test, label_index):
    zipBinarizer = LabelBinarizer().fit(df[label_index])
    trainCategorical = zipBinarizer.transform(train[label_index])
    testCategorical = zipBinarizer.transform(test[label_index])

    print("[DEBUG] столбец {} категорий: {}".format(label_index, zipBinarizer.classes_))
    return trainCategorical, testCategorical


def convert_df_cols_to_continuous(train, test, label_indices):
    cs = MinMaxScaler()
    trainContinuous = cs.fit_transform(train[label_indices])
    testContinuous = cs.transform(test[label_indices])

    print("[DEBUG] столбец {} значений от {} до {}".format(label_indices, len(cs.data_min_), cs.data_max_))
    return trainContinuous, testContinuous


def process_single_df_col(type, index):
    index = int(index)

    if type == 'lin':
        return lambda _, df, train, test: (train[index], test[index])
    elif type == 'cats':
        return lambda _, df, train, test: convert_df_col_to_categorical(df, train, test, index)
    elif type == 'cons':
        return lambda _, df, train, test: convert_df_cols_to_continuous(train, test, index)
    elif type == 'img':
        raise Exception("Could not convert one csv field ({}) to image", index)


def process_multi_df_cols(type, start, end):
    def get_index_range(df):
        s = int(start) if (start is not None) else 0
        e = int(end) if (end is not None) else len(df.columns)
        return range(s, e)

    if type == 'lin':
        return lambda _, df, train, test: (train[get_index_range(df)], test[get_index_range(df)])

    elif type == 'cats':
        return lambda _, df, train, test: \
            np.hstack([convert_df_col_to_categorical(df, train, test, i) for i in get_index_range(df)])

    elif type == 'cons':
        return lambda _, df, train, test: convert_df_cols_to_continuous(train, test, get_index_range(df))
    
    elif type == 'img':
        def unsqueeze_images(shape, df, train, test):
            indices = get_index_range(df)

            print("[DEBUG] columns {} of {} images".format(indices, shape))
            return (
                np.array([v.astype(int).reshape(*shape) for v in train[indices].values]),
                np.array([v.astype(int).reshape(*shape) for v in test[indices].values]))

        return lambda shape, df, train, test: unsqueeze_images(shape, df, train, test)


def process_single_col(mappings, is_file, type, index):
    if is_file:
        mappings.append(process_single_df_col(type, index))
    else:
        # directory (images only)
        raise Exception("Unable to index a dataset of type directory. Use range option [:]")


def process_multi_col(mappings, is_file, type, start, end):
    if is_file:
        mappings.append(process_multi_df_cols(type, start, end))
    elif type == 'img':
        # TODO: directory (images only)
        def func(shape, df, train, test):
            

        pass


def parse_prop(input_or_head, id, prop, aliases):
    m = re.match(prop_regex, prop)
            
    if m is not None:
        obj = m.groupdict()
        file, is_file = aliases[obj["ref"]]

        mappings = []
        if file not in mappings_by_file_name:
            mappings_by_file_name[file] = { input_or_head: { id: mappings }, not input_or_head: dict() }
        else:
            mappings_by_file_name[file][input_or_head][id] = mappings
            
        if obj["range"] is None:
            if obj["start"] is not None:
                process_single_col(mappings, is_file, obj["type"], obj["start"])
            else:
                raise Exception("Unexpected empty brackets for file mappings: {}. If you want to get all properties of csv data frame, use [:] instead".format(prop))
        else:
            process_multi_col(mappings, is_file, obj["type"], obj["start"], obj["last"])

    else:
        raise Exception("Input {} does not match the pattern {}".format(prop, prop_regex))


def parse_ambiguity(value, hp, name):
    basic_list = re.match(r'\[(?P<list>\d+(,\d+)*)\]', value)
    
    if basic_list is not None:
        obj = basic_list.groupdict()
        values = [int(v) for v in obj['list'].split(',')]

        hp.Choice(name, values)
        return

    basic_range = re.match(r'\[(?P<start>\d+):(?P<end>\d+):(?P<step>\d+)\]', value)
    if basic_range is not None:
        obj = basic_range.groupdict()
        hp.Int(name, int(obj['start']), int(obj['end']), step = int(obj['step']))
        return

    raise Exception('No rules were matched. Invalid ambiguity value: {}'.format(value))


def parse_ambiguity_mapping(prop):
    prop_match = re.match(r'(?P<prop>\w+)(\[(?P<index>\d+)\])?', prop)

    if prop_match is not None:
        obj = prop_match.groupdict()
        return obj['prop'], int(obj['index']) if obj['index'] is not None else None


def traverse_file_aliases(files):
    file_aliases = dict()
    for a in files:
        file_or_dir = files[a]

        if os.path.exists(file_or_dir):
            file_aliases[a] = (file_or_dir, os.path.isfile(file_or_dir))
        else:
            raise Exception('File {} does not exist'.format(file_or_dir))

    return file_aliases


def prepare_data(m, mappings, validation_split):
    aliases = traverse_file_aliases(mappings['files'])
        
    for input_id, input in enumerate(mappings['inputs']):
        for prop in input:
            parse_prop(True, input_id, prop, aliases)

    for output_id, output in enumerate(mappings['outputs']):
        for prop in output:
            parse_prop(False, output_id, prop, aliases)

    # add directory
    def enumerate_files_by_regex(dir_name):
        for file_name in get_files_by_dir(dir_name):
            m = re.match(r'(?P<number>\d+)\.jpg', file_name)
            if m: yield (file_name, int(m.group('number')))

    def read_image(name):
        # TODO: read_image using opencv
        pass

    # create file-dataset mappings
    datasets_by_file_name = dict()
    for file_name in mappings_by_file_name:
        if os.path.isfile(file_name):
            datasets_by_file_name[file_name] = \
                pd.read_csv(file_name, sep=",", header=None, skiprows=1, dtype=object)
        else:
            file_names_with_order = list(enumerate_files_by_regex(dir_name))

            datasets_by_file_name[dir_name] = \
                [read_image(name) for name, _ in sorted(file_names_with_order, key = lambda t: t[1])]

    # split all datasets to train and test ones
    datasets = datasets_by_file_name.values()
    splits = train_test_split(*datasets, test_size = validation_split)

    # traverse splits and file-mappings to collect input-wise and output-wise datasets
    train_test_splits_by_input_id = dict()
    train_test_splits_by_output_id = dict()

    for file_name, (trainX, testX) in zip(datasets_by_file_name.keys(), pairwise(splits)):
        dataset = datasets_by_file_name[file_name]
        is_type = os.path.isfile(file_name)

        mappings_by_input_id = mappings_by_file_name[file_name][True]
        for input_id in mappings_by_input_id:
            input_shape = m.inputs[input_id].shape[1:]
            
            for mapping in mappings_by_input_id[input_id]:
                (trainXi, testXi) = mapping(input_shape, dataset, trainX, testX)

                (train_aggr, test_aggr) = train_test_splits_by_input_id[input_id] \
                    if input_id in train_test_splits_by_input_id else (None, None)
                    
                train_test_splits_by_input_id[input_id] = (
                    trainXi if train_aggr is None else np.hstack([train_aggr, trainXi]),
                    testXi if test_aggr is None else np.hstack([test_aggr, testXi])) 

        mappings_by_output_id = mappings_by_file_name[file_name][False]
        for output_id in mappings_by_output_id:
            output_shape = m.outputs[output_id].shape[1:]
            
            for mapping in mappings_by_output_id[output_id]:
                (trainXo, testXo) = mapping(output_shape, dataset, trainX, testX)

                (train_aggr, test_aggr) = train_test_splits_by_output_id[output] \
                    if output_id in train_test_splits_by_output_id else (None, None)
                    
                train_test_splits_by_output_id[output_id] = (
                    trainXo if train_aggr is None else np.hstack([train_aggr, trainXo]),
                    testXo if test_aggr is None else np.hstack([test_aggr, testXo]))

    return train_test_splits_by_input_id, train_test_splits_by_output_id


def get_layer_predecessors(layer):
    #print("[DEBUG] predecessors", [prev.inbound_layers for prev in layer._inbound_nodes])
    return [prev.inbound_layers for prev in layer._inbound_nodes]

def train(args):
    m = models.load_model(args.model.name)
    
    print("==== summary ====")
    m.summary()

    try:
        mappings = yaml.full_load(args.mappings)
        train_test_splits_by_input_id, train_test_splits_by_output_id = prepare_data(m, mappings, args.validation_split)

        # train model on defined inputs/outputs
        print("[INFO] training model...")
        m.fit(
            [trainXi for (trainXi, _) in train_test_splits_by_input_id.values()],
            [trainXo for (trainXo, _) in train_test_splits_by_output_id.values()],
            validation_data=(
                [testXi for (_, testXi) in train_test_splits_by_input_id.values()],
                [testXo for (_, testXo) in train_test_splits_by_output_id.values()]),
            epochs = args.epochs,
            batch_size=args.batches,
            verbose=1)

        # save trained model weights
        model_file_name, model_file_ext = os.path.splitext(args.model.name)
        weight_file = model_file_name + "-weights" + model_file_ext
        
        m.save_weights(weight_file)
        print("[INFO] model weights saved", weight_file)

    except yaml.YAMLError as ex:
        print("Error in mappings file:", ex)

    except Exception as ex:
        print("[Error]", ex.__class__.__name__, ex)
        raise ex


def tune(args):
    from kerastuner.tuners import RandomSearch
    from kerastuner.engine.hyperparameters import HyperParameters

    m = models.load_model(args.model.name)

    try:
        mappings = yaml.full_load(args.mappings)
        train_test_splits_by_input_id, train_test_splits_by_output_id = prepare_data(m, mappings, args.validation_split)

        ambiguities = yaml.full_load(args.ambiguities)
        
        # save ambiguities
        hp = HyperParameters()
        amb_mappings_dict = dict()
        for i, ambiguity in enumerate(ambiguities['ambiguities']):
            parse_ambiguity(ambiguity['value'], hp, str(i))

            for mapping in ambiguity['mappings']:
                prop, index = parse_ambiguity_mapping(mapping['prop'])

                if (mapping['name'], prop) not in amb_mappings_dict:
                    amb_mappings_dict[(mapping['name'], prop)] = []
                
                amb_mappings_dict[(mapping['name'], prop)].append((index, str(i)))

        # build model
        def build_model(params):
            # update layer configurations
            layer_map = dict()
            for l in m.layers:
                new_config = l.get_config()

                for config_key in new_config:
                    if (l.name, config_key) in amb_mappings_dict:
                        
                        for (index, mapping_key) in amb_mappings_dict[(l.name, config_key)]:
                            if index is not None:
                                new_config[config_key] = replace_tuple_value(new_config[config_key], index, params.get(mapping_key))
                            else:
                                new_config[config_key] = params.get(mapping_key)
                    pass

                config = {'config': new_config, 'class_name': l.__class__.__name__}
                layer_map[l.name] = layers.deserialize(config)
            
            configured_layers = dict() # name -> layer with inputs
            def process_output(layer):
                if layer.name in configured_layers:
                    return configured_layers[layer.name]

                if isinstance(layer, layers.InputLayer):
                    configured_layers[layer.name] = layer.output
                    return configured_layers[layer.name]

                prevs = [process_output(prev) for prev in get_layer_predecessors(layer)]
                configured_layers[layer.name] = layer_map[layer.name](prevs)

                return configured_layers[layer.name]

            new_outputs = [process_output([l for l in m.layers if l.name == layer.name.split('/')[0]][0]) for layer in m.outputs]
            
            new_model = models.Model(m.inputs, new_outputs)
            new_model.compile(optimizer=m.optimizer, loss=m.loss, loss_weights=m.loss_weights, metrics=m.metrics)
            
            print("==== summary ====")
            new_model.summary()

            return new_model

        t = RandomSearch(
            build_model,
            hyperparameters=hp,
            max_trials=args.trials,
            objective='val_accuracy',
            directory=os.path.normpath('c:/.ngine'),
            overwrite=True)

        print() # empty line
        print("==== search space ====")
        t.search_space_summary()
        t.search(
            x = [trainXi for (trainXi, _) in train_test_splits_by_input_id.values()],
            y = [trainXo for (trainXo, _) in train_test_splits_by_output_id.values()],
            validation_data=(
                [testXi for (_, testXi) in train_test_splits_by_input_id.values()],
                [testXo for (_, testXo) in train_test_splits_by_output_id.values()]),
            epochs = args.epochs)
        
        print() # empty line
        print("==== trial results ====")
        t.results_summary()

        # replace ambiguities
        best_values = t.get_best_hyperparameters()[0]
        best_model = t.get_best_models(num_models=1)[0]
        for i, ambiguity in enumerate(ambiguities['ambiguities']):
            ambiguity['value'] = best_values.values[str(i)]

        print() # empty line
        print("==== best model ====")
        best_model.summary()

        print() # empty line
        print("==== best parameters ====")
        print(yaml.dump(ambiguities))

    except yaml.YAMLError as ex:
        print("Error in mappings file:", ex)

    except Exception as ex:
        print("[Error]", ex.__class__.__name__, ex)
        raise ex



def main(args):
    print("[DEBUG] Using python", sys.version)
    top_level = argparse.ArgumentParser(add_help=True)
    subparsers = top_level.add_subparsers()

    # train command parser
    parser_train = subparsers.add_parser('train')
    parser_train.add_argument('model', type=argparse.FileType('r'))
    parser_train.add_argument('mappings', type=argparse.FileType('r', encoding='utf-8'))
    parser_train.add_argument('epochs', type=int)
    parser_train.add_argument('batches', type=int)
    parser_train.add_argument('validation_split', type=float)
    parser_train.set_defaults(func=train)
    
    # tune command parser
    parser_train = subparsers.add_parser('tune')
    parser_train.add_argument('model', type=argparse.FileType('r'))
    parser_train.add_argument('ambiguities', type=argparse.FileType('r', encoding='utf-8'))
    parser_train.add_argument('mappings', type=argparse.FileType('r', encoding='utf-8'))
    parser_train.add_argument('epochs', type=int)
    parser_train.add_argument('trials', type=int)
    parser_train.add_argument('validation_split', type=float)
    parser_train.set_defaults(func=tune)

    args = top_level.parse_args(args[1:])
    args.func(args)


if __name__ == "__main__":
    main(sys.argv)
