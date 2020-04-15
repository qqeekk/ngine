namespace Ngine.Domain.Services.CodeGen
open Ngine.Domain.Utils
open Ngine.Domain.CodeGen

module LanguageLocatorBuilder =
    let create (NotNull "interpreters" interpreters: _ seq) =
        let interpreters = dict interpreters

        let resolveFor (NotNull "lang" lang) = 
            match interpreters.TryGetValue lang with
            | true, interpreter -> Ok interpreter
            | false, _ -> Error (UnknownLanguage lang)

        {new ILanguageLocator with
            member _.ResolveFor lang = resolveFor lang }
