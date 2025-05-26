open System
open Falco
open Falco.Routing
open Microsoft.AspNetCore.Builder


type DataPoint =
    { Id: Guid
      ExpName: string
      EffectSize: float
      StdError: float }

type RawData =
    { ExpName: string
      EffectSize: float
      StdError: float }

type DataPointCollection =
    { mutable Datapoints: DataPoint list }

    static member Default = { Datapoints = List.empty<DataPoint> }

    member this.addItem(rawData: RawData) =
        let guid = Guid.NewGuid()

        let newItem =
            { Id = guid
              ExpName = rawData.ExpName
              EffectSize = rawData.EffectSize
              StdError = rawData.StdError }

        this.Datapoints <- List.append this.Datapoints [ newItem ]

let postHandler (datapoints: DataPointCollection) : HttpHandler =
    fun ctx ->
        task {
            let! newData = Request.getJson<RawData> ctx
            datapoints.addItem newData
            return Response.ofJson datapoints ctx
        }

let getHandler datapoints : HttpHandler =
    fun ctx -> task { return Response.ofPlainText (datapoints.ToString()) ctx }


type MetaAnalyticEstimate = { Mean: float }

let doMetaAnalysis (datapoints: DataPointCollection) : MetaAnalyticEstimate =
    let effectsizes = datapoints.Datapoints |> List.map (fun x -> x.EffectSize)

    let weights =
        datapoints.Datapoints
        |> List.map (fun datapoint -> 1.0 / datapoint.StdError ** 2.0)

    let weightedSum =
        List.zip effectsizes weights |> List.map (fun (m, w) -> m * w) |> List.sum

    { Mean = weightedSum / List.sum weights }

let resultHandler (datapoints: DataPointCollection) : HttpHandler =
    fun ctx ->
        task {
            let res = doMetaAnalysis datapoints
            return Response.ofPlainText $"The meta-analytic estimate is: {res.Mean}" ctx
        }

[<EntryPoint>]
let main args =
    // let mutable datapoints = List.empty<DataPoint>
    let datapoints = DataPointCollection.Default

    let endpoints =
        [ get "/" (getHandler datapoints)
          post "/create" (postHandler datapoints)
          get "/result" (resultHandler datapoints) ]

    let wapp = WebApplication.Create()

    wapp.UseRouting().UseFalco(endpoints).Run(Response.ofPlainText "Not found")

    0 // Exit code
