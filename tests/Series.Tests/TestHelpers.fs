module TestHelpers

open System
open Series

let series(x:int) = 
    let s = new Series<float>(x) 
    s

let length x (s:Series<float>) = s.Length = x

let NaN (s:Series<float>) = Double.IsNaN(s.Current)

let ZERO (s:Series<float>) = s.Current = 0.

let add cur (s:Series<float>) = 
    s.Add cur
    s

let chkCurrent cur (s:Series<float>) = 
    s.Current = cur

let toArray x (s:Series<float>) = 
    s.ToArray(x)


