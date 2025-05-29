module MetaAnalyzer


open System
open System.Collections.Concurrent
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
    { DataPoints: ConcurrentBag<DataPoint> }

    static member Default = { DataPoints = ConcurrentBag<DataPoint>() }

    member this.addItem(rawData: RawData) =
        let guid = Guid.NewGuid()

        let newItem =
            { Id = guid
              ExpName = rawData.ExpName
              EffectSize = rawData.EffectSize
              StdError = rawData.StdError }

        this.DataPoints.Add newItem

type MetaAnalyticEstimate = { Mean: float }

let postHandler (datapoints: DataPointCollection) : HttpHandler =
    fun ctx ->
        task {
            let! newData = Request.getJson<RawData> ctx
            datapoints.addItem newData
            return Response.ofJson datapoints ctx
        }

let getHandler datapoints : HttpHandler =
    fun ctx -> task { return Response.ofJson datapoints ctx }

let doMetaAnalysis (datapoints: DataPointCollection) : MetaAnalyticEstimate =
    let effectsizes = datapoints.DataPoints |> Seq.map (fun x -> x.EffectSize)

    let weights =
        datapoints.DataPoints
        |> Seq.map (fun datapoint -> 1.0 / datapoint.StdError ** 2.0)

    let weightedSum =
        Seq.zip effectsizes weights |> Seq.map (fun (m, w) -> m * w) |> Seq.sum

    { Mean = weightedSum / Seq.sum weights }

let resultHandler (datapoints: DataPointCollection) : HttpHandler =
    fun ctx ->
        task {
            let res = doMetaAnalysis datapoints
            return Response.ofPlainText $"The meta-analytic estimate is: {res.Mean}" ctx
        }
