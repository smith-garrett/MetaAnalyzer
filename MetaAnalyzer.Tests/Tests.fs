﻿module Tests

open System
open Xunit

open MetaAnalyzer


[<Fact>]
let ``The meta-analytic result of a single effect size should equal the effect size`` () =
    let datapoints = DataPointCollection.Default

    datapoints.addItem
        { ExpName = "test"
          EffectSize = 10.0
          StdError = 1.0 }

    let est = doMetaAnalysis datapoints

    let value = datapoints.DataPoints.ToArray() |> fun x -> x[0].EffectSize

    Assert.Equal(value, est.Mean)

[<Fact>]
let ``The meta-analytic estimate of 10 and 12 with equal std. errors should be 11`` () =
    let datapoints = DataPointCollection.Default

    datapoints.addItem
        { ExpName = "test1"
          EffectSize = 10.0
          StdError = 1.0 }

    datapoints.addItem
        { ExpName = "test2"
          EffectSize = 12.0
          StdError = 1.0 }

    let est = doMetaAnalysis datapoints
    Assert.Equal(11.0, est.Mean)
