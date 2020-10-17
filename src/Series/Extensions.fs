namespace Series

open System.Runtime.CompilerServices

[<Extension>]
type SeriesExtensions () =
    [<Extension>]
    static member inline SMA(xs: ISeries<float>, period: int) : ISeries<float> = 
        let p = float period
        let mutable sum = 0.
        let mutable count = 0.
        let sma = Series.create<float> xs.Capacity
        xs.Added.Add(fun x -> 
            sum <-  sum + x
            count <- count + 1.
            if count > p then
                count <- p
                sum <- sum - xs.[period]
            sma.Add(sum / count))
        sma 

