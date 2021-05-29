namespace Ngine.Domain.Schemas.Expressions

type QuotedFunction =
    | Sigmoid
    | HardSigmoid
    | Tanh
    | ReLu
    | ELu of float32
    | LeakyReLu of float32
    | SeLu
