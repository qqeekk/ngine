namespace Ngine.Backend.Converters

open Ngine.Domain.Schemas
open System.Collections.Generic
open System

module NetworkErrorPrettyPrinter =
    type PrettyTree = Node of string * PrettyTree list

    let rec prettifyPropsConversionError = function
        | PropsConversionError.PropsPatternMissmatch { Pattern = p } ->
            String.Format("Ошибка форматирования свойства {0}. Строка не удовлетворяет шаблону: {1}", p.name, Option.defaultValue "(-)" p.defn), []

        | PropsConversionError.ValuesOutOfRange ps ->
            "Некоторые из значений шаблона находятся за границами допустимых значений",
            (ps |> Seq.map (fun p -> Node (sprintf "%s - %s" p.Property p.IndicatedValue, [])) |> Seq.toList)

    let prettifyInconsistentLayerConversionError id = function
        | InconsistentLayerConversionError.UnknownType ty ->
            String.Format("Недопустимый тип слоя {0} - {1}", LayerIdEncoder.encoder.encode id, ty), []
        
        | InconsistentLayerConversionError.InvalidAmbiguity (Variable var) ->
            String.Format("Переменная не определена: {0}", var), []

        | InconsistentLayerConversionError.MissingLayerId lid ->
            String.Format("Слой {0} не найден", LayerIdEncoder.encoder.encode lid), []
        
        | InconsistentLayerConversionError.PropsConversionError pe ->
            String.Format("Ошибка форматирования свойств слоя {0}", LayerIdEncoder.encoder.encode id),
            [Node (prettifyPropsConversionError pe)]


    let prettifyLayerConversionError id = function
        | LayerConversionError.Inconsistent (e) -> prettifyInconsistentLayerConversionError id e

        | ExpectedLayerId ->
            String.Format("Недопустимая связь: слой {0} не имеет связий ни с одним из слоев", LayerIdEncoder.encoder.encode id), []
    
        | LayerConversionError.PrevLayerPropsEmpty ->
            String.Format("Недопустимая связь: слой {0} должен объединять не менее двух слоев", LayerIdEncoder.encoder.encode id), []


    let private prettifyInternal prettifyConversionError (errors: NetworkConversionError<'a>[]) =
        let uniqueLayerErrors =
            errors
            |> Seq.collect (function
            | LayerSequenceError (LayerError (l, e)) ->
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
                    String.Format("Слой {0} типа {1} и слой {2} типа {3} имеют несочитаемые размерности",
                        LayerIdEncoder.encoder.encode id, l1.Type, LayerIdEncoder.encoder.encode id2, l2.Type), []

                | LayerCompatibilityError.DuplicateLayerId ->
                    String.Format("Слой {0} имеет неуникальный номер", LayerIdEncoder.encoder.encode id), []

                |> Seq.singleton
                
            | LayerError.LayerError es -> 
                es |> Seq.map (prettifyConversionError id)
                
            | LayerError.AggregateLayerError es ->
                es |> Seq.collect (fun e ->
                    if uniqueLayerErrors.Contains e
                    then Seq.ofList []
                    else prettifyLayerError e)

        errors
        |> Seq.collect (function
        | LayerSequenceError (AmbiguityError (amb, es)) ->
            seq { String.Format("Ошибка форматированния 'неопределенности' {0} - {1}", amb.Name, amb.Value),
                es |> Seq.map (prettifyPropsConversionError >> Node) |> Seq.toList }
            
        | LayerSequenceError (LayerError (l, e)) -> prettifyLayerError (l,e)
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
                seq { String.Format("Неизвестный тип функции потерь {0} - {1}", LayerIdEncoder.encoder.encode h.LayerId, h.Loss), [] })

        | OptimizerError p -> seq { prettifyPropsConversionError p }
        | EmptyHeadArrayError -> seq { "Отстутствуют выходные слои", [] })
        |> Seq.map Node
        |> Seq.toArray

    let prettify errors = prettifyInternal prettifyLayerConversionError errors

    let prettifyInconsistent errors = prettifyInternal prettifyInconsistentLayerConversionError errors
