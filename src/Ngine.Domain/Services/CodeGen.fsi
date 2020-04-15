namespace Ngine.Domain.Services.CodeGen
open Ngine.Domain.CodeGen

module LanguageLocatorBuilder =
    val create : interpreters: (string * INetworkCodeGenerator) seq -> ILanguageLocator
