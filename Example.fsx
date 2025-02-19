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
