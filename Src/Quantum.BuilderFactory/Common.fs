namespace Quantum.BuilderFactory

module Common =
    type Result<'TResult, 'TFailure> =
    | Success of 'TResult
    | Failure of 'TFailure

    let bind switchFunction twoTrackInput = 
        match twoTrackInput with
        | Success s -> 
            try 
                switchFunction s
            with
                | ex -> Failure (ex.Message, -200)
        | Failure f -> Failure f

    let map twoTrackInput oneTrackFunction = 
        match twoTrackInput with
        | Success s -> Success (oneTrackFunction s)
        | Failure f -> Failure f

    let tee f x = 
        f x |> ignore
        Success x

    let (>>=) twoTrackInput switchFunction = 
        bind switchFunction twoTrackInput 

    let (>>=>) twoTrackInput teeFunction = 
        bind (tee teeFunction) twoTrackInput 

    let (>~) twoTrackInput oneTrackFunction = 
        map twoTrackInput oneTrackFunction