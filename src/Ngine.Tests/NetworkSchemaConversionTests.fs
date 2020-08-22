namespace Ngine.Tests

open System
open Xunit
open FsUnit.Xunit
open Ngine.Backend.Resources.Properties
open Ngine.Backend.Converters

type NetworkSchemaConversionTests() =
    do Recources.Culture <- System.Globalization.CultureInfo.CurrentCulture
    let activatorConverter = ActivatorConverter.instance
    let isOk = function | Ok _ -> true | Error _ -> false

    [<Theory>]
    [<InlineData null>]
    [<InlineData "">]
    [<InlineData "chicken_wings">]
    member _.``ActivatorConverter decode does not throw exceptions on invalid data except null`` data =
        match data with
        | null -> should throw typeof<ArgumentNullException> <| fun () ->
            activatorConverter.Decode(data)
            |> ignore

        | data ->
            not (isOk (activatorConverter.Decode data))
            |> Assert.True
