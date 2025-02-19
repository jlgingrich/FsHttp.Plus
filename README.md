# FsHttp.Plus

A set of extensions for `FsHttp` that I've found useful.

Currently provides:

- Integrations with `Thoth.Json` allowing the use of Encoders directly in the request body and Decoders directly on responses.
- Utility methods for query strings parameters to ease creating functions that basically wrap an API call.

## Example

`Example.fsx`

```fsx
#r "nuget: Thoth.Json.Net"
#r "nuget: FsHttp"
#r "src/bin/Debug/net9.0/FsHttp.Plus.dll"

open FsHttp
open Thoth.Json.Net

type Post = {
    Id: int option
    Author: int
    Title: string
    Body: string
}

module Post =
    let encoder: Encoder<Post> =
        fun post ->
            let r =
                match post.Id with
                | None -> []
                | Some id -> [ "id", Encode.int id ]

            Encode.object (
                List.append r [
                    "userId", Encode.int post.Author
                    "title", Encode.string post.Title
                    "body", Encode.string post.Body
                ]
            )

    let decoder: Decoder<Post> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" (Decode.int |> Decode.map Some)
            Author = get.Required.Field "userId" Decode.int
            Title = get.Required.Field "title" Decode.string
            Body = get.Required.Field "body" Decode.string
        })

type PlaceholderAPI(url) =
    let jsonplaceholder = http { config_useBaseUrl url }

    member _.createPost(post) =
        jsonplaceholder {
            POST "/posts"
            body
            encode Post.encoder post
        }
        |> Request.send
        |> Response.decode Post.decoder
        |> Result.assertOk

    member _.getPost(?id, ?userId) =
        jsonplaceholder {
            GET "/posts"
            query [ Query.option "id" "%d" id; Query.option "userId" "%d" userId ]
        }
        |> Request.send
        |> Response.decode (Decode.list Post.decoder)
        |> Result.assertOk

// Script
let api = PlaceholderAPI "https://jsonplaceholder.typicode.com/"

let origPost = api.getPost (id = 1) |> List.head

let newPost = {
    origPost with
        Title = "Renamed post!"
        Id = None
}

let newPostWithId = api.createPost newPost
printfn "%A" newPostWithId

```

```text
{ Id = Some 101
  Author = 1
  Title = "Renamed post!"
  Body =
   "quia et suscipit
suscipit recusandae consequuntur expedita et cum
reprehenderit molestiae ut ut quas totam
nostrum rerum est autem sunt rem eveniet architecto" }
```
