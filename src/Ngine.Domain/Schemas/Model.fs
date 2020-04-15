namespace Ngine.Domain.Schemas
open Ngine.Domain.Schemas.Expressions

type Vector2D = uint32 * uint32
type Vector3D = uint32 * uint32 * uint32

type Activator =
    | QuotedFunction of QuotedFunction
    // TODO: | Polynomial of Polynomial

type PoolingType =
    | Max
    | Avg
    | Min

type Sensor3D = {
    BatchSize: uint32
    Channels: uint32
    Inputs: Vector3D
}

type Sensor2D = {
    BatchSize: uint32
    Channels: uint32
    Inputs: Vector2D
}

type Sensor1D = {
    BatchSize: uint32
    Inputs: uint32
}

type Convolutional2D = {
    Filters: uint32
    Kernel: Vector2D
    Strides: Vector2D
    Padding: Vector2D
}

type Convolutional3D = {
    Filters: uint32
    Kernel: Vector3D
    Strides: Vector3D
    Padding: Vector3D
}

type Pooling2D = {
    Kernel: Vector2D
    PoolingType: PoolingType
}

type Pooling3D = {
    Kernel: Vector3D
    PoolingType: PoolingType
}

type Dense = {
    Neurons: uint32
}

[<ReferenceEquality>]
type Layer3D =
    | Concatenation3D of NonHeadLayer3D[]
    | Conv3D of Convolutional3D * NonHeadLayer3D
    | Pooling3D of Pooling3D * NonHeadLayer3D
    | Activation3D of Activator * NonHeadLayer3D

and NonHeadLayer3D =
    | Layer3D of Layer3D
    | Sensor3D of Sensor3D

[<ReferenceEquality>]
type Layer2D =
    | Concatenation2D of NonHeadLayer2D[]
    | Conv2D of Convolutional2D * NonHeadLayer2D
    | Pooling2D of Pooling2D * NonHeadLayer2D
    | Activation2D of Activator * NonHeadLayer2D

and NonHeadLayer2D =
    | Layer2D of Layer2D
    | Sensor2D of Sensor2D

[<ReferenceEquality>]
type Layer1D =
    | Flatten3D of NonHeadLayer3D
    | Flatten2D of NonHeadLayer2D
    | Concatenation1D of NonHeadLayer1D[]
    | Dropout of float * NonHeadLayer1D
    | Dense of Dense * NonHeadLayer1D
    | Activation1D of Activator * NonHeadLayer1D

and NonHeadLayer1D =
    | Layer1D of Layer1D
    | Sensor1D of Sensor1D

type Layer =
    | D3 of Layer3D
    | D2 of Layer2D
    | D1 of Layer1D

type Loss =
    | MSE
    | BCE
    | CE

type Head =
    | Softmax of Loss * Layer1D // Classification
    | Activator of Loss * Layer * Activator // Activator conversion

type Optimizer =
    | GD
    | GDm
    | RProps
    | RMSProps
    | SGD
    | Adam

type Network = {
    Heads: Head []
    Optimizer: Optimizer
}
