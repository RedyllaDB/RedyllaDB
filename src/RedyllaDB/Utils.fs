module internal RedyllaDB.Utils

open System.Linq

module Seq =
    
    let inline tryHeadV (source: seq<_>) =
        source.FirstOrDefault() |> ValueOption.ofObj