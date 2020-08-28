using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Domain.Utils;
using NgineUI.ViewModels.Network.Editors;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace NgineUI.ViewModels.Network.Ambiguities
{
    using Ambiguity = KeyValuePair<AmbiguityVariableName, Values<uint>>;

    public class AmbiguityViewModel : ReactiveObject
    {
        private readonly IAmbiguityConverter converter;

        public AmbiguityViewModel(IAmbiguityConverter converter)
        {
            this.converter = converter;
            ValueEditor = new LookupEditorViewModel<Values<uint>>(v => MapStateToAmbiguity(v), string.Empty)
            {
                LookupValues = converter.ListPattern.deps.SelectMany(d => OptionModule.ToArray(d.defn))
            };

            var nameObservable = this.WhenAnyValue(v => v.Name);

            AmbiguityChanged = Observable.CombineLatest(
                nameObservable,
                ValueEditor.ValueChanged,
                (name, value) =>
                {
                    var res = converter.Decode(new Schema.Ambiguity(name ?? "", value));
                    return ResultExtensions.toOption(res);
                });

            AmbiguityChanged.ToProperty(this, vm => vm.CurrentAmbiguity, out currentAmbiguity, scheduler: ImmediateScheduler.Instance);
        }


        #region CurrentValue
        /// <summary>
        /// The latest value produced by this output.
        /// </summary>
        public FSharpOption<Ambiguity> CurrentAmbiguity => currentAmbiguity.Value;
        private ObservableAsPropertyHelper<FSharpOption<Ambiguity>> currentAmbiguity;

        public IObservable<FSharpOption<Ambiguity>> AmbiguityChanged { get; }
        #endregion

        #region Name
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }
        private string name;
        #endregion

        public LookupEditorViewModel ValueEditor { get; }

        private FSharpOption<Values<uint>> MapStateToAmbiguity(string value)
        {
            var res = converter.DecodeValues(value);
            return ResultExtensions.toOption(res);
        }

        internal void ClearState()
        {
            Name = string.Empty;
            ValueEditor.Value = string.Empty;
        }
    }
}
