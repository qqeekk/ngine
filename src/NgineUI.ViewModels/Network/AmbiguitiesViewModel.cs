using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Domain.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Ngine.Domain.Schemas.Schema;

namespace NgineUI.ViewModels.Network
{
    public class AmbiguitiesViewModel
    {
        private readonly IAmbiguityConverter converter;

        public ObservableCollection<Ambiguity> Items { get; } = new ObservableCollection<Ambiguity>();
        public ObservableCollection<string> Names { get; } = new ObservableCollection<string>();

        public AmbiguitiesViewModel(IAmbiguityConverter converter)
        {
            this.converter = converter;
        }


        public void Add(KeyValuePair<AmbiguityVariableName, Values<uint>> item)
        {
            var encoded = converter.Encode(item);
            Items.Add(encoded);
            Names.Add(encoded.Name);
        }

        public IEnumerable<KeyValuePair<AmbiguityVariableName, Values<uint>>> GetValues()
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
