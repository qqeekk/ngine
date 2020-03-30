namespace Ngine.Domain.Schemas

[<RequireQualifiedAccess>]
module Raw =

    module LayerTypes =
        [<Literal>] 
        let conv2D = "conv2D"

        [<Literal>] 
        let conv3D = "conv3D"
 
        [<Literal>] 
        let dense = "dense"

    [<CLIMutable>]
    type Layer = {
         Type : string
         Neurons : string
         Activator : string
    }

    [<CLIMutable>]
    type Network = {
        Layers : Layer []
    }