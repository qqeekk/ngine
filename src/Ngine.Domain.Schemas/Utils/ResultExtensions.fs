module internal ResultExtensions

let aggregateResults (results : #seq<Result<'a, 'b>>) : Result<'a [], 'b []> =
    (Ok [], results)
    ||> Seq.fold (fun state -> function
        | Ok success ->
            Result.map (fun col -> success::col) state
        | Error msg ->
            let prevErrors =
                match state with
                | Ok _ -> []
                | Error errors -> errors

            Error (msg::prevErrors)
        )
    |> function
    | Ok success -> Ok (Seq.rev success |> Seq.toArray)
    | Error fail -> Error (Seq.rev fail |> Seq.toArray)

let zipResults ares bres =
    match ares, bres with
    | (Ok a, Ok b) -> Ok (a, b)
    | Error e, Ok _ | Ok _, Error e -> Error [e]
    | Error e1, Error e2 -> Error [e1; e2]