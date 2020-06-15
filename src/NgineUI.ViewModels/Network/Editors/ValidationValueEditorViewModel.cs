using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Helpers;
using System;
using System.Collections;
using System.ComponentModel;
using System.Reactive.Concurrency;

namespace NgineUI.ViewModels.Network.Editors
{
    internal class NaiveReactiveValidationObject<T> : ReactiveValidationObject<T>
    {
        public NaiveReactiveValidationObject(IScheduler scheduler = null) : base(scheduler) { }
    }

    public class ValidationValueEditorViewModel<T> : ValueEditorViewModel<T>, IValidatableViewModel, INotifyDataErrorInfo
    {
        private readonly ReactiveValidationObject<T> validationObject;

        protected ValidationValueEditorViewModel(IScheduler scheduler = null)
        {
            validationObject = new NaiveReactiveValidationObject<T>(scheduler);
            validationObject.ErrorsChanged += (o, args) => ErrorsChanged?.Invoke(this, args);
        }

        /// <inheritdoc />
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <inheritdoc />
        public bool HasErrors => validationObject.HasErrors;


        /// <inheritdoc />
        public ValidationContext ValidationContext => validationObject.ValidationContext;

        /// <inheritdoc />
        public virtual IEnumerable GetErrors(string propertyName) => validationObject.GetErrors(propertyName);
    }
}
