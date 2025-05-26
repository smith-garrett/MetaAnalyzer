open System
open Falco
open Falco.Routing
open Microsoft.AspNetCore.Builder


type DataPoint =
    { Id: Guid
      ExpName: string
      EffectSize: float
      StdError: float }

type DataPointCollection = { mutable Items: DataPoint array }

type RawData =
    { ExpName: string
      EffectSize: float
      StdError: float }

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


type MetaAnalyticEstimate = { Mean: float }

let doMetaAnalysis (datapoints: DataPointCollection) : MetaAnalyticEstimate =
    let effectsizes = datapoints.Items |> Array.map (fun x -> x.EffectSize)

    let weights =
        datapoints.Items
        |> Array.map (fun datapoint -> 1.0 / (datapoint.StdError ** 2.0))

    let weightedSum =
        Array.zip effectsizes weights |> Array.map (fun (m, w) -> m * w) |> Array.sum

    { Mean = weightedSum / (Array.sum weights) }

let resultHandler (collection: DataPointCollection) : HttpHandler =
    fun ctx ->
        task {
            let res = doMetaAnalysis collection
            return Response.ofPlainText $"The meta-analytic estimate is: {res.Mean}" ctx
        }

[<EntryPoint>]
let main args =
    let mutable datapoints =
        { Items =
            [| { Id = Guid.NewGuid()
                 ExpName = "test"
                 EffectSize = 0.0
                 StdError = 1.0 } |] }

    let endpoints =
        [ get "/" (getHandler datapoints)
          post "/create" (postHandler datapoints)
          get "/result" (resultHandler datapoints) ]

    let wapp = WebApplication.Create()

    wapp.UseRouting().UseFalco(endpoints).Run(Response.ofPlainText "Not found")

    0 // Exit code
