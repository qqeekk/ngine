using Ngine.Domain.Schemas;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Editors
{
    using static Ngine.Domain.Schemas.Schema;
    using AmbiguousUIntVector3D = Tuple<Ambiguous<uint>, Ambiguous<uint>, Ambiguous<uint>>;

    public class AmbiguousUIntVector3DEditorViewModel : ValueEditorViewModel<AmbiguousUIntVector3D>
    {
        public AmbiguousUIntEditorViewModel XEditorViewModel { get; }
        public AmbiguousUIntEditorViewModel YEditorViewModel { get; }
        public AmbiguousUIntEditorViewModel ZEditorViewModel { get; }

        public AmbiguousUIntVector3DEditorViewModel(ObservableCollection<Ambiguity> ambiguities)
        {
            XEditorViewModel = new AmbiguousUIntEditorViewModel(ambiguities);
            YEditorViewModel = new AmbiguousUIntEditorViewModel(ambiguities);
            ZEditorViewModel = new AmbiguousUIntEditorViewModel(ambiguities);

            this.WhenAnyValue(v => v.XEditorViewModel.Value, v => v.YEditorViewModel.Value, v => v.ZEditorViewModel.Value)
                .Select(c => new AmbiguousUIntVector3D(c.Item1, c.Item2, c.Item3))
                .BindTo(this, v => v.Value);
        }
    }
}
