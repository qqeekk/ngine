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
    /// Interaction logic for UIntVector2DEditor.xaml
    /// </summary>
    public partial class UIntVector2DEditor : IViewFor<UIntVector2DEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(UIntVector2DEditorViewModel), typeof(UIntVector2DEditor), new PropertyMetadata(null));

        public UIntVector2DEditorViewModel ViewModel
        {
            get => (UIntVector2DEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (UIntVector2DEditorViewModel)value;
        }
        #endregion

        public UIntVector2DEditor()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.XEditorViewModel, v => v.xEditor.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.YEditorViewModel, v => v.yEditor.ViewModel).DisposeWith(d);
            });
        }
    }
}