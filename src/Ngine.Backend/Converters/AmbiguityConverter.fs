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
        |> pretty (sprintf "%s|%s" m.list.pretty.name m.range.pretty.name) 
            (mkRegex m) (stringify m true true >> Regex.Unescape)

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

    let private decodeValues = encoder.decode

    let private decode (ambiguity:Schema.Ambiguity) =
        let variable = AmbiguityNameEncoder.encoder.decode ambiguity.Name
        let values = decodeValues ambiguity.Value

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
            member _.DecodeValues v = decodeValues v
            member _.ListPattern = encoder.pretty}

    let keras (network:Network): Schema.AmbiguityMapProduct =
        let mappingProps name =
            let (|MaybeAmb|) prop = function
            | RefName (Variable var) when name = var -> Some prop
            | _ -> None
        
            function
            | LayerProps.Activator1D _ -> []
            | LayerProps.Activator2D _ -> []
            | LayerProps.Activator3D _ -> []
            | LayerProps.Dropout _ -> []
            | LayerProps.PrevLayers _ -> []
            | LayerProps.Flatten2D -> []
            | LayerProps.Flatten3D -> []
            | LayerProps.Sensor1D _ -> []
            | LayerProps.Sensor2D _ -> []
            | LayerProps.Sensor3D _ -> []
            | LayerProps.Convolutional2D conv ->
                let (MaybeAmb "filters" f) = conv.Filters
                let (MaybeAmb "kernel_size[0]" x, MaybeAmb "kernel_size[1]" y) = conv.Kernel
                let (MaybeAmb "strides[0]" s1, MaybeAmb "strides[1]" s2) = conv.Strides
                [ f; x; y; s1; s2 ] |> List.choose id
            
            | LayerProps.Pooling2D p ->
                let (MaybeAmb "kernel_size[0]" x, MaybeAmb "kernel_size[1]" y) = p.Kernel
                let (MaybeAmb "strides[0]" s1, MaybeAmb "strides[1]" s2) = p.Strides
                [ x; y; s1; s2 ] |> List.choose id
            
            | LayerProps.Convolutional3D conv ->
                let (MaybeAmb "filters" f) = conv.Filters
                let (MaybeAmb "kernel_size[0]" x, MaybeAmb "kernel_size[1]" y, MaybeAmb "kernel_size[2]" z) = conv.Kernel
                let (MaybeAmb "strides[0]" s1, MaybeAmb "strides[1]" s2, MaybeAmb "strides[2]" s3) = conv.Strides
                [ f; x; y; z; s1; s2; s3 ] |> List.choose id

            | LayerProps.Pooling3D p ->
                let (MaybeAmb "kernel_size[0]" x, MaybeAmb "kernel_size[1]" y, MaybeAmb "kernel_size[2]" z) = p.Kernel
                let (MaybeAmb "strides[0]" s1, MaybeAmb "strides[1]" s2, MaybeAmb "strides[2]" s3) = p.Strides
                [ x; y; z; s1; s2; s3 ] |> List.choose id

            | LayerProps.Dense dense ->
                let (MaybeAmb "units" u) = dense.Units
                Option.toList u

        let rec mappings name l : Schema.AmbiguityMapRecord list =
            let mappingsNonHead3D = function
            | Layer prev -> mappings name (D3 prev)
            | Sensor _ -> []

            let mappingsNonHead2D = function
            | Layer prev -> mappings name (D2 prev)
            | Sensor _ -> []

            let mappingsNonHead1D = function
            | Layer prev -> mappings name (D1 prev)
            | Sensor _ -> []


            match l with
            | D1 (HeadLayer (lid, l)) ->
                match l with
                | Flatten3D prev -> mappingsNonHead3D prev
                | Flatten2D prev -> mappingsNonHead2D prev
                | Dropout (_, prev) -> mappingsNonHead1D prev
                | Activation1D (_, prev) -> mappingsNonHead1D prev
                
                | Dense (prop, prev) ->
                    (mappingProps name (LayerProps.Dense prop)
                    |> List.map (fun p -> { Name = lid; Prop = p }))
                    @ mappingsNonHead1D prev
                
                | Concatenation1D prevs -> 
                    prevs 
                    |> Seq.collect (mappingsNonHead1D)
                    |> Seq.toList

                | Empty1D -> failwithf "Ошибка трансляции Keras %A: отсутствуют необходимые связи." lid

            | D2 (HeadLayer (lid, l)) ->
                match l with
                | Activation2D (_, prev) -> mappingsNonHead2D prev
                | Conv2D (prop, prev) ->
                    (mappingProps name (LayerProps.Convolutional2D prop)
                    |> List.map (fun p -> { Name = lid; Prop = p }))
                    @ mappingsNonHead2D prev
                
                | Pooling2D (prop, prev) ->
                    (mappingProps name (LayerProps.Pooling2D prop)
                    |> List.map (fun p -> { Name = lid; Prop = p }))
                    @ mappingsNonHead2D prev
                
                | Concatenation2D prevs -> 
                    prevs 
                    |> Seq.collect (mappingsNonHead2D)
                    |> Seq.toList

                | Empty2D -> failwithf "Ошибка трансляции Keras %A: отсутствуют необходимые связи." lid

            | D3 (HeadLayer (lid, l)) ->
                match l with
                | Activation3D (_, prev) -> mappingsNonHead3D prev
                | Conv3D (prop, prev) ->
                    (mappingProps name (LayerProps.Convolutional3D prop)
                    |> List.map (fun p -> { Name = lid; Prop = p }))
                    @ mappingsNonHead3D prev
                
                | Pooling3D (prop, prev) ->
                    (mappingProps name (LayerProps.Pooling3D prop)
                    |> List.map (fun p -> { Name = lid; Prop = p }))
                    @ mappingsNonHead3D prev
                
                | Concatenation3D prevs -> 
                    prevs 
                    |> Seq.collect (mappingsNonHead3D)
                    |> Seq.toList

                | Empty3D ->  failwithf "Ошибка трансляции Keras %A: отсутствуют необходимые связи." lid

        let mappings name : Schema.AmbiguityMapRecord[] =
            network.Heads
            |> Seq.collect (function
                | Head.Activator (_, _, l, _) -> mappings name l
                | Head.Softmax (_, _, l) -> mappings name (D1 l))
            |> Seq.distinct
            |> Seq.toArray

        { Ambiguities =
            network.Ambiguities
            |> Seq.map<_, Schema.AmbiguityMapValue> (fun kvp ->
                let (Variable name) = kvp.Key
                let a = encode kvp

                { Name = name; Mappings = mappings a.Name; Value = a.Value })
            |> Seq.toArray }
