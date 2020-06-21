namespace Ngine.Backend.Converters
open Ngine.Domain.Utils
open Ngine.Domain.Schemas
open Ngine.Domain.Schemas.Errors
open Ngine.Backend.Resources.Properties
open Keras
open System
open System.Text.RegularExpressions
open System.Collections.Generic
open System.Globalization

type PrimitiveEncoder<'a, 'b> = {
    pretty: Pretty
    regex: string -> string
    decode: GroupCollection -> uint32 -> string -> Result<'a option, 'b>
    encode: 'a -> string }


[<AutoOpen>]
module private EncoderPrimitives =
    type Printer = Map<PatternPropertyName, string>

    let regNamed pattern name =
        sprintf "(?<%s>%s)" name pattern

    let regOptional =
        sprintf "(%s)?"

    let regMultiple =
        sprintf "(%s)*"

    let regComplete =
        sprintf "^%s$"

    let prettyPrint ty prop =
        sprintf "{{ %s: %s }}" prop ty

    let combineGroupName =
        sprintf "%s_%s"

    let tryGetCapture (groups : GroupCollection) num (name:string) =
        match groups.[name] with
        | g when not g.Success -> None
        | valid ->
            if valid.Captures.Count > int num then
                Some valid.Captures.[int num]
            else
                None

    let eval create map name =
        let projectPrinted =
            match name with
            | Some name -> combineGroupName name
            | None -> id

        map
        |> Seq.map (fun (key, regex) -> key, projectPrinted key |> regex)
        |> Map.ofSeq
        |> create

    let pretty name regex create map =
        { name = name
          regex = regex
          defn = map
            |> Seq.map (fun (key, pretty) -> key, prettyPrint pretty.name key)
            |> Map.ofSeq
            |> create 
            |> Some
          deps = map
            |> Seq.map snd
            |> Seq.distinct
            |> Seq.toList }

    let tryDecodeByRegex regex map input =
        let regMatch = Regex.Match(input, regex)
            
        match regMatch.Success with
        | true -> map (regMatch.Groups)
        | false -> Ok None

    let mapToError (pretty: Pretty) =
        Result.bind (
            Option.map Ok
            >> Option.defaultValue (Error (PropsPatternMissmatch { Pattern = pretty } )))


module private IntegerEncoder =
    let tryParseNumber prop (num : Capture) =
        match UInt32.TryParse(num.Value)  with
        | true, n -> Ok n
        | false, _ -> Error { 
            Property = prop
            IndicatedValue = num.Value }

    let private decode (groups : GroupCollection) num name =
        tryGetCapture groups num name
        |> Option.map (tryParseNumber name)
        |> function
        | Some result -> Result.map Some result
        | None -> Ok None

    [<Literal>]
    let private regInt = "\d+"
    let encoder = {
        regex = regNamed regInt
        decode = decode
        encode = sprintf "%d"
        pretty = {
            name = "uint";
            regex = regInt
            defn = Some "positive integer"
            deps = [] }}


module private FloatEncoder =
    let tryParseFloat prop (num : Capture) =
        match Single.TryParse(num.Value, NumberStyles.Any, CultureInfo.InvariantCulture) with
        | true, v when  v >= 0.f && v <= 1.f -> Ok v
        | _ -> Error { 
            Property = prop
            IndicatedValue = num.Value }

    let private decode (groups : GroupCollection) num name =
        tryGetCapture groups num name
        |> Option.map (tryParseFloat name)
        |> function
        | Some result -> Result.map Some result
        | None -> Ok None

    [<Literal>]
    let private regFloat = "\d+(\.\d+)?"
    let encoder = {
        regex = regNamed regFloat
        decode = decode
        encode = sprintf "%.5f"
        pretty = {
            name = "ufloat"
            regex = regFloat
            defn = Some "floating point number: (0; 1)"
            deps = [] }}


module private VariableNameEncoder =
    let private decode (groups : GroupCollection) num name =
        tryGetCapture groups num name
        |> Option.map (fun cap -> Variable cap.Value)
        |> Ok

    [<Literal>]
    let private regVariable = "[\-\w]+"
    let encoder = {
        regex = regNamed regVariable
        decode = decode
        encode = fun (Variable n) -> n
        pretty = {
            name = "word";
            regex = regVariable
            defn = Some "word without whitespace"
            deps = [] }}


module private AmbiguousEncoder =
    type private M<'a> = {
        value: PrimitiveEncoder<'a, ValueOutOfRangeInfo>
        ref: PrimitiveEncoder<AmbiguityVariableName, obj>
    }

    let private stringify m valueOptional refOptional (printer:Printer) =
        let value = if valueOptional then printer.[nameof m.value] else ""
        let ref = if refOptional then sprintf "\?\(%s\)" printer.[nameof m.ref] else ""
        
        match valueOptional, refOptional with
        | true, true -> sprintf "(%s|%s)" value ref
        | true, false -> sprintf "%s" value
        | false, true -> sprintf "%s" ref
        | false, false -> ""

    let private regex m = 
        seq { 
            nameof m.value, m.value.regex 
            nameof m.ref, m.ref.regex 
        }
        |> eval (stringify m true true)

    let private pretty m =
        seq {
            nameof m.value, (m.value).pretty
            nameof m.ref, (m.ref).pretty
        }
        |> pretty ("%_" + m.value.pretty.name) (regex m None) (stringify m true true >> Regex.Unescape)

    let private encode m = function
        | Fixed v ->
            (seq {
                nameof m.value, fun _ -> (m.value).encode v
                nameof m.ref, fun _ -> ""
            }, None) ||> eval (stringify m true false)
        | RefName ref ->
            (seq {
                nameof m.value, fun _ -> ""
                nameof m.ref, fun _ -> (m.ref).encode ref
            }, None) ||> eval (stringify m false true)

    let private decode m (groups : GroupCollection) num namePrefix =
        let value =
            combineGroupName namePrefix (nameof m.value)
            |> m.value.decode groups num
    
        let ref =
            combineGroupName namePrefix (nameof m.ref)
            |> m.ref.decode groups num
            |> Result.mapError (fun _ -> { Property = ""; IndicatedValue = "" })

        ResultExtensions.zip value ref
        |> Result.map (function
        | Some value, None -> Some (Fixed value)
        | None, Some ref -> Some (RefName ref)
        | _ -> None)

    let encoder internalEncoder = 
        let m = { value = internalEncoder; ref = VariableNameEncoder.encoder }
        { regex = Some >> regex m
          pretty = pretty m
          encode = encode m
          decode = decode m }


module private CommaSeparatedValuesEncoder =
    type private M<'a, 'b> = {
        first: PrimitiveEncoder<'a, 'b>
        others: PrimitiveEncoder<'a, 'b> }

    let private stringify m multiple optional (printer:Printer) =
        let first = printer.[nameof m.first]
        let others = printer.[nameof m.others]

        optional first + multiple (sprintf ",%s" others)

    let private regex m prefix =
        let pat = 
            (seq {
                nameof m.first, m.first.regex
                nameof m.others, m.others.regex }, prefix)
            ||> eval (stringify m regMultiple regOptional)

        regNamed pat (prefix |> Option.map (fun p -> combineGroupName p "list") |> Option.defaultValue "list")

    let private pretty m =
        seq {
            nameof m.first, m.first.pretty
            nameof m.others, m.others.pretty }
        |> pretty (m.first.pretty.name + " array") (regex m None) (stringify m regMultiple regOptional >> Regex.Unescape)

    let private encode m (ids: 'a []) =
        ids |> Seq.map (m.first.encode) |> String.concat ","
        
    let private decode m (groups : GroupCollection) _ namePrefix =
        let elements = groups.[combineGroupName namePrefix "list"]
        let testRegex = m.first.regex "x"

        let elemMatches = 
            Regex.Matches(elements.Value, testRegex)
            |> Seq.map (fun mh -> m.first.decode mh.Groups 0u "x" |> Result.map Option.get)
            |> Seq.toList
            
        ResultExtensions.aggregate elemMatches
        |> Result.map (Some)

    let encoder elemEncoder =
        let m = { first = elemEncoder; others = elemEncoder }
        { pretty = pretty m
          regex = Some >> regex m
          decode = decode m
          encode = encode m }


module private RangeValuesEncoder =
    type private M<'a> = {
        from: PrimitiveEncoder<'a, ValueOutOfRangeInfo>
        ``end``: PrimitiveEncoder<'a, ValueOutOfRangeInfo>
        step: PrimitiveEncoder<'a, ValueOutOfRangeInfo> }

    let private m encoder = {
        from = encoder
        ``end`` = encoder
        step = encoder }

    let private stringify m (printer:Printer) =
        sprintf "%s:%s:%s" (printer.[nameof m.from]) (printer.[nameof m.``end``]) (printer.[nameof m.step])

    let private mkRegex m = 
        seq {
            nameof m.from, m.from.regex
            nameof m.``end``, m.``end``.regex
            nameof m.step, m.step.regex }
        |> eval (stringify m)

    let private mkPretty m prettyname =
        seq {
            nameof m.from, m.from.pretty
            nameof m.``end``, m.``end``.pretty
            nameof m.step, m.step.pretty }
        |> pretty prettyname (mkRegex m None) (stringify m >> Regex.Unescape)

    let private encode m range =
        (seq { 
            nameof m.from, fun _ -> (m.from).encode (range.start)
            nameof m.``end``, fun _ -> (m.``end``).encode (range.``end``)
            nameof m.step, fun _ -> (m.step).encode (range.step) }, None)
        ||> eval (stringify m >> Regex.Unescape)

    let private decode m (groups : GroupCollection) num prefix =
        let x =
            combineGroupName prefix (nameof m.from)
            |> m.from.decode groups num

        let y =
            combineGroupName prefix (nameof m.``end``)
            |> m.``end``.decode groups num

        let z =
            combineGroupName prefix (nameof m.step)
            |> m.step.decode groups num

        ResultExtensions.zip3 x y z
        |> Result.map (function
        | Some x, Some y, Some z -> Some { start = x; step = z; ``end`` = y }
        | _ -> None)
        |> Result.mapError (List.toArray)

    let private mkEncoder m prettyname = {
        regex = Some >> mkRegex m
        encode = encode m
        pretty = mkPretty m prettyname
        decode = decode m }

    let encoder internalEncoder =
        mkEncoder (m internalEncoder) (internalEncoder.pretty.name + " range")


module private Vector2DEncoder =
    type private M<'a> = {
        x: PrimitiveEncoder<'a, ValueOutOfRangeInfo list>
        y: PrimitiveEncoder<'a, ValueOutOfRangeInfo list> }
    
    let private m encoder = {
        x = encoder
        y = encoder }

    let private stringify mx (printer:Printer) =
        sprintf "\[%sx%s\]" (printer.[nameof mx.x]) (printer.[nameof mx.y])

    let private mkRegex mx = 
        seq {
            nameof mx.x, mx.x.regex
            nameof mx.y, mx.y.regex }
        |> eval (stringify mx)

    let private mkPretty mx prettyname =
        seq {
            nameof mx.x, mx.x.pretty
            nameof mx.y, mx.y.pretty }
        |> pretty prettyname (mkRegex mx None) (stringify mx >> Regex.Unescape)

    let private encode mx (x,y) =
        (seq { 
            nameof mx.x, fun _ -> (mx.x).encode x
            nameof mx.y, fun _ -> (mx.y).encode y }, None)
        ||> eval (stringify mx >> Regex.Unescape)

    let private decode mx create (groups : GroupCollection) num prefix =
        let x =
            combineGroupName prefix (nameof mx.x)
            |> mx.x.decode groups num
    
        let y =
            combineGroupName prefix (nameof mx.y)
            |> mx.y.decode groups num

        ResultExtensions.zip x y
        |> Result.map (function
        | Some x, Some y -> Some (create x y)
        | _ -> None)
        |> Result.mapError (List.concat)

    let private mkEncoder m prettyname create = {
        regex = Some >> mkRegex m
        encode = encode m
        pretty = mkPretty m prettyname
        decode = decode m create }

    let encoder internalEncoder =
        mkEncoder (m internalEncoder) (internalEncoder.pretty.name + "_vector2D")
        <| fun x y -> Vector2D(x,y)


module private Vector3DEncoder =
    type private M<'a> = {
        x: PrimitiveEncoder<'a, ValueOutOfRangeInfo list>
        y: PrimitiveEncoder<'a, ValueOutOfRangeInfo list>
        z: PrimitiveEncoder<'a, ValueOutOfRangeInfo list> }

    let private m encoder = {
        x = encoder
        y = encoder
        z = encoder }

    let private stringify mx (printer:Printer) =
        sprintf "\[%sx%sx%s\]" (printer.[nameof mx.x]) (printer.[nameof mx.y]) (printer.[nameof mx.z])

    let private mkRegex mx = 
        seq {
            nameof mx.x, mx.x.regex
            nameof mx.y, mx.y.regex
            nameof mx.z, mx.z.regex }
        |> eval (stringify mx)

    let private mkPretty mx prettyname =
        seq {
            nameof mx.x, mx.x.pretty
            nameof mx.y, mx.y.pretty
            nameof mx.z, mx.z.pretty }
        |> pretty prettyname (mkRegex mx None) (stringify mx >> Regex.Unescape)

    let private encode mx (x,y,z) =
        (seq { 
            nameof mx.x, fun _ -> (mx.x).encode x
            nameof mx.y, fun _ -> (mx.y).encode y
            nameof mx.z, fun _ -> (mx.z).encode z }, None)
        ||> eval (stringify mx >> Regex.Unescape)

    let private decode mx create (groups : GroupCollection) num prefix =
        let x =
            combineGroupName prefix (nameof mx.x)
            |> mx.x.decode groups num

        let y =
            combineGroupName prefix (nameof mx.y)
            |> mx.y.decode groups num

        let z =
            combineGroupName prefix (nameof mx.z)
            |> mx.z.decode groups num

        ResultExtensions.zip3 x y z
        |> Result.map (function
        | Some x, Some y, Some z -> Some (create x y z)
        | _ -> None)
        |> Result.mapError (List.concat)

    let private mkEncoder m prettyname create = {
        regex = Some >> mkRegex m
        encode = encode m
        pretty = mkPretty m prettyname
        decode = decode m create }

    let encoder internalEncoder =
        mkEncoder (m internalEncoder) (internalEncoder.pretty.name + "_vector3D")
        <| fun x y z -> Vector3D(x,y,z)


// Domain-specific

module LayerIdEncoder =
    let private m = {|
        row = IntegerEncoder.encoder
        col = IntegerEncoder.encoder |}
    
    let private stringify (printer:Printer) =
        sprintf "%s-%s" (printer.[nameof m.row]) (printer.[nameof m.col])

    let private mkRegex = 
        seq {
            nameof m.row, m.row.regex
            nameof m.col, m.col.regex }
        |> eval (stringify)

    let private mkPretty prettyname =
        seq {
            nameof m.row, m.row.pretty
            nameof m.col, m.col.pretty }
        |> pretty prettyname (mkRegex None) (stringify >> Regex.Unescape)

    let private encode (row,col) =
        (seq { 
            nameof m.row, fun _ -> (m.row).encode row
            nameof m.col, fun _ -> (m.col).encode col }, None)
        ||> eval (stringify >> Regex.Unescape)


    let private decode (groups : GroupCollection) num prefix =
        let row =
            combineGroupName prefix (nameof m.row)
            |> m.row.decode groups num
    
        let col =
            combineGroupName prefix (nameof m.col)
            |> m.col.decode groups num

        ResultExtensions.zip row col
        |> Result.map (function
        | Some row, Some col -> Some <| LayerId (row, col)
        | _ -> None)

    let encoder = {
        regex = Some >> mkRegex
        encode = encode
        pretty = mkPretty "layer_id"
        decode = decode }


module LayerConnectionEncoder =
    let private m = {|
        first = LayerIdEncoder.encoder
        second = LayerIdEncoder.encoder |}
    
    let private stringify createOptional (printer:Printer) =
        printer.[nameof m.first]
        + createOptional (":" + (printer.[nameof m.second]))

    let private mkRegex = 
        seq {
            nameof m.first, m.first.regex
            nameof m.second, m.second.regex }
        |> eval (stringify regOptional)

    let private mkPretty prettyname =
        seq {
            nameof m.first, m.first.pretty
            nameof m.second, m.second.pretty }
        |> pretty prettyname (mkRegex None) (stringify regOptional >> Regex.Unescape)

    let private encode ((first,second):LayerConnection) =
        let first = nameof m.first, fun _ -> (m.first).encode first
        match second with
        | Some second ->
            (seq {
                first
                nameof m.second, fun _ -> (m.second).encode second }, None)
            ||> eval (stringify id >> Regex.Unescape)
        | None -> 
            (seq { first; nameof m.second, fun _ -> ""}, None)
            ||> eval (stringify (fun _ -> "") >> Regex.Unescape)

    let private decode (groups : GroupCollection) num prefix =
        let first =
            combineGroupName prefix (nameof m.first)
            |> m.first.decode groups num
     
        let second =
            combineGroupName prefix (nameof m.second)
            |> m.second.decode groups num

        ResultExtensions.zip first second
        |> Result.map (function
        | Some first, second -> Some <| LayerConnection (first, second)
        | _ -> None)
        |> Result.mapError (List.concat >> List.toArray >> ValuesOutOfRange)

    let encoder = {
        regex = Some >> mkRegex
        encode = encode
        pretty = mkPretty "layer_connection_id"
        decode = decode }


module PoolingTypeEncoder =
    [<Literal>]
    let MaxPoolingType = "max"
    
    [<Literal>]
    let AvgPoolingType = "avg"

    let values = [| MaxPoolingType; AvgPoolingType |]

    let tryParsePoolingType = function
    | MaxPoolingType -> Some Max
    | AvgPoolingType -> Some Avg
    | _ -> None

    let private regPool = "(" + String.Join("|", values) + ")"

    let private encode = function
    | Avg -> AvgPoolingType
    | Max -> MaxPoolingType

    let private decode (groups : GroupCollection) num name =
        tryGetCapture groups num name
        |> Option.map (fun num ->
            match tryParsePoolingType num.Value with
            | Some ty -> Ok ty
            | _ -> Error {
                Property = name
                IndicatedValue = num.Value })
        |> function
        | Some result -> Result.map Some result
        | None -> Ok None

    let encoder = {
        regex = regNamed regPool
        decode = decode
        encode = encode
        pretty = {
            name = "pooling_type"
            regex = regPool
            defn = Some regPool
            deps = [] }}


module PaddingEncoder =
    [<Literal>]
    let ZeroPadding = "zero"
    
    [<Literal>]
    let SamePadding = "same"

    let values = [| ZeroPadding; SamePadding |]
    
    let private regPadding = "(" + String.Join("|", values) + ")"

    let tryParsePadding = function
    | ZeroPadding -> Some Zero
    | SamePadding -> Some Same
    | _ -> None

    let private encode = function
        | Same -> SamePadding
        | Zero -> ZeroPadding

    let private decode (groups : GroupCollection) num name =
        tryGetCapture groups num name
        |> Option.map (fun num ->
            match tryParsePadding num.Value with
            | Some ty -> Ok ty
            | _ -> Error {
                Property = name
                IndicatedValue = num.Value })
        |> function
        | Some result -> Result.map Some result
        | None -> Ok None

    let encoder = {
        regex = regNamed regPadding
        decode = decode
        encode = encode
        pretty = {
            name = "padding"
            regex = regPadding
            defn = Some regPadding
            deps = [] }}
