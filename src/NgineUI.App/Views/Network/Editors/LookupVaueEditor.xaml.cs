using NgineUI.ViewModels.Network.Editors;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System.Reactive.Disposables;
using System.Windows;

namespace NgineUI.App.Views.Network.Editors
{
    /// <summary>
    /// Interaction logic for GenericLookupVaueEditor.xaml
    /// </summary>
    public partial class LookupVaueEditor : IViewFor<LookupEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(LookupEditorViewModel), typeof(LookupVaueEditor), new PropertyMetadata(null));

        public LookupEditorViewModel ViewModel
        {
            get => (LookupEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (LookupEditorViewModel)value;
        }
        #endregion

        private string validationErrorText;
        private string ValidationErrorText
        {
            set
            {
                if (!string.IsNullOrEmpty(validationErrorText = value))
                {
                    EnterErrorState(editor, validationErrorText);
                }
                else
                {
                    ExitErrorState(editor);
                }
            }
            get => validationErrorText;
        }

        public LookupVaueEditor()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel,
                    vm => vm.LookupValues,
                    v => v.editor.ItemsSource).DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.Value,
                    v => v.editor.Text,
                    vm => vm,
                    v => (string)v).DisposeWith(d);

                this.BindValidation(ViewModel, vm => vm.Value, v => v.ValidationErrorText)
                    .DisposeWith(d);
            });
        }
    }
}
