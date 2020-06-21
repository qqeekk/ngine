using DynamicData;
using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Domain.Schemas.Expressions;
using Ngine.Domain.Utils;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Activator = Ngine.Domain.Schemas.Activator;

namespace NgineUI.ViewModels.Network.Nodes
{
    public abstract class HeadViewModelBase<TLayer> : NgineNodeViewModel, IConfigurable<Head>
    {
        private const string NameBase = "Head";
        private readonly IActivatorConverter activatorConverter;
        private readonly ILossConverter lossConverter;

        public LookupEditorViewModel<HeadFunction> ActivationEditor { get; }
        public ValueEditorViewModel<string> LossEditor { get; }
        public ValueEditorViewModel<float> LossWeightEditor { get; }
        public ValueNodeInputViewModel<HeadLayer<TLayer>> Previous { get; }

        public Head Output => output.Value;
        private ObservableAsPropertyHelper<Head> output;

        public HeadViewModelBase(
            IActivatorConverter activatorConverter,
            ILossConverter lossConverter,
            PortType port) : base(null, NodeType.Head, CombineName(NameBase, port), false)
        {
            ActivationEditor = new LookupEditorViewModel<HeadFunction>(
                    v => ParseActivator(activatorConverter, v),
                    activatorConverter.EncodeHeadActivation(DefaultActivator))
            {
                LookupValues = GetLookupValues(activatorConverter)
            };
            AddInlinedInput("Activation", ActivationEditor);

            var lossNames = Array.ConvertAll(lossConverter.LossFunctionNames, p => p.name);
            LossEditor = new ComboEditorViewModel(lossNames);
            AddInlinedInput("Loss", LossEditor);

            LossWeightEditor = new FloatEditorViewModel();
            AddInlinedInput("Loss Weight", LossWeightEditor);

            Previous = new NgineInputViewModel<HeadLayer<TLayer>>(PortType.Head);
            this.Inputs.Add(Previous);

            Observable
                .CombineLatest(
                    shouldUpdateChanged,
                    Previous.ValueChanged.Select(p => p ?? WrapEmpty(DefaultPrevious)),
                    ActivationEditor.ValueChanged.Select(v => OptionModule.DefaultValue(DefaultActivator, ActivationEditor.SelectedValue)),
                    LossEditor.ValueChanged.Select(v => lossConverter.DecodeLoss(v).ResultValue),
                    LossWeightEditor.ValueChanged,
                    (_, p, a, l, w) => EvaluateValue(p, a, l, w))
                .ToProperty(this, vm => vm.Output, out output);
            this.activatorConverter = activatorConverter;
            this.lossConverter = lossConverter;
        }

        public void Setup(Head config)
        {
            switch (config)
            {
                case Head.Activator a:
                    LossWeightEditor.Value = a.Item1;
                    LossEditor.Value = lossConverter.EncodeLoss(a.Item2);
                    ActivationEditor.Value = activatorConverter.EncodeHeadActivation(HeadFunction.NewActivator(a.Item4));
                    break;
                case Head.Softmax s:
                    LossWeightEditor.Value = s.Item1;
                    LossEditor.Value = lossConverter.EncodeLoss(s.Item2);
                    ActivationEditor.Value = activatorConverter.EncodeHeadActivation(HeadFunction.Softmax);
                    break;
            }
        }

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => HeadChoice(Output);

        protected abstract TLayer DefaultPrevious { get; }
        protected abstract HeadFunction DefaultActivator { get; }
        protected abstract Head EvaluateValue(HeadLayer<TLayer> prev, HeadFunction activator, Loss loss, float lossWeight);
        protected abstract FSharpOption<HeadFunction> ParseActivator(IActivatorConverter converter, string value);
        protected abstract IEnumerable<string> GetLookupValues(IActivatorConverter converter);
    }

    public abstract class MultiDimensionalHeadViewModel<TLayer> : HeadViewModelBase<TLayer>
    {
        public MultiDimensionalHeadViewModel(
            IActivatorConverter activatorConverter,
            ILossConverter lossConverter, PortType port) : base(activatorConverter, lossConverter, port)
        {
        }

        protected override HeadFunction DefaultActivator =>
            HeadFunction.NewActivator(Activator.NewQuotedFunction(QuotedFunction.ReLu));

        protected override IEnumerable<string> GetLookupValues(IActivatorConverter converter)
        {
            return Array.ConvertAll(converter.ActivationFunctionNames, p => p.defn.Value);
        }

        protected override FSharpOption<HeadFunction> ParseActivator(IActivatorConverter converter, string value)
        {
            var activator = ResultExtensions.toOption(converter.Decode(value));
            return OptionModule.Map(FSharpFunc<Activator, HeadFunction>
                .FromConverter(a => HeadFunction.NewActivator(a)), activator);
        }
    }
}
