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
    using AmbiguousUIntVector2D = Tuple<Ambiguous<uint>, Ambiguous<uint>>;

    public class AmbiguousUIntVector2DEditorViewModel: ValueEditorViewModel<AmbiguousUIntVector2D>
    {
        public AmbiguousUIntEditorViewModel XEditorViewModel { get; }
        public AmbiguousUIntEditorViewModel YEditorViewModel { get; }

        public AmbiguousUIntVector2DEditorViewModel(AmbiguityListViewModel ambiguities)
        {
            var fallbackValue = new AmbiguousUIntViewModel { Value = 0 };

            XEditorViewModel = new AmbiguousUIntEditorViewModel(0.ToString(), ambiguities);
            YEditorViewModel = new AmbiguousUIntEditorViewModel(0.ToString(), ambiguities);

            this.WhenAnyValue(v => v.XEditorViewModel.Value, v => v.YEditorViewModel.Value)
                .Select(c => new AmbiguousUIntVector2D(
                    OptionModule.DefaultValue(fallbackValue, XEditorViewModel.SelectedValue),
                    OptionModule.DefaultValue(fallbackValue, YEditorViewModel.SelectedValue)))
                .BindTo(this, v => v.Value);

            this.ValueChanged.Subscribe(v =>
            {
                XEditorViewModel.Value = Ambiguous.stringify(v.Item1);
                YEditorViewModel.Value = Ambiguous.stringify(v.Item2);
            });
        }
    }
}
