using NgineUI.ViewModels.Network.Editors;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace NgineUI.App.Views.Network.Editors
{
    /// <summary>
    /// Interaction logic for StringComboEditor.xaml
    /// </summary>
    public partial class ComboEditor : IViewFor<ComboEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(ComboEditorViewModel), typeof(ComboEditor), new PropertyMetadata(null));

        public ComboEditorViewModel ViewModel
        {
            get => (ComboEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ComboEditorViewModel)value;
        }
        #endregion

        public ComboEditor()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Options, v => v.editor.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Value, v => v.editor.SelectedItem).DisposeWith(d);
            });
        }
    }
}
