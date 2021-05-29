namespace Ngine.Domain.Utils
open System.Collections.Generic

type internal BijectiveMap<'a, 'b when 'a : equality and 'b : equality>(input : _[]) =
    let initSet comparer = HashSet<_>(input, comparer)

    let tryGet (set : HashSet<_>) =
        set.TryGetValue
        >> function
        | true, value -> Some value
        | false, _ -> None

    let leftSideComparedValues = initSet {
        new IEqualityComparer<'a * 'b> with
            member _.Equals ((aleft, _), (bleft, _)) = (aleft = bleft)
            member _.GetHashCode ((left, _)) =  left.GetHashCode()
        }

    let rightSideComparedValues = initSet {
        new IEqualityComparer<'a * 'b> with
            member _.Equals ((_, aright), (_, bright)) = (aright = bright)
            member _.GetHashCode ((_, right)) = right.GetHashCode()
        }

    member _.TryGetRight left =
        (left, Unchecked.defaultof<'b>) 
        |> tryGet leftSideComparedValues
        |> Option.map (snd)

    member _.TryGetLeft right =
        (Unchecked.defaultof<'a>, right) 
        |> tryGet rightSideComparedValues
        |> Option.map (fst)

    interface IEnumerable<'a * 'b> with
        member _.GetEnumerator() = input.GetEnumerator()
        member _.GetEnumerator() = (Array.toSeq input).GetEnumerator()
