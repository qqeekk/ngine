namespace Ngine.Backend.Converters

open Ngine.Domain.Schemas
open System.Collections.Generic
open System

module NetworkErrorPrettyPrinter =
    type PrettyTree = Node of string * PrettyTree list

    let rec prettifyPropsConversionError = function
        | PropsConversionError.PropsPatternMissmatch { Pattern = p } ->
            String.Format("Error converting raw props for {0}. Expected pattern: {1}", p.name, Option.defaultValue "(-)" p.defn), []

        | PropsConversionError.ValuesOutOfRange ps ->
            "Some values indicated in pattern are out of range",
            (ps |> Seq.map (fun p -> Node (sprintf "%s - %s" p.Property p.IndicatedValue, [])) |> Seq.toList)

    let prettify (errors: NetworkConversionError[]) =
        let uniqueLayerErrors =
            errors
            |> Seq.collect (function
            | LayerError (l, e) ->
                match e with
                | LayerError.LayerError e -> [l, LayerError.LayerError e]
                | LayerCompatibilityError e -> [l, LayerCompatibilityError e]
                | _ -> []
            | _ -> [])
            |> HashSet

        let rec prettifyLayerError ({ LayerId = (id, _) }:Schema.Layer as l1, e) =
            match e with
            | LayerError.LayerCompatibilityError { Layer2 = {LayerId = (id2, _) } as l2; Error = e } ->
                match e with
                | LayerCompatibilityError.DimensionMissmatch ->
                    String.Format("Layer {0} of type {1} does not match {2} of type {3} by dimensions",
                        LayerIdEncoder.encoder.encode id, l1.Type, LayerIdEncoder.encoder.encode id2, l2.Type), []

                | LayerCompatibilityError.DuplicateLayerId ->
                    String.Format("Layer id {0} is ambiguous", LayerIdEncoder.encoder.encode id), []

                |> Seq.singleton
                
            | LayerError.LayerError es -> 
                es |> Seq.map (function
                    | LayerConversionError.UnknownType ty ->
                        String.Format("Unknown type for layer {0} - {1}", LayerIdEncoder.encoder.encode id, ty), []
                    
                    | LayerConversionError.InvalidAmbiguity (Variable var) ->
                        String.Format("Invalid variable reference: {0}", var), []

                    | LayerConversionError.ExpectedLayerId ->
                        String.Format("Layer {0} not connected with any layer", LayerIdEncoder.encoder.encode id), []
                    
                    | LayerConversionError.MissingLayerId lid ->
                        String.Format("Layer {0} not found", LayerIdEncoder.encoder.encode lid), []
                    
                    | LayerConversionError.PrevLayerPropsEmpty ->
                        String.Format("Layer {0} props not filled with concatenated layer ids not found", LayerIdEncoder.encoder.encode id), []
                    
                    | LayerConversionError.PropsConversionError pe ->
                        String.Format("Error while converting props for layer {0}", LayerIdEncoder.encoder.encode id),
                        [Node (prettifyPropsConversionError pe)]
                )
                
            | LayerError.AggregateLayerError es ->
                es |> Seq.collect (fun e ->
                    if uniqueLayerErrors.Contains e
                    then Seq.ofList []
                    else prettifyLayerError e)

        errors
        |> Seq.collect (function
        | AmbiguityError (amb, es) ->
            seq { String.Format("Error converting ambiguity {0} - {1}", amb.Name, amb.Value), es |> Seq.map (prettifyPropsConversionError >> Node) |> Seq.toList }
            
        | LayerError (l, e) -> prettifyLayerError (l,e)
        | HeadError (h, es) ->
            es |> Seq.collect (function
            | HeadError.LayerError (Some l, e) -> 
                if uniqueLayerErrors.Contains (l,e)
                then Seq.ofList []
                else prettifyLayerError (l,e)

            | HeadError.LayerError (None, e) ->
                prettifyLayerError ({ LayerId = (0u, 0u), None; Type = ""; Props = "" }, e)

            | HeadError.HeadFunctionError p -> seq { prettifyPropsConversionError p }
            | HeadError.LossError (LossError.UnknownType) ->
                seq { String.Format("Unknown loss type for layer {0} - {1}", LayerIdEncoder.encoder.encode h.LayerId, h.Loss), [] })

        | OptimizerError p -> seq { prettifyPropsConversionError p })
        |> Seq.map Node
        |> Seq.toArray
