﻿(*
    FSharpLint, a linter for F#.
    Copyright (C) 2014 Matthew Mcveigh

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*)

namespace FSharpLint.Rules

/// Rules to enforce the use of XML documentation in various places.
module XmlDocumentation =

    open System
    open Microsoft.FSharp.Compiler.Ast
    open FSharpLint.Framework.Ast
    open FSharpLint.Framework.Configuration
    open FSharpLint.Framework.LoadVisitors

    [<Literal>]
    let AnalyserName = "XmlDocumentation"

    let configExceptionHeader config name =
        match isRuleEnabled config AnalyserName name with
            | Some(_, ruleSettings) when ruleSettings.ContainsKey "Enabled" ->
                match ruleSettings.["Enabled"] with
                    | Enabled(true) -> true
                    | _ -> false
            | Some(_)
            | None -> false

    let isPreXmlDocEmpty (preXmlDoc:PreXmlDoc) =
        match preXmlDoc.ToXmlDoc() with
            | XmlDoc(xs) ->
                let atLeastOneThatHasText ars = ars |> Array.exists (fun s -> not (String.IsNullOrWhiteSpace(s))) |> not
                xs |> atLeastOneThatHasText

    let getString str = FSharpLint.Framework.Resources.GetString(str)

    let ruleEnabled visitorInfo (astNode:CurrentNode) ruleName =
        configExceptionHeader visitorInfo.Config ruleName &&
            astNode.IsSuppressed(AnalyserName, ruleName) |> not

    let getIdText (id:Ident option) =
        match id with
        | None -> ""
        | Some i -> " " + i.idText

    let visitor visitorInfo checkFile astNode =
        match astNode.Node with
        | AstNode.ModuleOrNamespace(SynModuleOrNamespace.SynModuleOrNamespace(_, _, _, xmlDoc, _, _, range)) ->
            if ruleEnabled visitorInfo astNode "ModuleDefinitionHeader" && isPreXmlDocEmpty xmlDoc then
                visitorInfo.PostError range (getString "RulesXmlDocumentationModuleError")

        | AstNode.ExceptionRepresentation(SynExceptionRepr.ExceptionDefnRepr(_, _, _, xmlDoc, _, range)) ->
            if ruleEnabled visitorInfo astNode "ExceptionDefinitionHeader" && isPreXmlDocEmpty xmlDoc then
                visitorInfo.PostError range (getString "RulesXmlDocumentationExceptionError")

        | AstNode.EnumCase(SynEnumCase.EnumCase(_, id, _, xmlDoc, range)) ->
            if ruleEnabled visitorInfo astNode "EnumDefinitionHeader" && isPreXmlDocEmpty xmlDoc then
                visitorInfo.PostError range (getString "RulesXmlDocumentationEnumError" + " " + id.idText)

        | AstNode.UnionCase(SynUnionCase.UnionCase(_, id, _, xmlDoc, _, range)) ->
            if ruleEnabled visitorInfo astNode "UnionDefinitionHeader" && isPreXmlDocEmpty xmlDoc then
                visitorInfo.PostError range (getString "RulesXmlDocumentationUnionError" + " " + id.idText)

        | AstNode.MemberDefinition(SynMemberDefn.Member(synBinding, _)) ->
            if ruleEnabled visitorInfo astNode "MemberDefinitionHeader" then
                let (SynBinding.Binding(_, _, _, _, _, xmlDoc, _, _, _, _, range, _)) = synBinding
                if isPreXmlDocEmpty xmlDoc then
                    visitorInfo.PostError range (getString "RulesXmlDocumentationMemberError")

        | AstNode.MemberDefinition(SynMemberDefn.AutoProperty(_, _, id, _, _, _, xmlDoc, _, _, rangeOpt, range)) ->
            if ruleEnabled visitorInfo astNode "AutoPropertyDefinitionHeader" then
                if isPreXmlDocEmpty xmlDoc then
                    visitorInfo.PostError range (getString "RulesXmlDocumentationAutoPropertyError")

        | AstNode.TypeDefinition(SynTypeDefn.TypeDefn(coreInfo, typeDefnRep, _, rng)) ->
            if ruleEnabled visitorInfo astNode "TypeDefinitionHeader" then
                let (SynComponentInfo.ComponentInfo(_, _, _, _, xmlDoc, _, _, range)) = coreInfo
                if isPreXmlDocEmpty xmlDoc then
                    visitorInfo.PostError range (getString "RulesXmlDocumentationTypeError")
            if ruleEnabled visitorInfo astNode "RecordDefinitionHeader" then
                let evalField (SynField.Field(_, _, id, _, _, xmlDoc, _, range)) =
                    if isPreXmlDocEmpty xmlDoc then
                        visitorInfo.PostError range (getString "RulesXmlDocumentationRecordError" + getIdText id)
                match typeDefnRep with
                | Simple(simple, range) ->
                    match simple with
                    | SynTypeDefnSimpleRepr.Record(_, fields, _) -> fields |> List.iter evalField
                    | _ -> ()
                | _ -> ()
        | _ -> ()

        Continue

    type RegisterXmlDocumentationVisitor() =
        let plugin =
            {
                Name = AnalyserName
                Visitor = Ast(visitor)
            }

        interface IRegisterPlugin with
            member this.RegisterPlugin with get() = plugin