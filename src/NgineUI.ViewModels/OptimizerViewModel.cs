using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Domain.Utils;
using NgineUI.ViewModels.Network.Editors;
using System.Linq;

namespace NgineUI.ViewModels
{
    public class OptimizerViewModel
     {
        private readonly Optimizer defaultValue = Optimizer.NewAdam(1e-3f, new Adam(0.9f, 0.999f, 0));
        private readonly IOptimizerConverter converter;

        public OptimizerViewModel(IOptimizerConverter converter)
        {
            this.converter = converter;

            var defaultValue = converter.Encode(this.defaultValue);
            ValueEditor = new LookupEditorViewModel<Optimizer>(Convert, defaultValue)
            {
                LookupValues = converter.OptimizerNames.SelectMany(n => OptionModule.ToArray(n.defn)),
            };
        }


        public LookupEditorViewModel<Optimizer> ValueEditor { get; }

        private FSharpOption<Optimizer> Convert(string value)
        {
            var res = converter.Decode(value);
            return ResultExtensions.toOption(res);
        }

        internal void Fill(Optimizer optimizer)
        {
            ValueEditor.Value = converter.Encode(optimizer);
        }

        internal Optimizer GetValue()
            => OptionModule.DefaultValue(defaultValue, ValueEditor.SelectedValue);
    }
}
