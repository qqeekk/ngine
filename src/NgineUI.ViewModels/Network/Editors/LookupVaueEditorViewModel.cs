using Microsoft.FSharp.Core;
using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NgineUI.ViewModels.Network.Editors
{
    public class LookupEditorViewModel : ValidationValueEditorViewModel<string>
    {
        public IEnumerable<string> LookupValues { get; set; }
    }

    public class LookupEditorViewModel<T> : LookupEditorViewModel
    {
        private readonly Func<string, FSharpOption<T>> converter;

        public LookupEditorViewModel(Func<string, FSharpOption<T>> converter, string initial)
        {
            this.converter = converter;
            this.ValidationRule<LookupEditorViewModel, string>(vm => vm.Value,
                v => FSharpOption<T>.get_IsSome(SelectedValue),
                "Неверный формат данных");

            Value = initial;
        }

        public FSharpOption<T> SelectedValue => converter(Value ?? "");
    }
}
