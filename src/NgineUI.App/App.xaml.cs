using Ngine.Domain.Schemas;
using Ngine.Domain.Schemas.Expressions;
using NgineUI.App.Views.Network;
using NgineUI.App.Views.Network.Editors;
using NgineUI.ViewModels.Network;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NgineUI.ViewModels.Network.Nodes;
using NodeNetwork.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NgineUI.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Activation1DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Activation2DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Activation3DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Concatenation1DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Concatenation2DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Concatenation3DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Conv2DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Conv3DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Flatten2DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Flatten3DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Input1DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Input2DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Input3DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Pooling2DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Pooling3DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<Head1DViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<DenseViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new NgineNode(), typeof(IViewFor<NgineNodeViewModel>));

            Splat.Locator.CurrentMutable.Register(() => new NginePort(), typeof(IViewFor<NginePortViewModel>));

            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<NgineInputViewModel<NonHeadLayer<Layer1D, Sensor1D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<NgineInputViewModel<NonHeadLayer<Layer2D, Sensor2D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<NgineInputViewModel<NonHeadLayer<Layer3D, Sensor3D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<NgineInputViewModel<HeadLayer<Layer1D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<NgineInputViewModel<HeadLayer<Layer2D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<NgineInputViewModel<HeadLayer<Layer3D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<NgineListInputViewModel<NonHeadLayer<Layer1D, Sensor1D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<NgineListInputViewModel<NonHeadLayer<Layer2D, Sensor2D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<NgineListInputViewModel<NonHeadLayer<Layer3D, Sensor3D>>>));

            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<NgineOutputViewModel<NonHeadLayer<Layer1D, Sensor1D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<NgineOutputViewModel<NonHeadLayer<Layer2D, Sensor2D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<NgineOutputViewModel<NonHeadLayer<Layer3D, Sensor3D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<NgineOutputViewModel<HeadLayer<Layer1D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<NgineOutputViewModel<HeadLayer<Layer2D>>>));
            Splat.Locator.CurrentMutable.Register(() => new NodeOutputView(), typeof(IViewFor<NgineOutputViewModel<HeadLayer<Layer3D>>>));

            Splat.Locator.CurrentMutable.Register(() => new AmbiguousUIntEditor(), typeof(IViewFor<AmbiguousUIntEditorViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new AmbiguousUIntVector2DEditor(), typeof(IViewFor<AmbiguousUIntVector2DEditorViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new AmbiguousUIntVector3DEditor(), typeof(IViewFor<AmbiguousUIntVector3DEditorViewModel>));
            
            Splat.Locator.CurrentMutable.Register(() => new ComboEditor(), typeof(IViewFor<ComboEditorViewModel>));

            Splat.Locator.CurrentMutable.Register(() => new UIntEditor(), typeof(IViewFor<UIntEditorViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new UIntVector2DEditor(), typeof(IViewFor<UIntVector2DEditorViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new UIntVector3DEditor(), typeof(IViewFor<UIntVector3DEditorViewModel>));
        }
    }
}
