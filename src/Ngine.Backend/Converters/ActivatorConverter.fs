namespace Ngine.Backend.Converters
open Ngine.Domain.Schemas.Conversions
open Ngine.Domain.Schemas.Expressions
open Ngine.Domain.Schemas

module Functions =
    [<Literal>] 
    let sigmoid = "sigmoid"

    [<Literal>] 
    let relu = "relu"


type ActivatorConverter() =
    let encode activator =
        match activator with
        | QuotedFunction(Sigmoid) -> Functions.sigmoid
        | QuotedFunction(ReLu) -> Functions.relu

    let decode schema = 
        match schema with
        | Raw.Functions.sigmoid -> Ok (QuotedFunction Sigmoid)
        | Raw.Functions.relu -> Ok (QuotedFunction ReLu)
        | func -> Error [|{ 
            Position = (0u, uint32 schema.Length)
            Message = Message func }|]
    
    interface IActivatorConverter with
        member _.Encode activator = encode activator 
        member _.Decode schema = decode schema