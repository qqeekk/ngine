using System.Collections.ObjectModel;
using static Ngine.Domain.Schemas.Schema;

namespace NgineUI.ViewModels.Network
{
    public class AmbiguitiesViewModel
    {
        public ObservableCollection<Ambiguity> Items { get; set; }
    }
}
