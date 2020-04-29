namespace Ngine.Backend.Tests

open System
open Xunit
open FsUnit.Xunit
open Ngine.Domain.Schemas
open Ngine.Domain.Utils
open Ngine.Domain.Schemas.Expressions
open Ngine.Backend.Resources.Properties
open NHamcrest.Core
open Ngine.Backend.Converters

type NetworkSchemaConversionTests() =
    do Recources.Culture <- System.Globalization.CultureInfo.CurrentCulture
    let activatorConverter = ActivatorConverter.instance
    //let kernelConverter = KernelConverter.instance

    [<Fact>]
    member _.``Predefined functions are parsed correctly`` () =
        for schema in activatorConverter.ActivationFunctionNames do
            let encodedResult =
                activatorConverter.Decode(schema.name)
                |> Result.map (activatorConverter.Encode)

            Assert.Equal(encodedResult, Ok (schema.name))

    [<Theory>]
    [<InlineData null>]
    [<InlineData "">]
    [<InlineData "chicken_wings">]
    member _.``ActivatorConverter decode does not throw exceptions on invalid data except null`` data =
        match data with
        | null -> should throw typeof<ArgumentNullException> <| fun () ->
            do activatorConverter.Decode(data) |> ignore

        | data ->
            match activatorConverter.Decode data with
            | Error _-> true
            | _ -> false
            |> Assert.True
