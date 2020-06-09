using DynamicData;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Head1DViewModel : NgineNodeViewModel
    {
        public ValueNodeInputViewModel<string> ActivationEditor { get; }
        public ValueNodeInputViewModel<string> LossEditor { get; }
        public ValueNodeInputViewModel<HeadLayer<Layer1D>> Previous { get; }
        public ValueNodeOutputViewModel<Head> Output { get; }

        public Head1DViewModel(
            IActivatorConverter activatorConverter,
            ILossConverter lossConverter) : base(null, NodeType.Head, "Head1D", false)
        {
            var functionNames = Array.ConvertAll(activatorConverter.HeadFunctionNames, p => p.name);
            ActivationEditor = new ValueNodeInputViewModel<string>
            {
                Name = "Activation",
                Editor = new ComboEditorViewModel(functionNames),
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

            Previous = new NgineInputViewModel<HeadLayer<Layer1D>>(PortType.Head);
            this.Inputs.Add(Previous);

            Output = new NgineOutputViewModel<Head>(PortType.Head)
            {
                Value = Observable.CombineLatest(
                    Previous.ValueChanged,
                    ActivationEditor.ValueChanged.Select(v => activatorConverter.DecodeHeadActivation(v).ResultValue),
                    LossEditor.ValueChanged.Select(v => lossConverter.DecodeLoss(v).ResultValue),
                    (prev, activation, loss) =>
                    {
                        prev ??= HeadLayer<Layer1D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer1D.Empty1D);

                        if (activation.IsSoftmax)
                        {
                            return Head.NewSoftmax(1, loss, prev);
                        }

                        return Head.NewActivator(1, loss, HeadLayer.NewD1(prev), (activation as HeadFunction.Activator).Item);
                    })
            };
            Output.Port.IsVisible = false;
            this.Outputs.Add(Output);
        }
    }
}
