// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
module BuilderFactory 

open Quantum.BuilderFactory.Common
open Quantum.BuilderFactory.Parsing
open Quantum.BuilderFactory.Composing
open Quantum.BuilderFactory.TransitionReader
open Quantum.BuilderFactory.CodeGenerating
open System

let cprintfn c fmt = 
    Printf.kprintf 
        (fun s -> 
            let old = System.Console.ForegroundColor 
            try 
              System.Console.ForegroundColor <- c;
              System.Console.WriteLine s
            finally
              System.Console.ForegroundColor <- old) 
        fmt 
   
let indent (input: string) =
    input
    |> Utils.splitLines
    |> Array.map (fun x -> " |  " + x)
    |> String.concat "\r\n"

type Input = { OutputPath: string; InputDllPath: string; MapPath: string; }

let readFileContent path =
    if IO.File.Exists path then Success (IO.File.ReadAllText path) else Failure (sprintf "File %s not found" path, -100)

let readInputArgs args =
    match args with
    | [| dllPath; mapPath; outputPath|] ->
        Success {
            OutputPath = outputPath
            InputDllPath = dllPath 
            MapPath = mapPath
            }
    | _ -> Failure ("Wrong input arguments. The BuilderFactory tool expect 3 arguments as input parameters", -10)

let readDllData path =
    Success (readDll path)

let parseMap mapContent =
    Success (parse mapContent)

let getVersion = "1.0.0.0"

[<EntryPoint>]
let main argv = 
    
    let programResult: Result<int, _> =
        argv
        |> readInputArgs
        >>= (fun input -> Success (input, getVersion))
        >>=> (fun (_, version) -> 
            cprintfn ConsoleColor.Cyan "#"
            cprintfn ConsoleColor.Cyan "# Quantum fluent builder code generator %s" version
            cprintfn ConsoleColor.Cyan "#")
        
        (*
         * Reading dll data 
         *)
        >>=> (fun (_, _) -> 
            printfn ""
            printfn "reading input dll")
        >>= (fun (input, version) -> map (readDllData input.InputDllPath) (fun dllData -> (input, version, dllData)))
        >>=> (fun (_, _, dllData) -> 
            printfn " |  transitions found: \t%i" dllData.Transitions.Length
            printfn " |  functions found: \t%i" dllData.Functions.Length)

        (*
         * Reading map file
         *)
        >>=> (fun (_, _, _) -> 
            printfn ""
            printfn "reading map file")
        >>= fun (input, version, dllData) -> map (readFileContent input.MapPath) (fun mapContent -> (input, version, dllData, mapContent))
        >>=> (fun (_, _, _, mapContent) -> printfn "%s" (mapContent |> indent))

        (*
         * Parsing map file
         *)
        >>=> (fun _ -> 
            printfn ""
            printfn "parsing map file")
        >>= (fun (input, version, dllData, mapContent) -> (parseMap mapContent) >~ (fun parsedMap -> (input, version, dllData, parsedMap)))
        >>=> (fun (_, _, _, parsedMap) ->
            printfn " |  map nodes parsed: \t%i" parsedMap.Length)

        >>= (fun (input, version, dllData, parsedMap) -> map (Success (compose  parsedMap dllData.Transitions)) (fun composedNodes -> (input, version, dllData, composedNodes)))
        >>= (fun (input, version, dllData, composedNodes) -> map (Success (generate dllData.Functions composedNodes version)) (fun output -> (input, output)))
        >>= (fun (input, generatedCode: string) -> 
            use output = new System.IO.StreamWriter(input.OutputPath)
            do output.WriteLine generatedCode
            Success 0)
        >>=> (fun exitCode ->
            printfn ""
            cprintfn ConsoleColor.Green "Done")

    printfn ""

    let exitCode = 
        match programResult with
        | Success code -> code
        | Failure (message, code) -> 
            cprintfn ConsoleColor.Red "%s" message
            
            printfn ""
            printfn "exiting with code %i" code

            code

    //printfn ""
    //printfn "Press <Enter> to exit"
    //do System.Console.ReadLine() |> ignore

    exitCode