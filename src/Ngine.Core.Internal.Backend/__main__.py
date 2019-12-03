# Parsing.
import yaml
import sys

# NN.
from keras.models import Sequential

from layers import locator


def parse_yaml(path):
    with open(path, 'r') as stream:
        try:
            return yaml.safe_load(stream)
        except yaml.YAMLError as ex:
            print(ex)
            raise ex


def main():
    model = parse_yaml(sys.argv[1])
    nn = Sequential(map(locator.get_layer, model['layers']))
    print(nn)


if __name__ == '__main__':
    main()
