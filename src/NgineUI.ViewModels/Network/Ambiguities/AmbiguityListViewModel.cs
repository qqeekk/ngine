using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Domain.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NgineUI.ViewModels.Network.Ambiguities
{
    using Ambiguity = KeyValuePair<AmbiguityVariableName, Values<uint>>;

    public class AmbiguityListViewModel
    {
        private readonly IAmbiguityConverter converter;
        private readonly ObservableCollection<Schema.Ambiguity> items;
        private readonly ObservableCollection<string> names;

        public AmbiguityListViewModel(IAmbiguityConverter converter)
        {
            this.converter = converter;
            this.items = new ObservableCollection<Schema.Ambiguity>();
            this.names = new ObservableCollection<string>();
        }

        internal IEnumerable<string> Names => names;
        public IEnumerable<Schema.Ambiguity> Items => items;

        public void Add(Ambiguity item)
        {
            var encoded = converter.Encode(item);
            items.Add(encoded);
            names.Add(encoded.Name);
        }

        public void Clear()
        {
            items.Clear();
            names.Clear();
        }

        public void Fill(IEnumerable<Ambiguity> ambiguities)
        {
            Clear();
            foreach (var item in ambiguities)
            {
                Add(item);
            }
        }

        public IEnumerable<Ambiguity> GetValues()
        {
            foreach (var item in Items)
            {
                var encodedResult = converter.Decode(item);
                var encodedOption = ResultExtensions.toOption(encodedResult);

                if (OptionModule.IsSome(encodedOption))
                {
                    yield return encodedOption.Value;
                }
            }
        }
    }
}
