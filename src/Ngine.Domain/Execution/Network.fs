namespace Ngine.Domain.Execution
open Ngine.Domain
open System.Threading
open System.Threading.Tasks

type INetwork =
    abstract member Train: inputFile:string -> batch:uint32 -> epochs:uint32 -> validationSplit:float -> cancellationToken:CancellationToken -> Task
    abstract member Predict: inputFile:string -> weights:string -> cancellationToken:CancellationToken -> Task

type INetworkGenerator =
    abstract member SaveModel: definition : Schemas.Network -> string
    abstract member Instantiate: fileName : string -> INetwork
