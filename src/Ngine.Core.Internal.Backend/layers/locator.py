# Local imports.
from layers import dense, convolutional

layer_generators = {
    'convolutional': convolutional.Convolutional,
    'sensor/transform': dense.Dense
}


def get_layer(layer):
    return layer_generators[layer['type']](layer)
