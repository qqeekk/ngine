using NgineUI.ViewModels.Network.Editors;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NgineUI.App.Views.Network.Editors
{
    /// <summary>
    /// Interaction logic for AmbiguousIntegerEditor.xaml
    /// </summary>
    public partial class AmbiguousUIntEditor : IViewFor<AmbiguousUIntEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(AmbiguousUIntEditorViewModel), typeof(AmbiguousUIntEditor), new PropertyMetadata(null));

        public AmbiguousUIntEditorViewModel ViewModel
        {
            get => (AmbiguousUIntEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AmbiguousUIntEditorViewModel)value;
        }
        #endregion

        public AmbiguousUIntEditor()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel,
                    vm => vm.Ambiguities,
                    v => v.editor.ItemsSource).DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.Value,
                    v => v.editor.Text,
                    vm => ViewModelToViewConverterFunc(vm),
                    v => ViewToViewModelConverterFunc((string)v)).DisposeWith(d);
            });
        }

        private string ViewModelToViewConverterFunc(AmbiguousUIntViewModel value)
        {
            return value.Value?.ToString() ?? value.Ambiguity;
        }

        private AmbiguousUIntViewModel ViewToViewModelConverterFunc(string value)
        {
            if (uint.TryParse(value, out var returnValue))
            {
                return new AmbiguousUIntViewModel { Value = returnValue };
            };

            return new AmbiguousUIntViewModel { Ambiguity = value };
        }
    }
}
