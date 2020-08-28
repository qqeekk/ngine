using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using NgineUI.ViewModels.Network.Ambiguities;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Editors
{
    using AmbiguousUIntVector3D = Tuple<Ambiguous<uint>, Ambiguous<uint>, Ambiguous<uint>>;

    public class AmbiguousUIntVector3DEditorViewModel : ValueEditorViewModel<AmbiguousUIntVector3D>
    {
        public AmbiguousUIntEditorViewModel XEditorViewModel { get; }
        public AmbiguousUIntEditorViewModel YEditorViewModel { get; }
        public AmbiguousUIntEditorViewModel ZEditorViewModel { get; }

        public AmbiguousUIntVector3DEditorViewModel(AmbiguityListViewModel ambiguities)
        {
            XEditorViewModel = new AmbiguousUIntEditorViewModel(0.ToString(), ambiguities);
            YEditorViewModel = new AmbiguousUIntEditorViewModel(0.ToString(), ambiguities);
            ZEditorViewModel = new AmbiguousUIntEditorViewModel(0.ToString(), ambiguities);

            this.WhenAnyValue(v => v.XEditorViewModel.Value, v => v.YEditorViewModel.Value, v => v.ZEditorViewModel.Value)
                .Select(c => new AmbiguousUIntVector3D(
                    OptionModule.DefaultValue(AmbiguousUIntViewModel.Default, XEditorViewModel.SelectedValue),
                    OptionModule.DefaultValue(AmbiguousUIntViewModel.Default, YEditorViewModel.SelectedValue),
                    OptionModule.DefaultValue(AmbiguousUIntViewModel.Default, ZEditorViewModel.SelectedValue)))
                .BindTo(this, v => v.Value);

            this.ValueChanged.Subscribe(v =>
            {
                XEditorViewModel.Value = Ambiguous.stringify(v.Item1);
                YEditorViewModel.Value = Ambiguous.stringify(v.Item2);
                ZEditorViewModel.Value = Ambiguous.stringify(v.Item3);
            });
        }
    }
}
