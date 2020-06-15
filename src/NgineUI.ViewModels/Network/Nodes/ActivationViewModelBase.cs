using DynamicData;
using DynamicData.Binding;
using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Domain.Schemas.Expressions;
using Ngine.Domain.Services.Conversion;
using Ngine.Domain.Utils;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Nodes
{
    public abstract class ActivationViewModelBase<TLayer, TSensor> : NgineNodeViewModel
    {
        public ValueNodeInputViewModel<string> ActivationEditor { get; }
        public ValueNodeInputViewModel<NonHeadLayer<TLayer, TSensor>> Previous { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<TLayer, TSensor>> Output { get; }
        public ValueNodeOutputViewModel<HeadLayer<TLayer>> HeadOutput { get; }

        public ActivationViewModelBase(
            IActivatorConverter activatorConverter,
            LayerIdTracker idTracker,
            PortType port,
            string name, bool setId) : base(idTracker, NodeType.Layer, name, setId)
        {
            var defaultActivator = Ngine.Domain.Schemas.Activator.NewQuotedFunction(QuotedFunction.ReLu);
            var activationEditor = new LookupEditorViewModel<Ngine.Domain.Schemas.Activator>(
                    v => ResultExtensions.toOption(activatorConverter.Decode(v)),
                    activatorConverter.Encode(defaultActivator))
            {
                LookupValues = Array.ConvertAll(activatorConverter.ActivationFunctionNames, p => p.defn.Value)
            };

            ActivationEditor = new ValueNodeInputViewModel<string>
            {
                Name = "Activation",
                Editor = activationEditor,
            };
            ActivationEditor.Port.IsVisible = false;
            this.Inputs.Add(ActivationEditor); ;

            Previous = new NgineInputViewModel<NonHeadLayer<TLayer, TSensor>>(port);
            this.Inputs.Add(Previous);

            Previous.ValueChanged
                .Where(i => i != null)
                .Select(i => NetworkConverters.getLayerId(i).Item1)
                .Subscribe(prevId => this.Id = (prevId + 1u != Id.Item1) ? idTracker.Generate(prevId) : Id);

            HeadOutput = new NgineOutputViewModel<HeadLayer<TLayer>>(PortType.Head)
            {
                Value = Observable.CombineLatest(
                    this.WhenValueChanged(vm => vm.Id),
                    ActivationEditor.ValueChanged.Select(v => OptionModule.DefaultValue(defaultActivator, activationEditor.SelectedValue)),
                    (id, activation) => HeadLayer<TLayer>.NewHeadLayer(id, EvaluateOutput(activation)))
            };

            Output = new NgineOutputViewModel<NonHeadLayer<TLayer, TSensor>>(port)
            {
                Value = HeadOutput.Value.Select(o => NonHeadLayer<TLayer, TSensor>.NewLayer(o))
            };

            this.Outputs.Add(Output);
            this.Outputs.Add(HeadOutput);
        }

        protected abstract TLayer EvaluateOutput(Ngine.Domain.Schemas.Activator function);
    }
}
