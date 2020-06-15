using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Domain.Schemas.Expressions;
using Ngine.Domain.Utils;
using Ngine.Infrastructure.AppServices;
using System;
using System.Collections.Generic;
using static Ngine.Domain.Schemas.Head;
using Activator = Ngine.Domain.Schemas.Activator;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Head1DViewModel : HeadViewModelBase<Layer1D, HeadFunction>
    {
        public Head1DViewModel(
            IActivatorConverter activatorConverter,
            ILossConverter lossConverter) : base(activatorConverter, lossConverter, "Head1D")
        {
        }

        protected override HeadFunction DefaultActivator { get; } = HeadFunction.NewActivator(Activator.NewQuotedFunction(QuotedFunction.ReLu));

        protected override Head EvaluateValue(HeadLayer<Layer1D> prev, HeadFunction activator, Loss loss, float lossWeight)
        {
            prev ??= HeadLayer<Layer1D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer1D.Empty1D);

            if (activator.IsSoftmax)
            {
                return Head.NewSoftmax(lossWeight, loss, prev);
            }

            return Head.NewActivator(lossWeight, loss, HeadLayer.NewD1(prev), (activator as HeadFunction.Activator).Item);
        }

        protected override IEnumerable<string> GetLookupValues(IActivatorConverter converter)
        {
            return Array.ConvertAll(converter.HeadFunctionNames, p => p.defn.Value);
        }

        protected override FSharpOption<HeadFunction> ParseActivator(IActivatorConverter converter, string value)
        {
            return ResultExtensions.toOption(converter.DecodeHeadActivation(value));
        }
    }
}
