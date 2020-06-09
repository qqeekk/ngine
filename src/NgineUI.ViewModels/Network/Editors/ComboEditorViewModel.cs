using DynamicData.Binding;
using NodeNetwork.Toolkit.ValueNode;
using System;
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

    //public class ComboEditorViewModel<T> : ComboEditorViewModel
    //{
    //    private readonly Func<string, T> func;

    //    public ComboEditorViewModel(IEnumerable<string> options, Func<string, T> func) : base(options)
    //    {
    //        base.Value = options.First();
    //        this.func = func;
    //    }

    //    public new T Value => func(base.Value);

    //    public new IObservable<T> ValueChanged => base.ValueChanged.Select(func);
    //}
}
