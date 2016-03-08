module ExtensionsTests

open System
open System.Diagnostics
open FsUnit
open NUnit.Framework
open Series
open TestHelpers

[<Test>]
let sma_test1 ()=
    let s = series(128)
    let sma = s.SMA(5)
    s
    |> add 2.
    |> add 3.
    |> add 4.
    |> add 5. |> ignore
    sma.[0] |> should equal 3.5
    
    s |> add 1.|> ignore
    sma.[0] |> should equal 3.
    
    s |> add 2. |> ignore
    sma.[0] |> should equal 3.
    
    s |> add 6. |> ignore
    sma.[0] |> should equal ((4. + 5. + 1. + 2. + 6.) / 5.)


