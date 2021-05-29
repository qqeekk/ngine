using NodeNetwork.Toolkit.ValueNode;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Editors
{
    public class ComboEditorViewModel : ValueEditorViewModel<string>
    {
        public IEnumerable<string> Options { get; }

        public ComboEditorViewModel(IEnumerable<string> options)
        {
            Options = options;
            Value = options.First();
        }
    }
}
