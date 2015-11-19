namespace Quantum.BuilderFactory

module CodeGenerating =
    module private Impl =
        open System
        open Quantum.BuilderFactory.Composing
        open Quantum.BuilderFactory.TransitionReader
        open Antlr4.StringTemplate
        
        let collectAllUsedTypes (functions: SqlFunctionInfo list) (nodes:BuilderNodeGenerationInfo list): TypeInfo list = 
            let allChildren = 
                nodes 
                |> List.collect (fun node -> node.Children)
            
            let nestedTypes =
                allChildren
                |> List.collect (fun node -> node.TransitionDetails.Constructors)
                |> List.collect (fun ctor -> ctor.Arguments)
                |> List.map (fun ctor -> ctor.TypeInfo)

            let primaryTypes =
                allChildren
                |> List.map (fun node -> node.TransitionDetails.TypeInfo)

            let functionTypes =
                functions
                |> List.map (fun fn -> fn.FunctionType)

            let functionResultTypes =
                functions
                |> List.map (fun fn -> fn.ResultType)

            (primaryTypes @ nestedTypes @ functionTypes @ functionResultTypes)
            |> Seq.distinct
            |> Seq.toList

        let formatNamespaces (nodes:BuilderNodeGenerationInfo list) (functions: SqlFunctionInfo list) = 
            nodes 
            |> collectAllUsedTypes functions
            |> List.map (fun t -> t.Namespace)
            |> List.append ["Quantum.QueryBuilder.Common"]
            |> Seq.sort
            |> Seq.distinct
            
        let generate (functions: SqlFunctionInfo list) (nodes:BuilderNodeGenerationInfo list) (version: string)  =
            let group = new TemplateGroupFile(System.IO.Directory.GetCurrentDirectory() + "/templates.stg")
            let st = group.GetInstanceOf("_tmpl_file")
                        
            do st.Add("name", "x") |> ignore
            do st.Add("functions", functions |> List.toArray) |> ignore
            do st.Add("nodes", nodes |> List.toArray) |> ignore
            do st.Add("usings", formatNamespaces nodes functions) |> ignore
            do st.Add("version", version) |> ignore

            st.Render()

    let generate = Impl.generate