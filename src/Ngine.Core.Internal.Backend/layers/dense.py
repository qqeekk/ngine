from keras.layers import Dense

# Local imports.
from layers.base import BaseLayer


class Dense(BaseLayer):
    def __init__(self, layer):
        super().__init__(self, layer)
        self.neurons = layer['neurons']

    def generate_from_schema(self):
        return Dense(self.neurons, activation=self.activation)
