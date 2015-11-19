namespace Quantum.BuilderFactory

open Quantum.BuilderFactory.Parsing
open Quantum.BuilderFactory.Composing

module TransitionReader =

    type SqlFunctionInfo = { FunctionName: string; ResultType: TypeInfo; FunctionType: TypeInfo; Constructors: TransitionConstructor list }
    type DllReadingData = { Transitions: TransitionDetails list; Functions: SqlFunctionInfo list }

    module private Impl =
        open System
        open System.Reflection

        open Quantum.QueryBuilder.Common
        
        let toTypeInfo (t: Type) =
            { TypeName = t.Name; Namespace = t.Namespace }

        let isParams (parameter: ParameterInfo): bool =
            parameter.GetCustomAttributes(typedefof<System.ParamArrayAttribute>, false).Length > 0

        let listExportedTypes (assembly: System.Reflection.Assembly) = 
            assembly.GetExportedTypes()

        let createCtorArguments (ctor: ConstructorInfo) =
            ctor.GetParameters ()
            |> Seq.map (fun param -> { ArgumentName = param.Name; TypeInfo = param.ParameterType |> toTypeInfo; IsParams = param |> isParams })
            |> Seq.toList

        let createConstructors (t: Type): TransitionConstructor list =
            t.GetConstructors()
            |> Seq.map (fun ctor -> { TransitionConstructor.Arguments = ctor |> createCtorArguments })
            |> Seq.toList

        let createFunctionInfo (interfaceType: System.Type)(t:System.Type): SqlFunctionInfo =
            let resultType = t |> toTypeInfo
            { 
            ResultType = interfaceType.GetGenericArguments().[0] |> toTypeInfo; 
            FunctionType = resultType
            Constructors = t |> createConstructors; 
            FunctionName = resultType.TypeName.Replace("Function", "") 
            }

        let listFunctions (assembly: System.Reflection.Assembly) =
            assembly
            |> listExportedTypes
            |> Seq.choose (fun t -> 
                let interfaces = t.GetInterfaces()
                let sqlFunctionInterface =
                    interfaces
                    |> Seq.tryFind (fun it -> 
                        it.IsGenericType && (it.GetGenericTypeDefinition() = typedefof<Quantum.QueryBuilder.Common.ISqlFunction<_>>) )

                match sqlFunctionInterface with
                | Some it -> Some (t |> createFunctionInfo it)
                | _ -> None)
            |> Seq.toList

        let loadAssembly path =
            path
            |> Assembly.LoadFrom

        let listTransitionTypes (assembly: System.Reflection.Assembly) = 
            assembly
            |> fun assembly -> assembly.GetExportedTypes() 
            |> Seq.filter (fun t -> typedefof<Quantum.QueryBuilder.Common.IBuilderTransition>.IsAssignableFrom(t))
            |> Seq.toList
                                
        let createTransitionDetails (transitionType:Type): TransitionDetails  =
            { TypeInfo = transitionType |> toTypeInfo; Constructors = transitionType |> createConstructors }

        let readDll path = 
            let assembly =
                path
                |> loadAssembly

            let transitions = 
                assembly
                |> listTransitionTypes
                |> List.map createTransitionDetails

            let functions =
                assembly
                |> listFunctions

            { Transitions = transitions; Functions = functions }

    let readDll = Impl.readDll