module TestHelpers

open System
open Series

let series(x:int) = Series.create<float> x

let length x (s:ISeries<float>) = s.Length = x

let NaN (s:ISeries<float>) = Double.IsNaN(s.Current)

let ZERO (s:ISeries<float>) = s.Current = 0.

let add cur (s:ISeries<float>) = 
    s.Add cur
    s

let chkCurrent cur (s:ISeries<float>) = s.Current = cur

let toArray x (s:ISeries<float>) = s.ToArray(x)


