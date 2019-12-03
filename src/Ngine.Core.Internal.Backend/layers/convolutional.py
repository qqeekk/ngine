import re
from keras.layers import Convolution2D

# Local imports.
from layers.base import BaseLayer


class Convolutional(BaseLayer):
    def __init__(self, layer):
        super().__init__(self, layer)

        # Match 0:[0x0] pattern.
        m = re.fullmatch(r'(?P<filters>\d+):\[(?P<width>\d+)x(?P<height>\d+)]', layer['neurons'])

        # Split into values.
        self.filters = int(m.group('filters'))
        self.kernel_size = int(m.group('width')), int(m.group('height'))

    def generate_from_schema(self):
        return Convolution2D(self.filters, self.kernel_size, activation=self.activation)
