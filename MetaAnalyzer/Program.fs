open System
open Falco
open Falco.Routing
open Microsoft.AspNetCore.Builder
open MetaAnalyzer


[<EntryPoint>]
let main args =
    let datapoints = DataPointCollection.Default

    let endpoints =
        [ get "/" (getHandler datapoints)
          post "/create" (postHandler datapoints)
          get "/result" (resultHandler datapoints) ]

    let wapp = WebApplication.Create()

    wapp.UseRouting().UseFalco(endpoints).Run(Response.ofPlainText "Not found")

    0 // Exit code
