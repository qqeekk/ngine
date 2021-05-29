namespace Ngine.Backend.Converters
open Ngine.Domain.Utils
open Ngine.Domain.Schemas

module LossConverter =
    let private mappings = BijectiveMap [|
        BCE, "bce"
        CE, "ce"
        MSE, "mse"
    |]

    let private encode = mappings.TryGetRight >> Option.get
    let private decode schema =
        match mappings.TryGetLeft schema with
        | Some loss -> Ok loss
        | None -> Error LossError.UnknownType

    let instance =
        { new ILossConverter with
            member _.DecodeLoss loss = decode loss
            member _.EncodeLoss schema = encode schema
            member _.LossFunctionNames =
                mappings |> Seq.map (fun (_, name) -> { name = name; regex = name; defn = Some name; deps = [] }) |> Seq.toArray }

    let keras = function
        | BCE -> "binary_crossentropy"
        | MSE -> "mean_squared_error"
        | CE -> "categorical_crossentropy"
