namespace Ngine.Backend.Converters
open Ngine.Domain.Schemas
open System.Text.RegularExpressions
open Ngine.Domain.Utils
open System.Collections.Generic

type private ListEncoder<'a> = {
    pretty: Pretty
    decode: string -> Result<'a, PropsConversionError>
}

module private ListEncoder =
    type private M<'a> = {
        list: PrimitiveEncoder<'a [], ValueOutOfRangeInfo []>
        range: PrimitiveEncoder<Range<'a>, ValueOutOfRangeInfo []> }

    let private stringify m listOptional rangeOptional (printer:Printer) =
        let list = if listOptional then printer.[nameof m.list] else ""
        let range = if rangeOptional then printer.[nameof m.range] else ""

        match listOptional, rangeOptional with
        | true, true -> sprintf "(%s|%s)" list range
        | true, false -> list
        | false, true -> range
        | false, false -> ""
        |> sprintf "\[%s\]"

    let private mkRegex m =
        (seq {
            nameof m.list, m.list.regex
            nameof m.range, m.range.regex }, None)
        ||> eval (stringify m true true >> regComplete)

    let private mkPretty m =
        seq {
            nameof m.list, (m.list).pretty
            nameof m.range, (m.range).pretty }
        |> pretty (sprintf "%s|%s" m.list.pretty.name m.range.pretty.name) (mkRegex m) (stringify m true true >> Regex.Unescape)

    let private decode mx pretty =
        tryDecodeByRegex (pretty.regex) <| fun groups ->
            let list = mx.list.decode groups 0u (nameof mx.list)
            let range = mx.range.decode groups 0u (nameof mx.range)

            ResultExtensions.zip list range
            |> Result.map (function
            | Some list, None when not (Array.isEmpty list) -> Some (Values.List list)
            | _, Some r -> Some (Values.Range r)
            | _ -> None)
            |> Result.mapError (Array.concat >> ValuesOutOfRange)
        >> mapToError pretty

    let encode internalEncoder =
        let m = {
            list = CommaSeparatedValuesEncoder.encoder internalEncoder
            range = RangeValuesEncoder.encoder internalEncoder }

        function
        | List l ->
            (seq {
                nameof m.list, fun _ -> (m.list).encode l
                nameof m.range, fun _ -> "" }, None)
            ||> eval (stringify m true false >> Regex.Unescape)
        | Range r ->
            (seq {
                nameof m.list, fun _ -> ""
                nameof m.range, fun _ -> (m.range).encode r }, None)
            ||> eval (stringify m false true >> Regex.Unescape)

    let encoder internalEncoder =
        let m = {
            list = CommaSeparatedValuesEncoder.encoder internalEncoder
            range = RangeValuesEncoder.encoder internalEncoder }

        let pretty = mkPretty m

        { pretty = pretty
          decode = decode m pretty }

module private AmbiguityNameEncoder =
    let private m = {| name = VariableNameEncoder.encoder |}

    let private stringify (p:Printer) =
        p.[nameof m.name]

    let private regex =
        (seq {
            nameof m.name, m.name.regex }, None)
        ||> eval (stringify >> regComplete)

    let private pretty =
        seq {
            nameof m.name, m.name.pretty }
        |> pretty "ambiguity_name" regex (stringify >> Regex.Unescape)

    let private decode =
        tryDecodeByRegex regex <| fun groups ->
            m.name.decode groups 0u (nameof m.name)
            |> Result.mapError(fun _ -> ValuesOutOfRange [||])
        >> mapToError pretty

    let encode = m.name.encode
    let encoder = { pretty = pretty; decode = decode }

module AmbiguityConverter =
    let private encoder = ListEncoder.encoder IntegerEncoder.encoder

    let private decode (ambiguity:Schema.Ambiguity) =
        let variable = AmbiguityNameEncoder.encoder.decode ambiguity.Name
        let values = encoder.decode ambiguity.Value

        ResultExtensions.zip variable values
        |> Result.mapError (List.toArray)
        |> Result.map (KeyValuePair)

    let private encode (kvp: KeyValuePair<AmbiguityVariableName, Values<uint32>>) :Schema.Ambiguity =
        { Name = AmbiguityNameEncoder.encode kvp.Key
          Value = (ListEncoder.encode IntegerEncoder.encoder) kvp.Value }

    let instance = {
        new IAmbiguityConverter with
            member _.Encode kvp = encode kvp
            member _.Decode amb = decode amb
            member _.ListPattern = encoder.pretty}
