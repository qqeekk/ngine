namespace Ngine.Backend.Converters

open Ngine.Domain.Schemas
open Ngine.Domain.Utils
open System.Text.RegularExpressions

[<ReferenceEquality>]
type OptimizerEncoder = {
    pretty: Pretty
    decode: string -> Result<Optimizer option, ValueOutOfRangeInfo[]>
}

module private SGDEncoder =
    let private m = {|
        learningRate = FloatEncoder.encoder
        momentum = FloatEncoder.encoder
        decay = FloatEncoder.encoder |}

    let private stringify name createOptional (p:Printer) = 
        let lr = p.[nameof m.learningRate]
        let d = p.[nameof m.decay]
        let m = p.[nameof m.momentum]
        
        sprintf "%s\(%s\)" name lr
        + createOptional (", momentum=" + m)
        + createOptional (", decay=" + d)

    let private mkRegex name =
        (seq {
            nameof m.learningRate, m.learningRate.regex
            nameof m.momentum, m.momentum.regex
            nameof m.decay, m.decay.regex }, None)
        ||> eval (stringify name regOptional >> regComplete)
    
    let private mkPretty prettyname =
        seq {
            nameof m.learningRate, m.learningRate.pretty
            nameof m.momentum, m.momentum.pretty
            nameof m.decay, m.decay.pretty }
        |> pretty prettyname (mkRegex prettyname) (stringify prettyname regOptional >> Regex.Unescape)

    let private decode name =
        tryDecodeByRegex (mkRegex name) <| fun groups ->
            let learningRate = m.learningRate.decode groups 0u (nameof m.learningRate)
            let momentum = m.momentum.decode groups 0u (nameof m.momentum)
            let decay = m.decay.decode groups 0u (nameof m.decay)
            
            match ResultExtensions.zip3 learningRate momentum decay with
            | Ok (Some learningRate, momentum, decay) ->
                Ok (Some <| SGD (learningRate, {
                    momentum = Option.defaultValue 0f momentum
                    decay = Option.defaultValue 0f decay }))
            | Ok _ -> Ok None
            | Error errors -> Error (errors |> Seq.toArray)

    let private mkEncoder name = {
        pretty = mkPretty name
        decode = decode name }
    
    let encoder = mkEncoder "sgd"

    let encode (lr, sgd:SGD) =
        (seq {
            nameof m.learningRate, fun _ -> m.learningRate.encode lr
            nameof m.momentum, fun _ -> m.momentum.encode sgd.momentum
            nameof m.decay, fun _ -> m.decay.encode sgd.decay }, None)
        ||> eval (stringify encoder.pretty.name id >> Regex.Unescape)

module private RMSPropEncoder =
    let private m = {|
        learningRate = FloatEncoder.encoder
        rho = FloatEncoder.encoder
        decay = FloatEncoder.encoder |}

    let private stringify name createOptional (p:Printer) = 
        let lr = p.[nameof m.learningRate]
        let rho = p.[nameof m.rho]
        let d = p.[nameof m.decay]
        
        sprintf "%s\(%s\), rho=%s" name lr rho
        + createOptional (", decay=" + d)

    let private mkRegex name =
        (seq {
            nameof m.learningRate, m.learningRate.regex
            nameof m.rho, m.rho.regex
            nameof m.decay, m.decay.regex }, None)
        ||> eval (stringify name regOptional >> regComplete)
    
    let private mkPretty prettyname =
        seq {
            nameof m.learningRate, m.learningRate.pretty
            nameof m.rho, m.rho.pretty
            nameof m.decay, m.decay.pretty }
        |> pretty prettyname (mkRegex prettyname) (stringify prettyname regOptional >> Regex.Unescape)

    let private decode name =
        tryDecodeByRegex (mkRegex name) <| fun groups ->
            let learningRate = m.learningRate.decode groups 0u (nameof m.learningRate)
            let rho = m.rho.decode groups 0u (nameof m.rho)
            let decay = m.decay.decode groups 0u (nameof m.decay)
            
            match ResultExtensions.zip3 learningRate rho decay with
            | Ok (Some learningRate, Some rho, decay) ->
                Ok (Some <| RMSProp (learningRate, {
                    rho = rho
                    decay = Option.defaultValue 0f decay }))
            | Ok _ -> Ok None
            | Error errors -> Error (errors |> Seq.toArray)

    let private mkEncoder name = {
        pretty = mkPretty name
        decode = decode name }
    
    let encoder = mkEncoder "rmsProp"

    let encode (lr, rms:RMSProp) =
        (seq {
            nameof m.learningRate, fun _ -> m.learningRate.encode lr
            nameof m.rho, fun _ -> m.rho.encode rms.rho
            nameof m.decay, fun _ -> m.decay.encode rms.decay }, None)
        ||> eval (stringify encoder.pretty.name id >> Regex.Unescape)

module private AdamEncoder =
    let private m = {|
        learningRate = FloatEncoder.encoder
        beta1 = FloatEncoder.encoder
        beta2 = FloatEncoder.encoder
        decay = FloatEncoder.encoder |}

    let private stringify name createOptional (p:Printer) = 
        let lr = p.[nameof m.learningRate]
        let beta1 = p.[nameof m.beta1]
        let beta2 = p.[nameof m.beta2]
        let d = p.[nameof m.decay]
        
        sprintf "%s\(%s, %s, %s\)" name lr beta1 beta2
        + createOptional (", decay=" + d)

    let private mkRegex name =
        (seq {
            nameof m.learningRate, m.learningRate.regex
            nameof m.beta1, m.beta1.regex
            nameof m.beta2, m.beta2.regex
            nameof m.decay, m.decay.regex }, None)
        ||> eval (stringify name regOptional >> regComplete)
    
    let private mkPretty prettyname =
        seq {
            nameof m.learningRate, m.learningRate.pretty
            nameof m.beta1, m.beta1.pretty
            nameof m.beta2, m.beta2.pretty
            nameof m.decay, m.decay.pretty }
        |> pretty prettyname (mkRegex prettyname) (stringify prettyname regOptional >> Regex.Unescape)

    let private decode name =
        tryDecodeByRegex (mkRegex name) <| fun groups ->
            let learningRate = m.learningRate.decode groups 0u (nameof m.learningRate)
            let beta1 = m.beta1.decode groups 0u (nameof m.beta1)
            let beta2 = m.beta2.decode groups 0u (nameof m.beta2)
            let decay = m.decay.decode groups 0u (nameof m.decay)
            
            match ResultExtensions.zip4 learningRate beta1 beta2 decay with
            | Ok (Some learningRate, Some beta1, Some beta2, decay) ->
                Ok (Some <| Adam (learningRate, {
                    beta1 = beta1
                    beta2 = beta2
                    decay = Option.defaultValue 0f decay }))
            | Ok _ -> Ok None
            | Error errors -> Error (errors |> Seq.toArray)

    let private mkEncoder name = {
        pretty = mkPretty name
        decode = decode name }
    
    let encoder = mkEncoder "adam"

    let encode (lr, adam:Adam) =
        (seq {
            nameof m.learningRate, fun _ -> m.learningRate.encode lr
            nameof m.beta1, fun _ -> m.beta1.encode adam.beta1
            nameof m.beta2, fun _ -> m.beta1.encode adam.beta2
            nameof m.decay, fun _ -> m.decay.encode adam.decay }, None)
        ||> eval (stringify encoder.pretty.name id >> Regex.Unescape)

module OptimizerConverter =
    let private mappings = [|
        SGDEncoder.encoder
        RMSPropEncoder.encoder
        AdamEncoder.encoder
    |]

    let private decode (NotNull "schema" schema) =
        mappings
        |> Seq.tryPick (fun encoder -> 
            match encoder.decode schema with
            | Ok activation -> Option.map Ok activation
            | Error error -> Some (Error <| ValuesOutOfRange error))
        |> Option.defaultWith (fun () ->
            let pretties = mappings |> Seq.map (fun encoder -> encoder.pretty) |> List.ofSeq
            Error <| PropsPatternMissmatch { Pattern = { name = "optimizer"; defn = None; regex = ""; deps = pretties }}
        )
        
    let encode = function
        | Optimizer.RMSProp(lr, x) -> RMSPropEncoder.encode (lr, x)
        | Optimizer.SGD(lr, x) -> SGDEncoder.encode (lr, x)
        | Optimizer.Adam(lr, x) -> AdamEncoder.encode (lr, x)

    let instance = {
        new IOptimizerConverter with
            member _.Encode optimizer  = encode optimizer
            member _.Decode schema = decode schema
            member _.OptimizerNames with get() =
                mappings
                |> Seq.map (fun encoder -> encoder.pretty)
                |> Seq.toArray }

    let internal keras = function
        | Optimizer.RMSProp(lr, x) -> new Keras.Optimizers.RMSprop(lr, x.rho, decay = x.decay) :> Keras.Base
        | Optimizer.SGD(lr, x) -> new Keras.Optimizers.SGD(lr, x.momentum, decay = x.decay) :> Keras.Base
        | Optimizer.Adam(lr, x) -> new Keras.Optimizers.Adam(lr, x.beta1, x.beta2, decay = x.decay) :> Keras.Base
