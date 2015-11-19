namespace Quantum.BuilderFactory

module Parsing =
    type TransitionDef = {
        Code:string; 
        ChildCodes:string list;
        Name: string
    }

    type TransitionMapParser = string -> TransitionDef list

    module private Impl =
        type TransitionNode = { Code:string; IsOptional:bool; ParentCode: Option<string> }

        type NodePair = { master: string; slave: string; isOptional: bool }
        type InheritancePair = { Child: string; Parent: string; }
        
        (* ==================================================
           = Helper functions
           ================================================== *)

        (* code selector *)
        let takeCode node = node.Code

        (* parses single line part *)
        let parseLineItem (item:string) =
            let trimmedCode = item.Trim()
            let inheritanceParts = trimmedCode |> Utils.split [|":"|]
            let hasParent = inheritanceParts.Length > 1
            let code = Array.get inheritanceParts 0

            { 
            Code=code.TrimEnd '?'; 
            IsOptional=trimmedCode.EndsWith "?";
            ParentCode = if hasParent then Some (Array.get inheritanceParts 1) else None
            }

        (* parses map line*)
        let parseLine (line:string) = 
            line
            |> Utils.split([|"=>"|])
            |> Seq.map parseLineItem 
            |> Seq.toList

        (* transforms a list of parsed lines to a list of pairs master-slave *)
        let getAllPairs (lines: TransitionNode list list) =
            lines
            |> List.collect (fun line -> 
                (line |> List.rev).Tail 
                |> List.rev
                |> List.zip (line.Tail)
                |> List.map (fun (slave, master) -> { master = master.Code; slave = slave.Code; isOptional = slave.IsOptional }))

        (* lists inheritance pairs in form of parent-child *)
        let getInheritancePairs (lines: TransitionNode list list) =
            lines
            |> List.collect (fun line -> line)
            |> List.collect (fun node -> 
                match node.ParentCode with
                | Some parent -> [{ Parent = parent; Child = node.Code }]
                | _ -> [])

        (* composes node name from a list corresponding to a single nodes line *)
        let getName currentCode nodes = 
            let suffix:string = 
                nodes 
                    |> Seq.takeWhile (fun node -> node.Code <> currentCode) 
                    |> Seq.filter (fun node -> not node.IsOptional) 
                    |> Seq.filter (fun node -> node.Code <> "ROOT") 
                    |> Seq.map (takeCode >> Utils.makeCamelcase)
                    |> String.concat ""

            suffix + (currentCode |> Utils.makeCamelcase)

        (* composes node name using all node lines *)
        let getNameGlobal currentCode (nodes: TransitionNode list list) =
            nodes 
            |> List.map (getName currentCode)
            |> List.minBy (fun name -> name.Length)
        
        (* searches for node children by code using pairs list *)
        let rec findChildren currentCode pairs =
            pairs 
            |> List.filter (fun pair -> pair.master = currentCode)
            |> List.collect (fun pair ->
                if pair.isOptional then pair.slave :: findChildren pair.slave pairs else [pair.slave])

        let findChildrenWithInheritors currentCode pairs (inheritancePairs:InheritancePair list) = 
            let regularChildren = findChildren currentCode pairs

            let parent = 
                inheritancePairs
                |> Seq.tryFind (fun x -> x.Child = currentCode)

            match parent with
            | Some pair -> regularChildren @ (findChildren pair.Parent pairs)
            | _ -> regularChildren

        (* just a factory function *)
        let createNode allPairs lines inheritances code =
            { Code=code; ChildCodes=findChildrenWithInheritors code allPairs inheritances; Name=getNameGlobal code lines }

        (* ==================================================
           = Primary parser implementation
           ================================================== *)
        let defaultParser input = 
            let lines =                 
                input
                |> Utils.splitLines
                |> Array.toList
                |> List.map parseLine

            let allCodes = 
                lines
                |> List.collect (fun x -> x)
                |> Seq.map takeCode 
                |> Seq.distinct
    
            let allPairs = lines |> getAllPairs
            let allInheritances = lines |> getInheritancePairs
            let factory = createNode allPairs lines allInheritances

            allCodes |> Seq.map factory |> Seq.toList

    let parse: TransitionMapParser = Impl.defaultParser