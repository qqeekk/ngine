namespace Ngine.Domain.Schemas
open Ngine.Domain.Schemas.Expressions
open Ngine.Domain.Schemas.Kernels

type Activator =
    | QuotedFunction of QuotedFunction
    // TODO: | Polynomial of Polynomial

type Kernel =
    | Conv2D of Convolutional2DKernel
    | Conv3D of Convolutional3DKernel
    | Dense

[<CLIMutable>]
type Layer = {
    NeuronsTotal : uint32
    Activator : Activator
    Kernel : Kernel
}

[<CLIMutable>]
type Network = {
    Layers : Layer []
}
