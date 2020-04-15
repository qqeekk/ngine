namespace Ngine.Backend.Converters
open Ngine.Domain.Schemas.Expressions
open Ngine.Domain.Schemas
open Ngine.Domain.Utils
open System
open Ngine.Backend.Resources.Properties

module ActivatorConverter =
    let private mappings = BijectiveMap [|
        (Recources.Activators_sigmoid, Sigmoid)
        (Recources.Activators_relu, ReLu)
    |]

    let private encode (NotNull "activator" activator) =
        match activator with
        | QuotedFunction quote -> mappings.TryGetLeft quote |> Option.get

    let private decode (NotNull "schema" schema) =
        match mappings.TryGetRight schema with
        | Some func -> Ok (QuotedFunction func)
        | None -> Error [|{
            Position = (0u, uint32 schema.Length)
            Message = Message <| String.Format(Recources.ActivatorConverter_UndefinedFunctionMessage_1s, schema) }|]

    let instance = {
        new IActivatorConverter with
            member _.Encode activator  = encode activator
            member _.Decode schema = decode schema
            member _.QuotedFunctionNames with get() = mappings |> Seq.map (fst) |> Seq.toArray }

    let internal keras = function
    | QuotedFunction(Sigmoid) -> "sigmoid"
    | QuotedFunction(ReLu) -> "relu"
