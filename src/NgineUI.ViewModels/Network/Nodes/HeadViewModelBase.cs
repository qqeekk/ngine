using DynamicData;
using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Domain.Schemas.Expressions;
using Ngine.Domain.Utils;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Activator = Ngine.Domain.Schemas.Activator;

namespace NgineUI.ViewModels.Network.Nodes
{
    public abstract class HeadViewModelBase<TLayer, TActivator> : NgineNodeViewModel where TActivator : HeadFunction
    {
        public ValueNodeInputViewModel<string> ActivationEditor { get; }
        public ValueNodeInputViewModel<string> LossEditor { get; }
        public ValueNodeInputViewModel<float> LossWeightEditor { get; }
        public ValueNodeInputViewModel<HeadLayer<TLayer>> Previous { get; }
        public IObservable<Head> Output { get; }

        public HeadViewModelBase(
            IActivatorConverter activatorConverter,
            ILossConverter lossConverter,
            string name) : base(null, NodeType.Head, name, false)
        {
            var activationEditor = new LookupEditorViewModel<TActivator>(
                    v => ParseActivator(activatorConverter, v),
                    activatorConverter.EncodeHeadActivation(DefaultActivator))
            {
                LookupValues = GetLookupValues(activatorConverter)
            };

            ActivationEditor = new ValueNodeInputViewModel<string>
            {
                Name = "Activation",
                Editor = activationEditor,
            };
            ActivationEditor.Port.IsVisible = false;
            this.Inputs.Add(ActivationEditor); ;

            var lossNames = Array.ConvertAll(lossConverter.LossFunctionNames, p => p.name);
            LossEditor = new ValueNodeInputViewModel<string>
            {
                Name = "Loss",
                Editor = new ComboEditorViewModel(lossNames),
            };
            LossEditor.Port.IsVisible = false;
            this.Inputs.Add(LossEditor); ;

            LossWeightEditor = new ValueNodeInputViewModel<float>
            {
                Name = "Loss Weight",
                Editor = new FloatEditorViewModel(),
            };
            LossWeightEditor.Port.IsVisible = false;
            this.Inputs.Add(LossWeightEditor);

            Previous = new NgineInputViewModel<HeadLayer<TLayer>>(PortType.Head);
            this.Inputs.Add(Previous);

            Output = Observable.CombineLatest(
                    Previous.ValueChanged.Select(p => p ?? WrapEmpty(DefaultPrevious)),
                    ActivationEditor.ValueChanged.Select(v => OptionModule.DefaultValue(DefaultActivator, activationEditor.SelectedValue)),
                    LossEditor.ValueChanged.Select(v => lossConverter.DecodeLoss(v).ResultValue),
                    LossWeightEditor.ValueChanged,
                    EvaluateValue);
        }

        protected abstract TLayer DefaultPrevious { get; }
        protected abstract TActivator DefaultActivator { get; }
        protected abstract Head EvaluateValue(HeadLayer<TLayer> prev, TActivator activator, Loss loss, float lossWeight);
        protected abstract FSharpOption<TActivator> ParseActivator(IActivatorConverter converter, string value);
        protected abstract IEnumerable<string> GetLookupValues(IActivatorConverter converter);
    }

    public abstract class MultiDimensionalHeadViewModel<TLayer> : HeadViewModelBase<TLayer, HeadFunction.Activator>
    {
        public MultiDimensionalHeadViewModel(
            IActivatorConverter activatorConverter,
            ILossConverter lossConverter, string name) : base(activatorConverter, lossConverter, name)
        {
        }

        protected override HeadFunction.Activator DefaultActivator =>
            HeadFunction.NewActivator(Activator.NewQuotedFunction(QuotedFunction.ReLu)) as HeadFunction.Activator;

        protected override IEnumerable<string> GetLookupValues(IActivatorConverter converter)
        {
            return Array.ConvertAll(converter.ActivationFunctionNames, p => p.defn.Value);
        }

        protected override FSharpOption<HeadFunction.Activator> ParseActivator(IActivatorConverter converter, string value)
        {
            var activator = ResultExtensions.toOption(converter.Decode(value));
            return OptionModule.Map(FSharpFunc<Activator, HeadFunction.Activator>
                .FromConverter(a => HeadFunction.NewActivator(a) as HeadFunction.Activator), activator);
        }
    }
}
