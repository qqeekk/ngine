using Microsoft.FSharp.Core;
using Ngine.Domain.Execution;
using Ngine.Domain.Schemas;
using Ngine.Domain.Utils;
using Ngine.Infrastructure.Abstractions;
using Ngine.Infrastructure.Abstractions.Services;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.AppServices.Abstract;
using NgineUI.ViewModels.Control;
using NgineUI.ViewModels.Network.Ambiguities;
using NgineUI.ViewModels.Network.Nodes;
using NgineUI.ViewModels.Parameters;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.ViewModels;
using Python.Runtime;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using static NodeNetwork.Toolkit.NodeList.NodeListViewModel;
using Unit = System.Reactive.Unit;

namespace NgineUI.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly INetworkIO<InconsistentNetwork> inconsistentNetworkIO;
        private readonly INetworkIO<Ngine.Domain.Schemas.Network> networkIO;
        private readonly INetworkCompiler networkCompiler;
        private readonly INetworkPartsConverter partsConverter;
        private readonly IInteractionService interactionService;
        private readonly string kerasFolderPath;
        private readonly TrainParametersViewModel trainParametersViewModel;
        private readonly TuneParametersViewModel tuneParametersViewModel;

        private LayerIdTracker idTracker;
        private bool activation1DViewModelIsFirstLoaded = true;
        private bool activation2DViewModelIsFirstLoaded = true;
        private bool activation3DViewModelIsFirstLoaded = true;
        private bool concatenation1DViewModelIsFirstLoaded = true;
        private bool concatenation2DViewModelIsFirstLoaded = true;
        private bool concatenation3DViewModelIsFirstLoaded = true;
        private bool conv2DViewModelIsFirstLoaded = true;
        private bool conv3DViewModelIsFirstLoaded = true;
        private bool flatten2DViewModelIsFirstLoaded = true;
        private bool flatten3DViewModelIsFirstLoaded = true;
        private bool dropoutViewModelIsFirstLoaded = true;
        private bool input1DViewModelIsFirstLoaded = true;
        private bool input2DViewModelIsFirstLoaded = true;
        private bool input3DViewModelIsFirstLoaded = true;
        private bool pooling2DViewModelIsFirstLoaded = true;
        private bool pooling3DViewModelIsFirstLoaded = true;
        private bool denseViewModelIsFirstLoaded = true;

        private static bool InvertIfTrue(ref bool flag)
        {
            if (flag)
            {
                flag = false;
                return true;
            }

            return false;
        }

        private bool ChallengeSaveInFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                return interactionService.AskUserPermission("Файл уже существует. Перезаписать?", "Сохранить проект");
            }

            return true;
        }

        #region ExecutionCancellationTokenSource
        private CancellationTokenSource executionCancellationTokenSource;
        private CancellationTokenSource ExecutionCancellationTokenSource
        {
            get => executionCancellationTokenSource;
            set => this.RaiseAndSetIfChanged(ref executionCancellationTokenSource, value);
        }
        #endregion

        #region CurrentFileName
        private FSharpOption<string> currentFileName;
        public FSharpOption<string> CurrentFileName
        {
            get => currentFileName;
            set => this.RaiseAndSetIfChanged(ref currentFileName, value);
        }
        #endregion

        #region Network
        private NetworkViewModel network;
        public NetworkViewModel Network
        {
            get => network;
            set => this.RaiseAndSetIfChanged(ref network, value);
        }
        #endregion

        public AmbiguitiesViewModel Ambiguities { get; }
        public NodeListViewModel NodeList { get; }
        public HeaderViewModel Header { get; }
        public OptimizerViewModel Optimizer { get; }
        public Subject<Unit> ConversionErrorRaised { get; }

        public MainViewModel(INetworkIO<InconsistentNetwork> inconsistentNetworkIO,
                             INetworkIO<Ngine.Domain.Schemas.Network> networkIO,
                             INetworkCompiler networkCompiler,
                             INetworkPartsConverter partsConverter,
                             IInteractionService interactionService,
                             IFileFormat mappingsFileFormat,
                             string kerasFolderPath)
        {
            // TODO: inject
            this.inconsistentNetworkIO = inconsistentNetworkIO;
            this.networkIO = networkIO;
            this.networkCompiler = networkCompiler;
            this.partsConverter = partsConverter;
            this.interactionService = interactionService;
            this.kerasFolderPath = kerasFolderPath;
            this.trainParametersViewModel = new TrainParametersViewModel(interactionService, mappingsFileFormat);
            this.tuneParametersViewModel = new TuneParametersViewModel(interactionService, mappingsFileFormat);

            this.idTracker = new LayerIdTracker();
            var networkConverter = networkIO.NetworkConverter;

            CurrentFileName = FSharpOption<string>.None;
            Network = new NetworkViewModel();
            Optimizer = new OptimizerViewModel(networkConverter.OptimizerConverter);

            // Set up ambiguity values.
            Ambiguities = new AmbiguitiesViewModel(networkConverter.AmbiguityConverter);

            NodeList = new NodeListViewModel
            {
                Title = "Добавить слой",
                EmptySearchText = "Поиск...",
                EmptyLabel = "Нет результатов, удовлетворяющих условиям поиска.",
                StringifyDisplayMode = mode => mode switch
                {
                    DisplayMode.List => "список",
                    DisplayMode.Tiles => "миниатюры",
                    _ => throw new Exception()
                }
            };

            var isAnyJobRunning = this.WhenAnyValue(vm => vm.ExecutionCancellationTokenSource).Select(s => s != null);
            Header = new HeaderViewModel(inconsistentNetworkIO.FileFormat)
            {
                SaveModelCommand = ReactiveCommand.Create(() => SaveModel(CurrentFileName.Value),
                    canExecute: this.WhenAnyValue(vm => vm.CurrentFileName).Select(OptionModule.IsSome)),

                SaveKerasModelCommand = ReactiveCommand.Create(SaveKerasModel),
                SaveAsModelCommand = ReactiveCommand.Create(SaveModelAs),
                ReadModelCommand = ReactiveCommand.Create(ReadModel),
                RunTraingCommand = ReactiveCommand.CreateFromTask(RunTraining, isAnyJobRunning.Select(r => !r)),
                RunTuningCommand = ReactiveCommand.CreateFromTask(RunTuning, isAnyJobRunning.Select(r => !r)),
                StopRunningCommand = ReactiveCommand.Create(() => ExecutionCancellationTokenSource.Cancel(), isAnyJobRunning),
                ConfigureTrainingCommand = ReactiveCommand.Create(() =>
                {
                    interactionService.Navigate(trainParametersViewModel, "Ngine - Параметры");
                }),
                ConfigureTuningCommand = ReactiveCommand.Create(() =>
                {
                    interactionService.Navigate(tuneParametersViewModel, "Ngine - Параметры");
                }),
            };

            NodeList.AddNodeType(() => new Input1DViewModel(idTracker, !InvertIfTrue(ref input1DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Input2DViewModel(idTracker, !InvertIfTrue(ref input2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Input3DViewModel(idTracker, !InvertIfTrue(ref input3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new DenseViewModel(idTracker, Ambiguities.AmbiguityList, !InvertIfTrue(ref denseViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Conv2DViewModel(idTracker, Ambiguities.AmbiguityList, !InvertIfTrue(ref conv2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Conv3DViewModel(idTracker, Ambiguities.AmbiguityList, !InvertIfTrue(ref conv3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Pooling2DViewModel(idTracker, Ambiguities.AmbiguityList, !InvertIfTrue(ref pooling2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Pooling3DViewModel(idTracker, Ambiguities.AmbiguityList, !InvertIfTrue(ref pooling3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Activation1DViewModel(networkConverter.LayerConverter.ActivatorConverter, idTracker, !InvertIfTrue(ref activation1DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Activation2DViewModel(networkConverter.LayerConverter.ActivatorConverter, idTracker, !InvertIfTrue(ref activation2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Activation3DViewModel(networkConverter.LayerConverter.ActivatorConverter, idTracker, !InvertIfTrue(ref activation3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Concatenation1DViewModel(idTracker, !InvertIfTrue(ref concatenation1DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Concatenation2DViewModel(idTracker, !InvertIfTrue(ref concatenation2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Concatenation3DViewModel(idTracker, !InvertIfTrue(ref concatenation3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Flatten2DViewModel(idTracker, !InvertIfTrue(ref flatten2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Flatten3DViewModel(idTracker, !InvertIfTrue(ref flatten3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new DropoutViewModel(idTracker, !InvertIfTrue(ref dropoutViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Head1DViewModel(networkConverter.LayerConverter.ActivatorConverter, networkConverter.LossConverter));
            NodeList.AddNodeType(() => new Head2DViewModel(networkConverter.LayerConverter.ActivatorConverter, networkConverter.LossConverter));
            NodeList.AddNodeType(() => new Head3DViewModel(networkConverter.LayerConverter.ActivatorConverter, networkConverter.LossConverter));

            ConversionErrorRaised = new Subject<Unit>();
        }

        private async Task RunTuning()
        {
            var tuneParameters = tuneParametersViewModel.TryGetValue();
            if (tuneParameters.IsError)
            {
                interactionService.ShowUserMessage(string.Join(Environment.NewLine, tuneParameters.ErrorValue), "Ошибка");
                return;
            }

            if (tuneParameters.ResultValue is var (path, epochs, trials, split)
                && TrySaveKerasModel(kerasFolderPath, out var networkCompilerOutput))
            {
                if (OptionModule.IsNone(networkCompilerOutput.AmbiguitiesPath))
                {
                    interactionService.ShowUserMessage("Не указано пространство поиска для гиперпараметров", "Ошибка");
                    return;
                }

                var network = networkCompiler.NetworkGenerator.Instantiate(networkCompilerOutput.CompiledNetworkPath);

                using (ExecutionCancellationTokenSource = new CancellationTokenSource())
                {
                    await network.Tune(networkCompilerOutput.AmbiguitiesPath.Value, path, trials, epochs, split, ExecutionCancellationTokenSource.Token);
                    interactionService.ShowUserMessage("Результаты обучения отображены в терминале", "Обучение закончено");
                }

                ExecutionCancellationTokenSource = null;
            }
        }

        private async Task RunTraining()
        {
            var trainParameters = trainParametersViewModel.TryGetValue();
            if (trainParameters.IsError)
            {
                interactionService.ShowUserMessage(string.Join(Environment.NewLine, trainParameters.ErrorValue), "Ошибка");
                return;
            }

            if (trainParameters.ResultValue is var (path, batch, epochs, split)
                && TrySaveKerasModel(kerasFolderPath, out var networkCompilerOutput))
            {
                var network = networkCompiler.NetworkGenerator.Instantiate(networkCompilerOutput.CompiledNetworkPath);

                using (ExecutionCancellationTokenSource = new CancellationTokenSource())
                { 
                    await network.Train(path, batch, epochs, split, ExecutionCancellationTokenSource.Token);
                    interactionService.ShowUserMessage("Результаты обучения отображены в терминале", "Обучение закончено");
                }
                
                ExecutionCancellationTokenSource = null;
            }
        }

        private void SaveKerasModel()
        {
            interactionService.OpenFolderDialog(folderName => TrySaveKerasModel(folderName, out _));
        }

        private void SaveModelAs()
        {
            interactionService.SaveFileDialog(inconsistentNetworkIO.FileFormat, file =>
            {
                SaveModel(file);
                if (OptionModule.IsNone(CurrentFileName))
                {
                    CurrentFileName = file;
                }
            });
        }

        private void ReadModel()
        {
            interactionService.OpenFileDialog(inconsistentNetworkIO.FileFormat, file =>
            {
                ReadModel(file);
                CurrentFileName = FSharpOption<string>.Some(file);
            });
        }

        private bool TrySaveKerasModel(string folderName, out INetworkCompilerOutput networkCompilerOutput)
        {
            var model = partsConverter.Encode(Network, Ambiguities.AmbiguityList.GetValues(), Optimizer.GetValue());
            var encoded = inconsistentNetworkIO.NetworkConverter.EncodeInconsistent(model);

            if (networkIO.TryParse(encoded, out var result))
            {
                try
                {
                    networkCompilerOutput = networkCompiler.Write(folderName, result);
                    return true;
                }
                catch (PythonException ex)
                {
                    Console.WriteLine($"Ошибка конвертации Keras: {ex.Message}");
                }
            }

            networkCompilerOutput = default;
            return false;
        }

        private void SaveModel(string fileName)
        {
            var encoded = partsConverter.Encode(Network, Ambiguities.AmbiguityList.GetValues(), Optimizer.GetValue());

            if (ChallengeSaveInFile(fileName))
            {
                inconsistentNetworkIO.Write(fileName, encoded);
            }
        }

        private void ReadModel(string fileName)
        {
            if (inconsistentNetworkIO.Read(fileName, out var network))
            {
                (Network, Ambiguities.AmbiguityList, idTracker) = partsConverter.Decode(network);
                Optimizer.Fill(network.Optimizer);
            }
            else
            {
                ConversionErrorRaised.OnNext(Unit.Default);
            }
        }
    }
}
