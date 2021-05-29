using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using ReactiveUI;
using System.Reactive.Linq;
using System.Windows.Input;

namespace NgineUI.ViewModels.Network.Ambiguities
{
    public class AmbiguitiesViewModel : ReactiveObject
    {
        private AmbiguityListViewModel ambiguityList;

        public AmbiguitiesViewModel(IAmbiguityConverter converter)
        {
            CurrentAmbiguity = new AmbiguityViewModel(converter);
            AmbiguityList = new AmbiguityListViewModel(converter);

            AddAmbiguityCommand = ReactiveCommand.Create(
                canExecute: CurrentAmbiguity.AmbiguityChanged.Select(OptionModule.IsSome),
                execute: () =>
                {
                    AmbiguityList.Add(CurrentAmbiguity.CurrentAmbiguity.Value);
                    CurrentAmbiguity.ClearState();
                });
        }

        public AmbiguityViewModel CurrentAmbiguity { get; }
        public AmbiguityListViewModel AmbiguityList 
        {
            get => ambiguityList;
            set => this.RaiseAndSetIfChanged(ref ambiguityList, value);
        }

        public ICommand AddAmbiguityCommand { get; }
    }
}
