module Utils

open System

let split (separators: String array) (input:String) =
    input.Split(separators, StringSplitOptions.RemoveEmptyEntries)

let splitLines input =
    input |> split [|"\r\n";"\n"|]

let replace (findText: string) (replaceText: string) (input: string) = 
    input.Replace(findText, replaceText)

let makeCamelcase (input:string) =
    let lowercase (word:String) = word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower()
    input 
    |> split [|"_"; " "|]
    |> Seq.map lowercase
    |> String.concat ""    
