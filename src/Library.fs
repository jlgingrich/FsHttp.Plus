namespace FsHttp

open FsHttp
open Thoth.Json.Net
open FSharp.Data

module Option =
    let getOrFail err opt =
        match opt with
        | Some v -> v
        | None -> failwith err


type QueryParams = List<string * string>

module Query =
    let item (k: string) f v : QueryParams = [ k, sprintf f v ]

    let values (k: string) f vs : QueryParams =
        List.collect (fun s -> [ k, sprintf f s ]) vs

    let option (k: string) f vo : QueryParams =
        match vo with
        | Some v -> [ k, sprintf f v ]
        | None -> []

    let join: list<QueryParams> -> QueryParams = List.concat

[<AutoOpen>]
module Extensions =
    type IRequestContext<'t> with
        [<CustomOperation("query")>]
        member _.Query(context: IRequestContext<HeaderContext>, qs: QueryParams list) =
            context.Self.Query(Query.join qs)

        [<CustomOperation>]
        member _.encode(context: IRequestContext<BodyContext>, encoder: Encoder<'a>, value: 'a) =
            context.Self.Json(Encode.toString 0 (encoder value))

module Response =
    // Decodes the response with a Thoth.Json decoder
    let decode decoder response =
        Decode.fromString decoder (Response.toText response)

    let toHtml = Response.toStream >> HtmlDocument.Load >> _.Html()


module Result =
    // Gets the value associated with the option. Throws when the argument is Error
    let assertOk r =
        match r with
        | Ok r' -> r'
        | Error e -> failwithf "Assertion failed: %A" e
