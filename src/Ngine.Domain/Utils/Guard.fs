namespace Ngine.Domain.Utils
open System

[<AutoOpen>]
module internal Guard =
    let (|NotNull|) identifier value =
        if obj.ReferenceEquals(value, null) then raise (ArgumentNullException identifier) else value
