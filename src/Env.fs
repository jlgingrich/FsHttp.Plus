namespace FsHttp

open System
open System.Collections

/// Simplified ways to interact with environment variables
type Env() =

    /// Gets a map of all environment variables
    static member getAll() =
        Environment.GetEnvironmentVariables()
        |> Seq.cast<DictionaryEntry>
        |> Seq.map (fun kv -> kv.Key :?> string, kv.Value :?> string)
        |> Map.ofSeq

    /// Sets an environment variable
    static member set key value =
        Environment.SetEnvironmentVariable(key, value)

    /// Gets an environment variable as a string, failing if it is missing
    static member get(key, ?msg) =
        match Environment.GetEnvironmentVariable key, msg with
        | null, Some m -> failwithf m key
        | null, None -> failwithf "Environment variable '%s' is not set" key
        | o, _ -> o

    /// Gets an environment variable and tries to parse it, failing if it is missing or the wrong type
    static member getWith(key, parser: string -> 't, ?msg) =
        match Environment.GetEnvironmentVariable key, msg with
        | null, Some m -> failwithf m key
        | null, None -> failwithf "Environment variable '%s' is not set" key
        | o, _ -> parser o

    /// Tries to get an environment variable, returning an option
    static member tryGet key =
        Environment.GetEnvironmentVariable key |> Option.ofObj

    /// Tries to get an environment variable and parse it, returning an option
    static member tryGetWith key parser =
        Environment.GetEnvironmentVariable key
        |> Option.ofObj
        |> Option.map (fun s ->
            match parser s with
            | true, v -> Some v
            | false, _ -> None)
        |> Option.flatten
