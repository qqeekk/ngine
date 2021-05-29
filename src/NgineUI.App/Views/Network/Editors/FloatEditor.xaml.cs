using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System.Globalization;
using System.Windows;

namespace NgineUI.App.Views.Network.Editors
{
    /// <summary>
    /// Interaction logic for FloatEditor.xaml
    /// </summary>
    public partial class FloatEditor : IViewFor<FloatEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(FloatEditorViewModel), typeof(FloatEditor), new PropertyMetadata(null));

        public FloatEditorViewModel ViewModel
        {
            get => (FloatEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FloatEditorViewModel)value;
        }
        #endregion

        public FloatEditor()
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

        private string ViewModelToViewConverterFunc(float value)
        {
            if (string.IsNullOrWhiteSpace(editor.Text))
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            return editor.Text;
        }

        private float ViewToViewModelConverterFunc(string value)
        {
            if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var returnValue)
                && returnValue >= 0
                && returnValue <= 1)
            {
                ExitErrorState(editor);
                return returnValue;
            }
            else
            {
                EnterErrorState(editor, "Недопустимое значение");
                return 0f;
            }
        }
    }
}
