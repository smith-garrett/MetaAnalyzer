# WIP: A web API for automatically generating meta-analyses from uploaded data

After carrying out a series of experiments, it is common in research to carry out a meta-analysis of all of the results. A meta-analysis is a quantitive summary of a series of effect size estimates, effectively a weighted average.

This project is meant to a) provide a simple web API for uploading experiment results and providing a simple meta-analysis and b) give me an opportunity to practice web programming in F#.

I'm using [Falco](https://www.falcoframework.com/) as the web framework and calculating the meta-analysis manually using [the inverse variance method](https://en.wikipedia.org/wiki/Inverse-variance_weighting). The main assumption is that the effect sizes that are uploaded to the API are normally distributed.

The format expected by the API is an HTTP POST-request to "/create" with the following JSON schema:

```json
{
  "ExpName": "my-favorite-experiment"
  "EffectSize": 10.0
  "StdError": 0.5
}
```

The currently uploaded data points can be viewed via a GET request to "/", and the result is at "/result"

Currently, this only runs locally, but maybe I'll scale it up and host it somewhere eventually.

