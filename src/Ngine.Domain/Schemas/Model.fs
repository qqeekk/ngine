namespace Ngine.Domain.Schemas
open Ngine.Domain.Schemas.Expressions
open System.Collections.Generic

type Vector2D<'a> = 'a * 'a
type LayerId = uint32 * uint32
type LayerConnection = LayerId * LayerId option

type Range<'a> = {
    start: 'a
    step: 'a
    ``end``: 'a
}

type Values<'a> =
    | Range of Range<'a>
    | List of 'a []

[<RequireQualifiedAccess>]
module Vector2D =
    let map mapping ((x,y):Vector2D<'a>) = Vector2D (mapping x, mapping y)

type Vector3D<'a> = 'a * 'a * 'a

[<RequireQualifiedAccess>]
module Vector3D =
    let map mapping ((x,y,z):Vector3D<'a>) = Vector3D (mapping x, mapping y, mapping z)

type AmbiguityVariableName = Variable of string

type Ambiguous<'a> =
    | Fixed of 'a
    | RefName of AmbiguityVariableName

module Ambiguous =
    let value (ambiguities:IDictionary<AmbiguityVariableName, Values<'a>>) = function
    | Fixed a -> a
    | RefName name -> 
        match ambiguities.[name] with
        | List a -> a.[0]
        | Range { start = s } -> s

type Activator =
    | QuotedFunction of QuotedFunction
    // TODO: | Polynomial of Polynomial

type PoolingType =
    | Max
    | Avg

type Padding =
    | Same
    | Zero // valid

/// {{ int_channels }}:[{{ int_i1 }}x{{ int_i2 }}x{{ int_i3 }}]
type Sensor3D = {
    Channels: uint32
    Inputs: Vector3D<uint32>
}

/// {{ int_channels }}:[{{ int_i1 }}x{{ int_i2 }}]
type Sensor2D = {
    Channels: uint32
    Inputs: Vector2D<uint32>
}

/// {{ int_batch }}:{{ int_inputs }}
type Sensor1D = {
    Inputs: uint32
}

/// {{ int_filters }}:[{{ int_k1 }}x{{ int_k2 }}](, strides = [{{ int_s1 }}x{{ int_s2 }}])?(, padding = [{{ int_p1 }}x{{ int_p2 }}])?
type Convolutional2D = {
    Filters: Ambiguous<uint32>
    Kernel: Vector2D<Ambiguous<uint32>>
    Strides: Vector2D<Ambiguous<uint32>>
    Padding: Padding
}

/// {{ int_filters }}:[{{ int_k1 }}x{{ int_k2 }}x{{ int_k3 }}](, strides = [{{ int_s1 }}x{{ int_s2 }}x{{ int_s3 }}])?(, padding = [{{ int_p1 }}x{{ int_p2 }}]x{{ int_p3 }})?
type Convolutional3D = {
    Filters: Ambiguous<uint32>
    Kernel: Vector3D<Ambiguous<uint32>>
    Strides: Vector3D<Ambiguous<uint32>>
    Padding: Padding
}

/// {{ max | avg | min }}:[{{ int_k1 }}x{{ int_k2 }}]
type Pooling2D = {
    Kernel: Vector2D<Ambiguous<uint32>>
    Strides: Vector2D<Ambiguous<uint32>>
    PoolingType: PoolingType
}

/// {{ max | avg | min }}:[{{ int_k1 }}x{{ int_k2 }}x{{ int_k3 }}]
type Pooling3D = {
    Kernel: Vector3D<Ambiguous<uint32>>
    Strides: Vector3D<Ambiguous<uint32>>
    PoolingType: PoolingType
}

/// {{ int_units }}
type Dense = {
    Units: Ambiguous<uint32>
}

type HeadLayer<'T> =
    | HeadLayer of LayerId * 'T

type NonHeadLayer<'TLayer, 'TSensor> =
    | Layer of HeadLayer<'TLayer>
    | Sensor of LayerId * 'TSensor

[<ReferenceEquality>]
type Layer3D =
    | Concatenation3D of NonHeadLayer<Layer3D, Sensor3D>[]
    | Conv3D of Convolutional3D * NonHeadLayer<Layer3D, Sensor3D>
    | Pooling3D of Pooling3D * NonHeadLayer<Layer3D, Sensor3D>
    | Activation3D of Activator * NonHeadLayer<Layer3D, Sensor3D>
    | Empty3D

[<ReferenceEquality>]
type Layer2D =
    | Concatenation2D of NonHeadLayer<Layer2D, Sensor2D>[]
    | Conv2D of Convolutional2D * NonHeadLayer<Layer2D, Sensor2D>
    | Pooling2D of Pooling2D * NonHeadLayer<Layer2D, Sensor2D>
    | Activation2D of Activator * NonHeadLayer<Layer2D, Sensor2D>
    | Empty2D

[<ReferenceEquality>]
type Layer1D =
    | Flatten3D of NonHeadLayer<Layer3D, Sensor3D>
    | Flatten2D of NonHeadLayer<Layer2D, Sensor2D>
    | Concatenation1D of NonHeadLayer<Layer1D, Sensor1D>[]
    | Dropout of float32 * NonHeadLayer<Layer1D, Sensor1D>
    | Dense of Dense * NonHeadLayer<Layer1D, Sensor1D>
    | Activation1D of Activator * NonHeadLayer<Layer1D, Sensor1D>
    | Empty1D

type HeadLayer =
    | D3 of HeadLayer<Layer3D>
    | D2 of HeadLayer<Layer2D>
    | D1 of HeadLayer<Layer1D>

type Loss =
    | MSE
    | BCE
    | CE

type Head =
    | Softmax of float32 * Loss * HeadLayer<Layer1D> // Classification
    | Activator of float32 * Loss * HeadLayer * Activator // Activator conversion

type SGD = {
    momentum: float32
    decay: float32 }

type RMSProp = {
    rho: float32
    decay: float32 }

type Adam = {
    beta1: float32
    beta2: float32
    decay: float32 }

type Optimizer =
    | RMSProp of float32 * RMSProp
    | SGD of float32 * SGD
    | Adam of float32 * Adam

type Network = {
    Heads: Head []
    Optimizer: Optimizer
    Ambiguities: IDictionary<AmbiguityVariableName, Values<uint32>>
}
