namespace Ngine.Domain.Utils
module internal ResultExtensions =
    let aggregate (results : seq<Result<'a, 'b>>) : Result<'a [], 'b []> =
        (Ok [], results)
        ||> Seq.fold (fun acc -> function
            | Ok success ->
                Result.map (fun col -> success::col) acc
            | Error msg ->
                let prevErrors =
                    match acc with
                    | Ok _ -> []
                    | Error errors -> errors

                Error (msg::prevErrors)
            )
        |> function
        | Ok success -> Ok (Seq.rev success |> Seq.toArray)
        | Error fail -> Error (Seq.rev fail |> Seq.toArray)

    let zip ares bres =
        match ares, bres with
        | (Ok a, Ok b) -> Ok (a, b)
        | Error e, Ok _ | Ok _, Error e -> Error [e]
        | Error e1, Error e2 -> Error [e1; e2]

    let zip3 ares bres cres =
        match ares, bres, cres with
        | (Ok a, Ok b, Ok c) -> Ok (a, b, c)
        | Error e, Ok _, Ok _ 
        | Ok _, Error e, Ok _
        | Ok _, Ok _, Error e-> Error [e]
        | Ok _, Error e1, Error e2
        | Error e1, Ok _, Error e2
        | Error e1, Error e2, Ok _ -> Error [e1; e2]
        | Error e1, Error e2, Error e3 -> Error [e1; e2; e3]