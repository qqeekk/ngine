namespace Ngine.Backend.Converters

module internal NetworkConverter =

    open Keras.Models
    open Ngine.Backend.Converters
    open Ngine.Domain.Schemas

    let internal keras network =
        let model = new Sequential()

        do Seq.iter (KernelConverter.keras >> model.Add) network.Layers
        model :> BaseModel
