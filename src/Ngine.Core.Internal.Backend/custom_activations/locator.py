from keras import activations

# Local imports.
from custom_activations import sigmoid

activator_generators = [
    sigmoid.generate_from_schema
]


def get_activation(identifier):
    try:
        return activations.get(identifier)
    except ValueError:
        for approached_value in [generator(identifier) for generator in activator_generators]:
            if approached_value is not None:
                return approached_value
        pass
