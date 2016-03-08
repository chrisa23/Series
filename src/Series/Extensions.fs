namespace Series

open System.Runtime.CompilerServices

[<Extension>]
type SeriesExtensions () =
    [<Extension>]
    static member inline SMA(xs: Series<float>, period: int) = 
        let p = float period
        let sum = ref 0.
        let count = ref 0.
        let sma = Series<float>(xs.Capacity)
        xs.Added.Add(fun x -> 
            sum :=  !sum + x
            count := !count + 1.
            if !count > p then
                count := p
                sum := !sum - xs.[period]
            sma.Add(!sum / !count))
        sma 

