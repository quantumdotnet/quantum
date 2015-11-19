namespace Quantum.BuilderFactory

module Composing =
    type TypeInfo = { TypeName: string; Namespace: string; }
    type TransitionConstructorArgument = { TypeInfo: TypeInfo; ArgumentName: string; IsParams: bool } // todo rename to general
    type TransitionConstructor = { Arguments: TransitionConstructorArgument list }
    type TransitionDetails = { TypeInfo: TypeInfo; Constructors: TransitionConstructor list }

    type BuilderNodeChild = { ClassName: string; Name: string; TransitionDetails: TransitionDetails }
    type BuilderNodeGenerationInfo = { ClassName: string; Children: BuilderNodeChild list; IsRoot: bool }

    open System;
    open Quantum.BuilderFactory.Parsing

    module private Impl =
        let sanitize input =
            input
            |> Utils.replace "_" ""

        (* composes class name basing on node name *)
        let formatNodeClassName (node:TransitionDef) = 
            let sanitizedCode = 
                node.Name 
                |> sanitize
            
            sanitizedCode + "Node"

        (* searches for System.Type corresponding to node transition *)
        let findTransitionDetails (transitions: TransitionDetails list) code =
            let canonicalCode = ((code |> sanitize) + "Transition").ToUpper()
            transitions 
            |> List.find(fun t -> t.TypeInfo.TypeName.ToUpper() = canonicalCode)

        let createBuilderChildNode (nodes: TransitionDef list) (transitions: TransitionDetails list) (code: string) =
            let className = 
                nodes 
                |> List.find(fun n -> n.Code = code) 
                |> formatNodeClassName

            let transitionDetails = findTransitionDetails transitions code
            { ClassName = className; Name = code; TransitionDetails = transitionDetails }

        let createBuilderNode (nodes: TransitionDef list) (transitions: TransitionDetails list) (node: TransitionDef) =
            let childrenFactory = createBuilderChildNode nodes transitions
            let children = 
                node.ChildCodes 
                |> List.map childrenFactory

            let className = node |> formatNodeClassName
            { ClassName=className; Children=children; IsRoot=className.StartsWith "Root" }

        let compose (nodes: TransitionDef list) (transitions: TransitionDetails list) =
            let factory = createBuilderNode nodes transitions
            nodes
            |> List.map factory

    let compose = Impl.compose