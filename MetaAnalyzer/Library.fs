module MetaAnalyzer


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
    { mutable DataPoints: DataPoint list }

    static member Default = { DataPoints = List.empty<DataPoint> }

    member this.addItem(rawData: RawData) =
        let guid = Guid.NewGuid()

        let newItem =
            { Id = guid
              ExpName = rawData.ExpName
              EffectSize = rawData.EffectSize
              StdError = rawData.StdError }

        this.DataPoints <- List.append this.DataPoints [ newItem ]

type MetaAnalyticEstimate = { Mean: float }

let postHandler (datapoints: DataPointCollection) : HttpHandler =
    fun ctx ->
        task {
            let! newData = Request.getJson<RawData> ctx
            datapoints.addItem newData
            return Response.ofJson datapoints ctx
        }

let getHandler datapoints : HttpHandler =
    fun ctx -> task { return Response.ofPlainText (datapoints.ToString()) ctx }

let doMetaAnalysis (datapoints: DataPointCollection) : MetaAnalyticEstimate =
    let effectsizes = datapoints.DataPoints |> List.map (fun x -> x.EffectSize)

    let weights =
        datapoints.DataPoints
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
