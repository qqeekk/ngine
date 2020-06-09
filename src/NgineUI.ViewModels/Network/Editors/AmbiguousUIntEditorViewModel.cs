using Ngine.Domain.Schemas;
using NodeNetwork.Toolkit.ValueNode;
using System.Collections.ObjectModel;
using static Ngine.Domain.Schemas.Schema;

namespace NgineUI.ViewModels.Network.Editors
{
    public class AmbiguousUIntViewModel
    {
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

    public class AmbiguousUIntEditorViewModel : ValueEditorViewModel<AmbiguousUIntViewModel>
    {
        public ObservableCollection<Ambiguity> Ambiguities { get; }

        public AmbiguousUIntEditorViewModel(ObservableCollection<Ambiguity> ambiguities)
        {
            Ambiguities = ambiguities;
            Value = new AmbiguousUIntViewModel { Value = 0u };
        }
    }
}
