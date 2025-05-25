open System
open Falco
open Falco.Routing
open Microsoft.AspNetCore.Builder


type DataPoint =
    { Id: Guid
      ExpName: string
      EffectSize: float
      StdError: float option }

type DataPointCollection = { mutable Items: DataPoint array }

type RawData =
    { ExpName: string
      EffectSize: float
      StdError: float option }

let addItem (collection: DataPointCollection) (rawData: RawData) =
    let guid = Guid.NewGuid()

    let newItem =
        { Id = guid
          ExpName = rawData.ExpName
          EffectSize = rawData.EffectSize
          StdError = rawData.StdError }

    // collection.Items <- newItem :: collection.Items
    collection.Items <- Array.append collection.Items [| newItem |]

let postHandler (collection: DataPointCollection) : HttpHandler =
    fun ctx ->
        task {
            let! newData = Request.getJson<RawData> ctx
            addItem collection newData
            return Response.ofJson collection ctx
        }

let getHandler (collection: DataPointCollection) : HttpHandler =
    fun ctx -> task { return Response.ofPlainText (collection.ToString()) ctx }


[<EntryPoint>]
let main args =
    let mutable datapoints =
        { Items =
            [| { Id = Guid.NewGuid()
                 ExpName = "test"
                 EffectSize = 0.0
                 StdError = Some 1.0 } |] }

    let endpoints =
        [ get "/" (getHandler datapoints); post "/create" (postHandler datapoints) ]

    let wapp = WebApplication.Create()

    wapp.UseRouting().UseFalco(endpoints).Run(Response.ofPlainText "Not found")

    0 // Exit code
