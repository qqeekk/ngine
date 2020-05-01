namespace Ngine.Domain.Schemas

[<RequireQualifiedAccess>]
module Schema =
    [<CLIMutable>]
    type Layer = {
        LayerId: LayerId * LayerId option
        Type: string
        Props: string
    }

    [<CLIMutable>]
    type Head = {
        LayerId: LayerId
        Activation: string
        Loss: string
        LossWeight: float32
    }

    [<CLIMutable>]
    type Ambiguity = {
        Name: string
        Value: string
    }

    [<CLIMutable>]
    type Network = {
        Layers : Layer []
        Heads: Head []
        Optimizer: string
        Ambiguities: Ambiguity []
    }

    [<CLIMutable>]
    type AmbiguityMapRecord = {
        Name: LayerId
        Prop: string
    }
        
    [<CLIMutable>]
    type AmbiguityMapValue = {
        Value: string
        Mappings: AmbiguityMapRecord[]
    }
        
    [<CLIMutable>]
    type AmbiguityMapProduct = {
        Ambiguities: AmbiguityMapValue[]
    }