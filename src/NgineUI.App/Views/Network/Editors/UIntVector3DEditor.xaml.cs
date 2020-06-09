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
    /// Interaction logic for UIntVector3DEditor.xaml
    /// </summary>
    public partial class UIntVector3DEditor : IViewFor<UIntVector3DEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(UIntVector3DEditorViewModel), typeof(UIntVector3DEditor), new PropertyMetadata(null));

        public UIntVector3DEditorViewModel ViewModel
        {
            get => (UIntVector3DEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (UIntVector3DEditorViewModel)value;
        }
        #endregion

        public UIntVector3DEditor()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.XEditorViewModel, v => v.xEditor.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.YEditorViewModel, v => v.yEditor.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ZEditorViewModel, v => v.zEditor.ViewModel).DisposeWith(d);
            });
        }
    }
}
