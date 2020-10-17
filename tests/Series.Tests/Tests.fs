module SeriesTests
open System.Runtime.CompilerServices
open System
open System.Diagnostics
open FsUnit
open Xunit
open Series
open TestHelpers

[<Fact>]
let uninitialied_series_length_should_be_0 ()=
    let s = series(4)
    s.Length |> should equal 0

[<Fact>]
let series_current_can_be_set() =
    let s = series(5)
    s |> add 4.5 |> (chkCurrent 4.5) |> should equal true
    length 1 s |> should equal true

[<Fact>]
let series_item_can_be_got() =
    let s = series(6)
    s
    |> add 4.5
    |> (fun x -> x.[0])
    |> should equal 4.5

[<Fact>]
let series_toArray1() =
    let s = series(5)
    s
    |> add 4.5
    |> add 5.5
    |> toArray 2
    |> should equal [| 4.5; 5.5|] 

[<Fact>]
let series_toArray_when_x_is_longer() =
    let s = series(3)
    s
    |> add 4.5
    |> add 5.5
    |> toArray 4
    |> should equal [| 4.5; 5.5|] 

[<Fact>]
let series_toArray_when_full() =
    let s = series(6)
    s
    |> add 4.5
    |> add 5.5
    |> add 6.5
    |> toArray 3
    |> should equal [| 4.5; 5.5; 6.5|] 

[<Fact>]
let series_toArray_when_overfull() =
    let s = series(4)
    s
    |> add 4.5
    |> add 5.5
    |> add 6.5
    |> add 7.5
    |> add 8.5
    |> toArray 3
    |> should equal [|  6.5; 7.5; 8.5|] 

[<Fact>]
let series_toArray_when_overfull_andl_less_than_tail() =
    let s = series(4)
    s
    |> add 4.5
    |> add 5.5
    |> add 6.5
    |> add 7.5
    |> add 8.5
    |> toArray 1
    |> should equal [|  8.5; |] 

[<Fact>]
let series2() =
    let s = series(10)
    s
    |> add 4.5
    |> add 5.5
    |> add 6.5
    |> add 7.5
    |> add 8.5
    |> toArray 5
    |> should equal [|  4.5; 5.5; 6.5; 7.5; 8.5; |] 
     
[<Fact>]
let series3() =
    let s = series(10)
    s
    |> add 5.5
    |> add 4.5
    |> add 7.5
    |> add 8.5
    |> toArray 5
    |> should equal [|   5.5; 4.5; 7.5; 8.5; |] 
     
[<Fact>]           
let testFill() =
    let s = series(4)
    s
    |> add 4.5
    |> add 5.5
    |> add 6.5
    |> add 7.5
    |> add 8.5 |> ignore
    let d = Array.zeroCreate 3
    s.FillArray d |> ignore
    d |> should equal [| 6.5; 7.5; 8.5 |]

[<Fact>]
let series_load() =
    let s = series(2)
    s
    |> (fun x -> s.Load([|2.5;1.2|]); s)
    |> toArray 2
    |> should equal [|  2.5; 1.2 |] 

[<Fact>]
let series_empty() =
    let s = series(2)
    s.ToArray()
    |> should equal [||] 
    s
    |> add 5.
    |> length 1 
    |> should equal true

[<Fact>]
let series_array_init() =
    let s = Series([|2.0; 3.0|]) :> ISeries<float>
    s.Length |> should equal 2
    s.Capacity |> should equal 2
    s.ToArray() |> should equal [|2.0; 3.0|]
    s.Lookback 0 |> should equal 3.0
    s.Forward 1 |> should equal 3.0

[<Fact>]
let series_enumerable() =
    let s = Series([|2.0; 3.0; 4.5; 5.0|]) :> ISeries<float>
    let f = Seq.filter (fun x -> x > 3.0) s
    f |> Seq.toArray |> should equal [|4.5; 5.0|]

[<Fact>]
let series_clear() =
    let s = Series([|2.0; 3.0|]) :> ISeries<float>
    s.Clear()
    s.Length |> should equal 0

[<Struct>]//
type TestStruct = {
    One: int
    Two: int
}

let extractOne (x:byref<TestStruct>) = 
    x.One

[<Fact>]
let get_by_ref() = 
    let s = Series([|{One = 1; Two = 2}; {One = 2; Two = 3} |]) :> ISeries<TestStruct>
    let x = &s.ItemRef(0)
    x.One |> should equal 2
   // let mapped = Series.mapRef extractOne s
    //mapped.[0] |> should equal 2

