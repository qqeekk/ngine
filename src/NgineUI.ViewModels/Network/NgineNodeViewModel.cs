using DynamicData.Binding;
using Ngine.Infrastructure.AppServices;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network
{
    public enum NodeType
    {
        Layer,
        Head,
        Input,
    }

    public class NgineNodeViewModel : NodeViewModel
    {
        private Tuple<uint, uint> id;

        public Tuple<uint, uint> Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }

        public NodeType NodeType { get; }

        public NgineNodeViewModel(LayerIdTracker idTracker, NodeType type, string name, bool setId)
        {
            this.WhenValueChanged(vm => vm.Id)
                .Where(id => id != null)
                .Subscribe(id => this.Name = name + (id.Item1 != 0 ? $" ({id.Item1}-{id.Item2})" : ""));

            Id = setId ? idTracker.Generate(0u) : Tuple.Create(0u, 0u);
            NodeType = type;
        }
    }
}
