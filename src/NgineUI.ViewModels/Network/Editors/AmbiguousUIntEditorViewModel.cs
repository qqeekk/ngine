using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using NgineUI.ViewModels.Network.Ambiguities;
using System.Collections.Generic;
using System.Linq;

namespace NgineUI.ViewModels.Network.Editors
{
    public class AmbiguousUIntViewModel
    {
        public static AmbiguousUIntViewModel Default => new AmbiguousUIntViewModel { Value = 0 };

        public string Ambiguity { get; set; }

        public uint? Value { get; set; }

        public static implicit operator Ambiguous<uint> (AmbiguousUIntViewModel @this)
        {
            if (@this.Value is uint v)
            {
                return Ambiguous<uint>.NewFixed(v);
            }

            return Ambiguous<uint>.NewRefName(AmbiguityVariableName.NewVariable(@this.Ambiguity));
        }
    }

    public class AmbiguousUIntEditorViewModel : LookupEditorViewModel<AmbiguousUIntViewModel>
    {
        private static FSharpOption<AmbiguousUIntViewModel> Convert(string value, AmbiguityListViewModel ambiguities)
        {
            if (uint.TryParse(value, out var returnValue))
            {
                return FSharpOption<AmbiguousUIntViewModel>.Some(
                    new AmbiguousUIntViewModel { Value = returnValue });
            };

            if (ambiguities.Names.Contains(value))
            {
                return FSharpOption<AmbiguousUIntViewModel>.Some(
                    new AmbiguousUIntViewModel { Ambiguity = value });
            }

            return FSharpOption<AmbiguousUIntViewModel>.None;
        }

        public AmbiguousUIntEditorViewModel(string initial, AmbiguityListViewModel ambiguities)
            : base(v => Convert(v, ambiguities), initial)
        {
            LookupValues = ambiguities.Names;
        }
    }

    //public class AmbiguousUIntEditorViewModel : ValueEditorViewModel<AmbiguousUIntViewModel>
    //{
    //    public ObservableCollection<Ambiguity> Ambiguities { get; }

    //    public AmbiguousUIntEditorViewModel(ObservableCollection<Ambiguity> ambiguities)
    //    {
    //        Ambiguities = ambiguities;
    //        Value = new AmbiguousUIntViewModel { Value = 0u };
    //    }
    //}
}
