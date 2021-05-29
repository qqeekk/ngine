using NgineUI.ViewModels.Network.Editors;
using ReactiveUI;
using System.Windows;

namespace NgineUI.App.Views.Network.Editors
{
    /// <summary>
    /// Interaction logic for IntegerEditor.xaml
    /// </summary>
    public partial class UIntEditor : IViewFor<UIntEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(UIntEditorViewModel), typeof(UIntEditor), new PropertyMetadata(null));

        public UIntEditorViewModel ViewModel
        {
            get => (UIntEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (UIntEditorViewModel)value;
        }
        #endregion

        public UIntEditor()
        {
            InitializeComponent();

            this.WhenActivated(d => d(
                this.Bind(ViewModel,
                    vm => vm.Value,
                    v => v.editor.Text,
                    this.ViewModelToViewConverterFunc,
                    this.ViewToViewModelConverterFunc)
            ));
        }

        private string ViewModelToViewConverterFunc(uint value)
        {
            return value.ToString();
        }

        private uint ViewToViewModelConverterFunc(string value)
        {
            if (uint.TryParse(value, out var returnValue))
            {
                ExitErrorState(editor);
            }
            else
            {
                EnterErrorState(editor, "Недопустимое значение");
            }

            return returnValue;
        }
    }
}
