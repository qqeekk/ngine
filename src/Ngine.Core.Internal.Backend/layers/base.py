from abc import ABC, abstractmethod
from custom_activations import locator


class BaseLayer(ABC):
    def __init(self, layer):
        self.activation = locator.activator_generators(layer['activator'])

    @abstractmethod
    def generate_from_schema(self):
        pass
