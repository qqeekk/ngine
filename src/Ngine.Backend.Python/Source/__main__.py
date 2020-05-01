from sklearn.preprocessing import *
from sklearn.model_selection import train_test_split
import pandas as pd
import numpy as np
import cv2
import argparse
import yaml, re
import sys, os
from itertools import *
import keras

def pairwise(iterable):
    "s -> (s0,s1), (s1,s2), (s2, s3), ..."
    a, b = tee(iterable)
    next(b, None)
    return zip(a, b)

# d | d:d |: | d: | :d
prop_regex = r"^(?P<type>(lin|cats|cons|img)):(\$(?P<ref>\w+)(\[(?P<start>\d+)?(?P<range>:(?P<last>\d+)?)?\])?)$"

mappings_by_file_name = dict() # string -> bool -> id -> [shape, df, train, test -> train_x, test_x]
mappings_by_dir_name = dict() # string -> bool -> id -> [shape, train, test -> train_x, test_x]

def convert_df_col_to_categorical(df, train, test, label_index):
    zipBinarizer = LabelBinarizer().fit(df[label_index])
    trainCategorical = zipBinarizer.transform(train[label_index])
    testCategorical = zipBinarizer.transform(test[label_index])

    print("[DEBUG] column {} of {} categories: {}".format(label_index, len(zipBinarizer.classes_), zipBinarizer.classes_))
    return trainCategorical, testCategorical


def convert_df_cols_to_continuous(train, test, label_indices):
    cs = MinMaxScaler()
    trainContinuous = cs.fit_transform(train[label_indices])
    testContinuous = cs.transform(test[label_indices])

    print("[DEBUG] column {} of values from {} to {}".format(label_indices, len(cs.data_min_), cs.data_max_))
    return trainContinuous, testContinuous


def process_single_df_col(type, index):
    index = int(index)

    if type == 'lin':
        return lambda _, df, train, test: (train[index], test[index])
    elif type == 'cats':
        return lambda _, df, train, test: convert_df_col_to_categorical(df, train, test, index)
    elif type == 'cons':
        return lambda _, df, train, test: convert_df_cols_to_continuous(train, test, index)
    else:
        raise Exception("Could not convert one csv column data ({}) to image", index)


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
    
    else:
        def unsqueeze_images(shape, df, train, test):
            indices = get_index_range(df)

            print("[DEBUG] columns {} of {} images".format(indices, shape))
            return [v.reshape(*shape) for v in train[indices].values], [v.reshape(*shape) for v in test[indices].values]

        return lambda shape, df, train, test: unsqueeze_images(shape, df, train, test)


def process_image(shape, image):
    return cv2.resize(image, shape)


def process_single_col(mappings, is_file, type, index):
    if is_file:
        mappings.append(process_single_df_col(type, index))
    else:
        # directory (images only)
        raise Exception("Unable to index a dataset of type: directory")


def process_multi_col(mappings, is_file, type, start, end):
    if is_file:
        mappings.append(process_multi_df_cols(type, start, end))
    else:
        # directory (images only)
        mappings.append(process_image)



def parse_prop(input_or_head, id, prop, aliases):
    m = re.match(prop_regex, prop)
            
    if m is not None:
        obj = m.groupdict()
        file, is_file = aliases[obj["ref"]]

        if is_file:
            if file not in mappings_by_file_name:
                mappings_by_file_name[file] = { input_or_head: { id: [] }, not input_or_head: dict() }
            else:
                mappings_by_file_name[file][input_or_head][id] = []
            
        elif file not in mappings_by_dir_name:
            mappings_by_dir_name[file] = { input_or_head: { id: [] }, not input_or_head: dict() }
        else:
            mappings_by_dir_name[file][input_or_head][id] = []

        mappings = mappings_by_file_name[file][input_or_head][id] if is_file \
            else mappings_by_dir_name[file][input_or_head][id]

        if obj["range"] is None:
            if obj["start"] is not None:
                process_single_col(mappings, is_file, obj["type"], obj["start"])
            elif not is_file:
                mappings.append(process_image)
            else:
                raise Exception("Unexpected empty brackets for file mappings: {}. If you want to get all properties of csv data frame, use [:] instead".format(prop))
        else:
            process_multi_col(mappings, is_file, obj["type"], obj["start"], obj["last"])

        # print(obj)
    else:
        raise Exception("Input {} does not match the pattern {}".format(prop, prop_regex))


def traverse_file_aliases(files):
    file_aliases = dict()
    for a in files:
        file_or_dir = files[a]

        if os.path.exists(file_or_dir):
            file_aliases[a] = (file_or_dir, os.path.isfile(file_or_dir))
        else:
            raise Exception('File {} does not exist'.format(file_or_dir))
        pass

    return file_aliases


def train(args):
    m = keras.models.load_model(args.model.name)
    
    print("==== summary ====")
    m.summary()

    try:
        mappings = yaml.full_load(args.mappings)
        aliases = traverse_file_aliases(mappings['files'])
        
        for input_id, input in enumerate(mappings['inputs']):
            for prop in input:
                parse_prop(True, input_id, prop, aliases)

        for output_id, output in enumerate(mappings['outputs']):
            for prop in output:
                parse_prop(False, output_id, prop, aliases)

        #print("==== mappings ====")
        #print("[DEBUG]", mappings_by_file_name)

        # create file-dataset mappings
        datasets_by_file_name = dict()
        for file_name in mappings_by_file_name:
            datasets_by_file_name[file_name] = pd.read_csv(file_name, sep=",", header=None, skiprows=1, dtype=object)

        # split all datasets to train and test ones
        splits = train_test_split(*datasets_by_file_name.values(), test_size = args.validation_split)

        # traverse splits and file-mappings to collect input-wise and output-wise datasets
        train_test_splits_by_input_id = dict()
        train_test_splits_by_output_id = dict()

        for file_name, (trainX, testX) in zip(datasets_by_file_name.keys(), pairwise(splits)):

            mappings_by_input_id = mappings_by_file_name[file_name][True]
            for input_id in mappings_by_input_id:
                input_shape = m.inputs[input_id].shape[1:]
                print("[INFO] input {} shape is {}".format(input_id, input_shape))
                
                for mapping in mappings_by_input_id[input_id]:
                    (trainXi, testXi) = mapping(input_shape, datasets_by_file_name[file_name], trainX, testX)

                    (train_aggr, test_aggr) = train_test_splits_by_input_id[input_id] \
                        if input_id in train_test_splits_by_input_id else (None, None)
                        
                    train_test_splits_by_input_id[input_id] = (
                        trainXi if train_aggr is None else np.hstack([train_aggr, trainXi]),
                        testXi if test_aggr is None else np.hstack([test_aggr, testXi]))

            mappings_by_output_id = mappings_by_file_name[file_name][False]
            for output_id in mappings_by_output_id:
                output_shape = m.outputs[output_id].shape[1:]
                print("[INFO] output {} shape is {}".format(output_id, output_shape))
                
                for mapping in mappings_by_output_id[output_id]:
                    (trainXo, testXo) = mapping(output_shape, datasets_by_file_name[file_name], trainX, testX)

                    (train_aggr, test_aggr) = train_test_splits_by_output_id[output] \
                        if output_id in train_test_splits_by_output_id else (None, None)
                        
                    train_test_splits_by_output_id[output_id] = (
                        trainXo if train_aggr is None else np.hstack([train_aggr, trainXo]),
                        testXo if test_aggr is None else np.hstack([test_aggr, testXo]))

        # train model on defined inputs/outputs
        print("[INFO] training model...")
        m.fit(
            [trainXi for (trainXi, _) in train_test_splits_by_input_id.values()],
            [trainXo for (trainXo, _) in train_test_splits_by_output_id.values()],
            validation_data=(
                [testXi for (_, testXi) in train_test_splits_by_input_id.values()],
                [testXo for (_, testXo) in train_test_splits_by_output_id.values()]),
            epochs = args.epochs,
            batch_size = args.batches)

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


def main(args):
    # print("python worker started...")
    top_level = argparse.ArgumentParser(add_help=True)
    subparsers = top_level.add_subparsers()

    # train command parser
    parser_train = subparsers.add_parser('train')
    parser_train.add_argument('model', type=argparse.FileType('r'))
    parser_train.add_argument('mappings', type=argparse.FileType('r'))
    parser_train.add_argument('epochs', type=int)
    parser_train.add_argument('batches', type=int)
    parser_train.add_argument('validation_split', type=float)
    parser_train.set_defaults(func=train)

    args = top_level.parse_args(args[1:])
    args.func(args)


if __name__ == "__main__":
    main(sys.argv)
