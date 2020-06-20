using System.Collections.ObjectModel;
using static Ngine.Domain.Schemas.Schema;

namespace NgineUI.ViewModels.Network
{
    public class AmbiguitiesViewModel
    {
        public ObservableCollection<Ambiguity> Items { get; } = new ObservableCollection<Ambiguity>();
        public ObservableCollection<string> Names { get; } = new ObservableCollection<string>();

        public void Add(Ambiguity item)
        {
            Items.Add(item);
            Names.Add(item.Name);
        }
    }
}
