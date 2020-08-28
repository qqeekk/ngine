using Microsoft.FSharp.Core;
using Microsoft.Win32;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using Ngine.Infrastructure.Services;
using NgineUI.ViewModels.AppServices.Abstract;
using NgineUI.ViewModels.Control;
using NgineUI.ViewModels.Network.Ambiguities;
using NgineUI.ViewModels.Network.Nodes;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.ViewModels;
using Python.Runtime;
using ReactiveUI;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using static NodeNetwork.Toolkit.NodeList.NodeListViewModel;
using Unit = System.Reactive.Unit;

namespace NgineUI.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private FSharpOption<string> currentFileName;
        public FSharpOption<string> CurrentFileName
        {
            get => currentFileName;
            set => this.RaiseAndSetIfChanged(ref currentFileName, value);
        }

        private readonly INetworkIO<InconsistentNetwork> inconsistentNetworkIO;
        private readonly INetworkIO<Ngine.Domain.Schemas.Network> networkIO;
        private readonly KerasNetworkIO kerasNetworkIO;
        private readonly INetworkPartsConverter partsConverter;
        private LayerIdTracker idTracker;
        private NetworkViewModel network;
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

        public NetworkViewModel Network
        {
            get => network;
            set => this.RaiseAndSetIfChanged(ref network, value);
        }

        public AmbiguitiesViewModel Ambiguities { get; }
        public NodeListViewModel NodeList { get; }
        public HeaderViewModel Header { get; }
        public OptimizerViewModel Optimizer { get; }
        public Subject<Unit> ConversionErrorRaised { get; }
        public Subject<Unit> ConfigureTrainingShouldOpen { get; }
        public Subject<Unit> ConfigureTuningShouldOpen { get; }

        public MainViewModel(INetworkIO<InconsistentNetwork> inconsistentNetworkIO, INetworkIO<Ngine.Domain.Schemas.Network> networkIO,
            KerasNetworkIO kerasNetworkIO, INetworkPartsConverter partsConverter)
        {
            // TODO: inject
            this.inconsistentNetworkIO = inconsistentNetworkIO;
            this.networkIO = networkIO;
            this.kerasNetworkIO = kerasNetworkIO;
            this.partsConverter = partsConverter;
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

            Header = new HeaderViewModel
            {
                SaveModelCommand = ReactiveCommand.Create(() => SaveModel(CurrentFileName.Value),
                    canExecute: this.WhenAnyValue(vm => vm.CurrentFileName).Select(OptionModule.IsSome)),

                SaveKerasModelCommand = ReactiveCommand.Create(() =>
                {
                    using var fileDialog = new System.Windows.Forms.FolderBrowserDialog();

                    if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        SaveKerasModel(fileDialog.SelectedPath);
                    }
                }),

                SaveAsModelCommand = ReactiveCommand.Create(() =>
                {
                    var fileDialog = new SaveFileDialog
                    {
                        Filter = "Ngine-schema files (*.yaml)|*.yaml",
                    };

                    if (fileDialog.ShowDialog() == true)
                    {
                        SaveModel(fileDialog.FileName);
                        CurrentFileName = FSharpOption<string>.Some(fileDialog.FileName);
                    }
                }),
                ReadModelCommand = ReactiveCommand.Create(() =>
                {
                    var fileDialog = new OpenFileDialog
                    {
                        Filter = "Ngine-schema files (*.yaml)|*.yaml",
                    };

                    if (fileDialog.ShowDialog() == true)
                    {
                        ReadModel(fileDialog.FileName);
                        CurrentFileName = FSharpOption<string>.Some(fileDialog.FileName);
                    }
                }),
                ConfigureTrainingCommand = ReactiveCommand.Create(ConfigureTraining),
                ConfigureTuningCommand = ReactiveCommand.Create(ConfigureTuning),
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
            NodeList.AddNodeType(() => new Head1DViewModel(networkConverter.LayerConverter.ActivatorConverter, networkConverter.LossConverter));
            NodeList.AddNodeType(() => new Head2DViewModel(networkConverter.LayerConverter.ActivatorConverter, networkConverter.LossConverter));
            NodeList.AddNodeType(() => new Head3DViewModel(networkConverter.LayerConverter.ActivatorConverter, networkConverter.LossConverter));

            ConversionErrorRaised = new Subject<Unit>();
            ConfigureTrainingShouldOpen = new Subject<Unit>();
            ConfigureTuningShouldOpen = new Subject<Unit>();
            //TODO: remove/uncomment. 
            //var codeObservable = eventNode.OnClickFlow.Values.Connect().Select(_ => new StatementSequence(eventNode.OnClickFlow.Values.Items));
            //codeObservable.BindTo(this, vm => vm.CodePreview.Code);
            //codeObservable.BindTo(this, vm => vm.CodeSim.Code);

            //ForceDirectedLayouter layouter = new ForceDirectedLayouter();
            //var config = new Configuration
            //{
            //    Network = Network,
            //};
            //AutoLayout = ReactiveCommand.Create(() => layouter.Layout(config, 10000));
            //StartAutoLayoutLive = ReactiveCommand.CreateFromObservable(() =>
            //    Observable.StartAsync(ct => layouter.LayoutAsync(config, ct)).TakeUntil(StopAutoLayoutLive)
            //);
            //StopAutoLayoutLive = ReactiveCommand.Create(() => { }, StartAutoLayoutLive.IsExecuting);
        }

        private void ConfigureTraining()
        {
            ConfigureTrainingShouldOpen.OnNext(Unit.Default);
        }

        private void ConfigureTuning()
        {
            ConfigureTuningShouldOpen.OnNext(Unit.Default);
        }

        private static bool ChallengeSaveInFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                var result = MessageBox.Show("Файл уже существует. Перезаписать?", "Сохранить проект", MessageBoxButton.OKCancel);
                return result switch
                {
                    MessageBoxResult.OK => true,
                    _ => false
                };
            }

            return true;
        }

        private void SaveKerasModel(string folderName)
        {
            var model = partsConverter.Encode(Network, Ambiguities.AmbiguityList.GetValues(), Optimizer.GetValue());
            var encoded = inconsistentNetworkIO.NetworkConverter.EncodeInconsistent(model);

            if (networkIO.TryParse(encoded, out var result))
            {
                try
                {
                    kerasNetworkIO.Write(folderName, result);
                }
                catch (PythonException ex)
                {
                    Console.WriteLine($"Ошибка конвертации Keras: {ex.Message}");
                }
            }
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
