namespace Ngine.Backend.Converters
open Ngine.Domain.Schemas.Expressions
open Ngine.Domain.Schemas
open Ngine.Domain.Utils
open System
open Ngine.Backend.Resources.Properties
open Ngine.Domain.Schemas
open System.Text.RegularExpressions
open Keras.Layers

[<ReferenceEquality>]
type ActivationEncoderData = {
    pretty: Pretty
    decode: string -> Result<Activator option, ValueOutOfRangeInfo[]>
}

module QuotedActivationEncoder =
    let private m = {| alpha = FloatEncoder.encoder |}

    let private stringify name (p:Printer) =
        sprintf "%s\(%s\)" name p.[nameof m.alpha]

    let private regex name =
        (seq { nameof m.alpha, m.alpha.regex }, None)
        ||> eval (stringify name >> regComplete)

    let pretty name =
        seq { nameof m.alpha, m.alpha.pretty }
        |> pretty name (regex name) (stringify name >> Regex.Unescape)

    let prettyMisc name =
        { name = name; regex = name; defn = Some name; deps = [] }

    let private decode convert name =
        tryDecodeByRegex (regex name) <| fun groups ->
            m.alpha.decode groups 0u (nameof m.alpha)
            |> Result.map (Option.map (convert >> QuotedFunction))
            |> Result.mapError (Array.singleton)

    let private decodeMisc activation name =
        tryDecodeByRegex name (fun _ -> activation |> QuotedFunction |> Some |> Ok)

    let private encoder decode pretty name = {
        pretty = pretty name
        decode = decode name }

    let encode encoder = function
        | QuotedFunction.ELu a | QuotedFunction.LeakyReLu a ->
            (seq { nameof m.alpha, fun _ -> m.alpha.encode a }, None)
            ||> eval (stringify encoder.pretty.name >> Regex.Unescape)
        | _ -> encoder.pretty.name

    let eluEncoder = encoder (decode QuotedFunction.ELu) pretty Recources.Activators_elu
    let leakyReluEncoder = encoder (decode QuotedFunction.LeakyReLu) pretty Recources.Activators_leakyRelu
    let reluEncoder = encoder (decodeMisc QuotedFunction.ReLu) prettyMisc Recources.Activators_relu
    let seluEncoder = encoder (decodeMisc QuotedFunction.SeLu) prettyMisc Recources.Activators_selu
    let tanhEncoder = encoder (decodeMisc QuotedFunction.Tanh) prettyMisc Recources.Activators_tanh
    let sigmoidEncoder = encoder (decodeMisc QuotedFunction.Sigmoid) prettyMisc Recources.Activators_sigmoid
    let hardSigmoidEncoder = encoder (decodeMisc QuotedFunction.HardSigmoid) prettyMisc Recources.Activators_hardSigmoid

type private HeadFunctionEncoder = {
    pretty: Pretty
    decode: string -> Result<HeadFunction option, PropsConversionError>
}

module private SoftmaxEncoder =
    let private decode name func =
        if func = name
        then HeadFunction.Softmax |> Some |> Ok
        else None |> Ok

    let private mkEncoder name = {
        decode = decode name
        pretty = { name = name; regex = name; defn = Some name; deps = [] } }

    let encoder = mkEncoder "softmax"
    let encoded = encoder.pretty.name

module ActivatorConverter =
    let private mappings = [|
        QuotedActivationEncoder.reluEncoder
        QuotedActivationEncoder.eluEncoder
        QuotedActivationEncoder.leakyReluEncoder
        QuotedActivationEncoder.seluEncoder
        QuotedActivationEncoder.tanhEncoder
        QuotedActivationEncoder.sigmoidEncoder
        QuotedActivationEncoder.hardSigmoidEncoder
    |]

    let private decode (NotNull "schema" schema) =
        mappings
        |> Seq.tryPick (fun encoder ->
            match encoder.decode schema with
            | Ok activation -> Option.map Ok activation
            | Error error -> Some (Error <| ValuesOutOfRange error))
        |> Option.defaultWith (fun () ->
            let pretties = mappings |> Seq.map (fun encoder -> encoder.pretty) |> List.ofSeq
            Error <| PropsPatternMissmatch { Pattern = { name = "activation"; regex = ""; defn = None; deps = pretties }}
        )

    let encode (QuotedFunction activation) =
        let encoder =
            match activation with
            | ReLu _ -> QuotedActivationEncoder.reluEncoder
            | LeakyReLu _ -> QuotedActivationEncoder.leakyReluEncoder
            | ELu _ -> QuotedActivationEncoder.eluEncoder
            | SeLu -> QuotedActivationEncoder.seluEncoder
            | Tanh -> QuotedActivationEncoder.tanhEncoder
            | Sigmoid -> QuotedActivationEncoder.sigmoidEncoder
            | HardSigmoid -> QuotedActivationEncoder.hardSigmoidEncoder

        QuotedActivationEncoder.encode encoder activation

    let encodeHeadFunction = function
        | HeadFunction.Softmax -> SoftmaxEncoder.encoded
        | HeadFunction.Activator a -> encode a

    let decodeHeadFunction func =
        SoftmaxEncoder.encoder.decode func
        |> Result.bind (function
            | Some f -> Ok f
            | None -> decode func |> Result.map (HeadFunction.Activator))
        |> Result.mapError (function
            | PropsPatternMissmatch pretty ->
                { pretty with Pattern = { name = "headFunction"
                                          defn = None
                                          regex = ""
                                          deps = [SoftmaxEncoder.encoder.pretty; pretty.Pattern] }
                } |> PropsPatternMissmatch
            | error -> error)

    let instance = {
        new IActivatorConverter with
            member _.Encode activator  = encode activator
            member _.Decode schema = decode schema
            member _.EncodeHeadActivation activator = encodeHeadFunction activator
            member _.DecodeHeadActivation schema = decodeHeadFunction schema
            member _.ActivationFunctionNames with get() =
                mappings
                |> Seq.map (fun encoder -> encoder.pretty)
                |> Seq.toArray

            member self.HeadFunctionNames with get() =
                Array.append [| SoftmaxEncoder.encoder.pretty |] self.ActivationFunctionNames }

    let internal keras = function
        | QuotedFunction(Sigmoid) -> new Keras.Layers.Activation("sigmoid") :> BaseLayer
        | QuotedFunction(SeLu) -> new Keras.Layers.Activation("selu") :> BaseLayer
        | QuotedFunction(ReLu) -> new Keras.Layers.ReLU() :> BaseLayer
        | QuotedFunction(ELu a) -> new Keras.Layers.ELU(a) :> BaseLayer
        | QuotedFunction(HardSigmoid) -> new Keras.Layers.Activation("hard_sigmoid") :> BaseLayer
        | QuotedFunction(LeakyReLu a) -> new Keras.Layers.LeakyReLU(a) :> BaseLayer
        | QuotedFunction(Tanh) -> new Keras.Layers.Activation("tanh") :> BaseLayer

    let internal kerasHeadActivation = function
        | HeadFunction.Activator a -> keras a
        | HeadFunction.Softmax -> new Keras.Layers.Softmax() :> BaseLayer
