namespace Ngine.Domain.CodeGen

open Ngine.Domain.Schemas

type UnknownLanguage = UnknownLanguage of string

type NetworkCode = {
    Language: string
    Code: string
}

type INetworkCodeGenerator =
    abstract member GenerateFromDefinition: definition : Network -> Result<NetworkCode, string>

type ILanguageLocator =
    abstract member ResolveFor: language : string -> Result<INetworkCodeGenerator, UnknownLanguage>
