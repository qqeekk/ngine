namespace Ngine.Tests

open Ngine.Domain.Schemas
open Ngine.Domain.Services.Conversion
open Ngine.Backend.Converters
open System.Collections.Generic
open NgineUI.ViewModels.Functional
open Ngine.Infrastructure.Services
open Ngine.Infrastructure.Serialization
open System.Linq
open System
open FsUnit
open Xunit
open NgineUI.ViewModels
open Ngine.Backend
open Ngine.Infrastructure.Configuration
open Microsoft.Extensions.Configuration
open Ngine.Infrastructure.Services.FileFormats
open Ngine.Infrastructure.Services

type NetworkViewModelManagerTests() =
    let converter = 
        let activatorConverter = KernelConverter.create ActivatorConverter.instance
        NetworkConverters.create activatorConverter LossConverter.instance OptimizerConverter.instance AmbiguityConverter.instance

    let vmManager =
        NetworkViewModelManager.instance converter
    
    let consistentNetworkIO =
        NetworkIO(converter, SerializationProfile.Deserializer, SerializationProfile.Serializer)

    let networkIO =
        InconsistentNetworkIO(converter, SerializationProfile.Deserializer, SerializationProfile.Serializer)

    let kerasExecutionOptions =
        let config = DefaultConfigurationBuilder.Create("appsettings.json").Build()
        config.GetSection("AppSettings:ExecutionOptions").Get<KerasExecutionOptions>();

    let kerasNetworGenerator =
        KerasNetworkGenerator(kerasExecutionOptions.PythonPath)

    let kerasNetworkIO =
        KerasNetworkCompiler(SerializationProfile.Serializer, kerasNetworGenerator)

    let model =
        // sensor2D - 1:[28x28]
        let ``1-1-kernel`` = {
            Channels = 1u
            Inputs = (28u, 28u) }

        // conv2D - 6:[5x5], padding=same
        let ``2-1-kernel`` = { 
            Filters = Ambiguous.Fixed 6u
            Kernel = (Ambiguous.Fixed 5u, Ambiguous.Fixed 5u)
            Strides = (Ambiguous.Fixed 1u, Ambiguous.Fixed 1u)
            Padding = Same }

        // activation2D - tanh
        let ``3-1-kernel`` =
            Activator.QuotedFunction (Expressions.QuotedFunction.Tanh)

        // pooling2D - avg:[2x2], strides=[1x1]
        let ``4-1-kernel`` = {
            Kernel = (Ambiguous.Fixed 2u, Ambiguous.Fixed 2u)
            Strides = (Ambiguous.Fixed 1u, Ambiguous.Fixed 1u)
            PoolingType = PoolingType.Avg }

        // sensor1D - 10
        let ``1-2-kernel`` = {
            Inputs = 10u }

        // dense - 50
        let ``2-2-kernel`` = {
            Units = Ambiguous.Fixed 50u }

        // activation2D - tanh
        let ``3-2-kernel`` =
            Activator.QuotedFunction (Expressions.QuotedFunction.Sigmoid)
        
        // dense - 100
        let ``7-1-kernel`` = {
            Units = Ambiguous.Fixed 100u }

        let ``1-1`` = NonHeadLayer<Layer2D, _>.Sensor ((1u, 1u), ``1-1-kernel``)
        let ``2-1`` = NonHeadLayer<_, Sensor2D>.Layer (((2u, 1u), Conv2D (``2-1-kernel``, ``1-1``)) |> HeadLayer.HeadLayer)
        let ``3-1`` = NonHeadLayer<_, Sensor2D>.Layer (((3u, 1u), Activation2D(``3-1-kernel``, ``2-1``)) |> HeadLayer.HeadLayer)
        let ``4-1`` = NonHeadLayer<_, Sensor2D>.Layer (((4u, 1u), Pooling2D(``4-1-kernel``, ``3-1``)) |> HeadLayer.HeadLayer)
        let ``5-1`` = NonHeadLayer<_, Sensor1D>.Layer (((5u, 1u), Flatten2D ``4-1``) |> HeadLayer.HeadLayer)

        let ``1-2`` = NonHeadLayer<Layer1D, _>.Sensor ((1u, 2u), ``1-2-kernel``)
        let ``2-2`` = NonHeadLayer<_, Sensor1D>.Layer (((2u, 2u), Dense(``2-2-kernel``, ``1-2``)) |> HeadLayer.HeadLayer)
        let ``3-2`` = NonHeadLayer<_, Sensor1D>.Layer (((3u, 2u), Activation1D(``3-2-kernel``, ``2-2``)) |> HeadLayer.HeadLayer)

        let ``6-1`` = NonHeadLayer<_, Sensor1D>.Layer (((6u, 1u), Concatenation1D [| ``5-1``; ``3-2`` |]) |> HeadLayer.HeadLayer)
        let ``7-1`` = HeadLayer.HeadLayer ((7u, 1u), Dense(``7-1-kernel``, ``6-1``))

        let head = Head.Softmax (1.f, Loss.CE, ``7-1``)
        let layers = [|
            NetworkConverters.convert2D ``1-1``
            NetworkConverters.convert2D ``2-1``
            NetworkConverters.convert2D ``3-1``
            NetworkConverters.convert2D ``4-1``
            NetworkConverters.convert1D ``5-1``
            NetworkConverters.convert1D ``1-2``
            NetworkConverters.convert1D ``2-2``
            NetworkConverters.convert1D ``3-2``
            NetworkConverters.convert1D ``6-1``
            Choice1Of2 (HeadLayer.D1 ``7-1``) |]

        let optimizer = Optimizer.SGD(1e-4f, { momentum = 0.5f; decay = 0.f })

        { Heads = [| head |]
          Layers = layers
          Ambiguities = Dictionary()
          Optimizer = optimizer }

    [<Xunit.Fact>]
    member _.``Should convert inconsistent network to view model``() =
        let (struct (network, _, _)) = vmManager.Decode model
        let nn = vmManager.Encode(network, model.Ambiguities, model.Optimizer)
        
        Assert.Equal<Head>(nn.Heads, model.Heads)
        

    [<Xunit.Fact>]
    member _.``Read model properly``() =
        networkIO.Write("file.yaml", model)
        let _, nn = networkIO.Read("file.yaml")

        Assert.Equal<Choice<_,_>>(nn.Layers, model.Layers)
        Assert.Equal<Head>(nn.Heads, model.Heads)

    [<Xunit.Fact>]
    member _.``Verify runtimes``() =
        let vm = MainViewModel(networkIO, consistentNetworkIO, kerasNetworkIO, vmManager, null, NgineMappingsFormat(), 
                               AmbiguitiesIO(SerializationProfile.Deserializer), kerasExecutionOptions.OutputDirectory)

        do vm.Header.ReadModelCommand.Execute() |> ignore
        do vm.Header.SaveModelCommand.Execute() |> ignore

