namespace Ngine.Domain.Execution
open Ngine.Domain

type INetwork =
    abstract member Train: inputs : double[] -> expected : double[] -> unit
    abstract member Predict: inputs : double[] -> double[]

type INetworkGenerator =
    abstract member GenerateFromSchema: definition : Schemas.Network -> INetwork
