using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Editors
{
    using UIntVector3D = Tuple<uint, uint, uint>;

    public class UIntVector3DEditorViewModel : ValueEditorViewModel<UIntVector3D>
    {
        public UIntEditorViewModel XEditorViewModel { get; }
        public UIntEditorViewModel YEditorViewModel { get; }
        public UIntEditorViewModel ZEditorViewModel { get; }

        public UIntVector3DEditorViewModel()
        {
            XEditorViewModel = new UIntEditorViewModel();
            YEditorViewModel = new UIntEditorViewModel();
            ZEditorViewModel = new UIntEditorViewModel();

            this.WhenAnyValue(v => v.XEditorViewModel.Value, v => v.YEditorViewModel.Value, v => v.ZEditorViewModel.Value)
                .Select(c => new UIntVector3D(c.Item1, c.Item2, c.Item3))
                .BindTo(this, v => v.Value);
        }
    }
}
