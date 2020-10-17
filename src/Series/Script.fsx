// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#load "Series.fs"
open Series

let s = Series<float>(20) 
s.Add 2.0
s.Add 3.0
s.Add 4.0
let v = s.[0]//4.0