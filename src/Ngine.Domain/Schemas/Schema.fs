namespace Ngine.Domain.Schemas

open System

[<RequireQualifiedAccess>]
module Schema =
    type LayerId = uint32 * uint32

    [<CLIMutable>]
    type Layer = {
        LayerId: LayerId
        PreviousLayerId: LayerId option
        Type: string
        Props: string
    }

    [<CLIMutable>]
    type Head = {
        LayerId: LayerId
        Activation: string
        Loss: string
    }

    [<CLIMutable>]
    type Network = {
        Layers : Layer []
        Heads: Head []
        Optimizer: string
    }
