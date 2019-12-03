import re
from ast import literal_eval as make_tuple

from keras import backend as bk
from keras.layers import Activation


def generate_from_schema(identifier):
    # Consider that postfix after name of function is a valid python tuple.
    custom_sigmoid = re.fullmatch(r'sigmoid(.*)', identifier)

    if custom_sigmoid is not None:
        c, k = make_tuple(custom_sigmoid.group(1))

        print('Custom activation:', f'{c} * sigmoid(x) + {k}')
        return Activation(lambda x: c * bk.sigmoid(x) + k)
    else:
        return None
