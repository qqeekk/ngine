using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Editors
{
    using UIntVector2D = Tuple<uint, uint>;

    public class UIntVector2DEditorViewModel : ValueEditorViewModel<UIntVector2D>
    {
        public UIntEditorViewModel XEditorViewModel { get; }
        public UIntEditorViewModel YEditorViewModel { get; }

        public UIntVector2DEditorViewModel()
        {
            XEditorViewModel = new UIntEditorViewModel();
            YEditorViewModel = new UIntEditorViewModel();

            this.WhenAnyValue(v => v.XEditorViewModel.Value, v => v.YEditorViewModel.Value)
                .Select(c => new UIntVector2D(c.Item1, c.Item2))
                .BindTo(this, v => v.Value);

            this.ValueChanged.Subscribe(v =>
            {
                XEditorViewModel.Value = v.Item1;
                YEditorViewModel.Value = v.Item2;
            });
        }
    }
}
