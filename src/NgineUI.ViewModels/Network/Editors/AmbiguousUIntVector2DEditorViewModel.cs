using Ngine.Domain.Schemas;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Editors
{
    using static Ngine.Domain.Schemas.Schema;
    using AmbiguousUIntVector2D = Tuple<Ambiguous<uint>, Ambiguous<uint>>;

    public class AmbiguousUIntVector2DEditorViewModel: ValueEditorViewModel<AmbiguousUIntVector2D>
    {
        public AmbiguousUIntEditorViewModel XEditorViewModel { get; }
        public AmbiguousUIntEditorViewModel YEditorViewModel { get; }

        public AmbiguousUIntVector2DEditorViewModel(ObservableCollection<Ambiguity> ambiguities)
        {
            XEditorViewModel = new AmbiguousUIntEditorViewModel(ambiguities);
            YEditorViewModel = new AmbiguousUIntEditorViewModel(ambiguities);

            this.WhenAnyValue(v => v.XEditorViewModel.Value, v => v.YEditorViewModel.Value)
                .Select(c => new AmbiguousUIntVector2D(c.Item1, c.Item2))
                .BindTo(this, v => v.Value);
        }
    }
}
