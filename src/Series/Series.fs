namespace Series

open System
open System.Collections
open System.Collections.Generic

///allocationless circular buffer with Lookback from end
///Indexing is based from the end of the series.
///Enumeration is from the front... 


type ISeries<'a> = 
    inherit IEnumerable<'a>
    //TODO: need to be able to turn off Added for Ref based
    [<CLIEvent>]
    abstract member Added : IEvent<'a> 
    abstract member Capacity: int with get
    abstract member Length : int with get
    abstract member Lookback :int -> 'a
    abstract member LookbackRef :int -> byref<'a>
    abstract member Forward : int -> 'a
    abstract member ForwardRef : int -> byref<'a>
    abstract member Add : 'a -> unit
    abstract member Current : 'a with get
    abstract member Item : int -> 'a with get
    abstract member ItemRef : int -> byref<'a> with get
    abstract member Load : 'a[] -> unit
    abstract member FillArray : 'a[] -> bool
    abstract member FillArray : 'a[] * int[] -> bool
    abstract member ToArray: int -> 'a[]
    abstract member ToArray: unit -> 'a[]
    abstract member Clear: unit -> unit



[<Sealed>]
type SingletonSeries<'a> ( item) = 
    let added = Event<'a>()
    let mutable value = item 
    
    new() = SingletonSeries( Unchecked.defaultof<'a>)
    interface ISeries<'a> with
        [<CLIEvent>]
        member __.Added = added.Publish
        member __.Capacity with get() = 1
        member __.Length with get() = 1
        member __.Lookback x = value
        member __.LookbackRef x = &value
        member __.Forward x = value
        member __.ForwardRef x = &value
        member __.Add item = 
            value <- item
            added.Trigger item
        member __.Current with get() = value
        member __.Item 
            with get i = 
                if i <> 0 then raise (ArgumentOutOfRangeException())
                value
        member __.ItemRef
            with get i = 
                if i <> 0 then raise (ArgumentOutOfRangeException())
                &value
        member __.FillArray (result:'a[]) = 
            result.[0] <- value
            true
        member __.FillArray (result:'a[], indexes:int[])= 
            result.[0] <- value     
            true   
        member __.ToArray i = [| value |]
           
        member __.ToArray() = [| value |]

        member __.Load d = value <- d.[d.Length-1]
        member __.Clear() = value <- Unchecked.defaultof<'a>
    interface IEnumerable<'a> with 
        member this.GetEnumerator() = 
          let mutable index = -1
          { new IEnumerator<'a> with
                member e.Current with get() = value
              interface System.IDisposable with
                member e.Dispose () = ()
              interface IEnumerator with
                member e.Reset () = index <- -1
                member e.Current with get() = box value
                member e.MoveNext () = 
                  index <- index + 1
                  index = 0 }
        member this.GetEnumerator () =
          (this:>IEnumerable<'a>).GetEnumerator() :> IEnumerator   


[<Sealed>]
type Series2D(data:float[,], row, head, tail) =
    let added = Event<float>()
    let mutable head = head
    let mutable tail = tail
    let mutable count = (head - tail + 1)
    let capacity = data.GetLength 1 
    let incrm x y = (x + y) % capacity
    let decr x y = (x - y + capacity) % capacity
    let forward i = 
        if count = 0 || i >= count then raise (IndexOutOfRangeException())
        data.[row,incrm tail i]
    let lookback i = 
        if count = 0 || i >= count then raise (IndexOutOfRangeException())
        data.[row,decr head i]
    let forwardRef i  = 
        if count = 0 || i >= count then raise (IndexOutOfRangeException())
        &data.[row, incrm tail i]
    let lookbackRef i  = 
        if count = 0 || i >= count then raise (IndexOutOfRangeException())
        &data.[row, decr head i]
    let advance() =            
        if capacity > 1 then 
            count <- count + 1
            if count > capacity then
                count <- capacity
                tail <- incrm tail 1
            head <- incrm head 1
            data.[row,head] <- nan
        elif capacity = 1 && head = -1 then 
            count <- 1
            head <- 0
    interface ISeries<float> with
        [<CLIEvent>]
        member __.Added = added.Publish
        member __.Capacity with get() = capacity
        member __.Length with get() = count
        member __.Lookback x = lookback x
        member __.LookbackRef x = &lookbackRef x
        member __.Forward x = forward x
        member __.ForwardRef x : byref<float>  = &forwardRef x
        member __.Add item = 
            advance()
            data.[row,head] <- item
            added.Trigger item
        member __.Current with get() = data.[row,head]
        member __.Item with get i = lookback i
        member __.ItemRef with get i : byref<float>  = &lookbackRef i
        member __.Load (toLoad:float[]) =
            for d in toLoad do 
                advance()
                data.[row,head] <- d
        member __.Clear() = 
            head <- -1
            tail <- 0
            count <- 0

        //redo fill array , return end index
        member __.FillArray (result:float[]) = 
            let i = result.Length
            //Array.Clear(result, 0, result.Length)
            match i with
            | 0 -> false
            | a when a <= count && tail = 0 -> 
                Array.Copy(data, count-a ,result, 0 , a)
                true
            | a when a <= count &&  a <= tail -> 
                Array.Copy(data, head + 1 - a , result, 0, a);
                true
            | a when a <= count && tail > 0 -> 
                let firstLength = tail
                let secondLength = a - firstLength
                let offset = count - secondLength
                Array.Copy(data, head + 1 - firstLength , result, a - firstLength, firstLength)
                Array.Copy(data, offset , result, 0, secondLength)
                true
            | a when a > count -> false
            | _ -> false
        member __.FillArray (result:float[], indexes:int[])= 
            //assert they match lengths...
            let last = indexes.[indexes.Length-1]
            if last > count then false else
            Array.iteri (fun i i2 -> result.[i] <- lookback i2) indexes
            true   
        member this.ToArray i =
            match i with
            | 0 -> [||]
            | a when a <= count && tail = 0 -> 
                let result = Array.zeroCreate<float> a
                Array.Copy(data, count-a ,result, 0 , a)
                result
            | a when a <= count &&  a <= tail -> 
                let result = Array.zeroCreate<float> a
                Array.Copy(data, head + 1 - a , result, 0, a);
                result
            | a when a <= count && tail > 0 -> 
                let result = Array.zeroCreate<float> a
                let firstLength = tail
                let secondLength = a - firstLength
                let offset = count - secondLength
                Array.Copy(data, head + 1 - firstLength , result, a - firstLength, firstLength)
                Array.Copy(data, offset , result, 0, secondLength)
                result
            | a when a > count -> (this :> ISeries<float>).ToArray(count)
            | _ -> [||]
        member this.ToArray() = (this :> ISeries<float>).ToArray(count)

   
    interface IEnumerable<float> with 
        member __.GetEnumerator() = 
            let mutable index = -1
            let mutable current = nan
            { new IEnumerator<float> with
                    member e.Current with get() = current
                interface System.IDisposable with
                    member e.Dispose () = ()
                interface IEnumerator with
                    member e.Reset () = index <- -1; current <- nan
                    member e.Current with get() = box current
                    member e.MoveNext () = 
                        index <- index + 1
                        if (index >= count) then false
                        else current <- forward index; true  }
        member this.GetEnumerator () =
            (this:>IEnumerable<float>).GetEnumerator() :> IEnumerator 


[<Sealed>]
type Series<'a> (items:'a[], head, tail) = 
    let added = Event<'a>()
    let data = items
    let mutable head = head
    let mutable tail = tail
    let mutable count = (head - tail + 1)
    let capacity = items.Length 
    let incrm x y = (x + y) % capacity
    let decr x y = (x - y + capacity) % capacity
    let forward i = 
        if count = 0 || i >= count then raise (IndexOutOfRangeException())
        data.[incrm tail i]
    let lookback i = 
        if count = 0 || i >= count then raise (IndexOutOfRangeException())
        data.[decr head i]
    let forwardRef i  = 
        if count = 0 || i >= count then raise (IndexOutOfRangeException())
        &data.[incrm tail i]
    let lookbackRef i  = 
        if count = 0 || i >= count then raise (IndexOutOfRangeException())
        &data.[decr head i]
    let advance() =            
        if capacity > 1 then 
            count <- count + 1
            if count > capacity then
                count <- capacity
                tail <- incrm tail 1
            head <- incrm head 1
            data.[head] <- Unchecked.defaultof<'a>
        elif capacity = 1 && head = -1 then 
            count <- 1
            head <- 0

    new(capacity) = Series<'a>(Array.zeroCreate capacity, -1, 0) 
    new() = Series<'a>(Array.zeroCreate 512, -1, 0) //??
    new(items:'a[]) = Series<'a>(items, items.Length - 1, 0)
    interface ISeries<'a> with
        [<CLIEvent>]
        member __.Added = added.Publish
        member __.Capacity with get() = capacity
        member __.Length with get() = count
        member __.Lookback x = lookback x
        member __.LookbackRef x = &lookbackRef x
        member __.Forward x = forward x
        member __.ForwardRef x : byref<'a>  = &forwardRef x
        member __.Add item = 
            advance()
            data.[head] <- item
            added.Trigger item
        member __.Current with get() = data.[head]
        member __.Item with get i = lookback i
        member __.ItemRef with get i : byref<'a>  = &lookbackRef i
        member __.Load (toLoad:'a[]) =
            for d in toLoad do 
                advance()
                data.[head] <- d
        member this.Clear() = 
            head <- -1
            tail <- 0
            count <- 0

        //redo fill array , return end index
        member __.FillArray (result:'a[]) = 
            let i = result.Length
            //Array.Clear(result, 0, result.Length)
            match i with
            | 0 -> false
            | a when a <= count && tail = 0 -> 
                Array.Copy(data, count-a ,result, 0 , a)
                true
            | a when a <= count &&  a <= tail -> 
                Array.Copy(data, head + 1 - a , result, 0, a);
                true
            | a when a <= count && tail > 0 -> 
                let firstLength = tail
                let secondLength = a - firstLength
                let offset = count - secondLength
                Array.Copy(data, head + 1 - firstLength , result, a - firstLength, firstLength)
                Array.Copy(data, offset , result, 0, secondLength)
                true
            | a when a > count -> false
            | _ -> false
        member __.FillArray (result:'a[], indexes:int[])= 
            //assert they match lengths...
            let last = indexes.[indexes.Length-1]
            if last > count then false else
            Array.iteri (fun i i2 -> result.[i] <- lookback i2) indexes
            true   
        member this.ToArray i =
            match i with
            | 0 -> [||]
            | a when a <= count && tail = 0 -> 
                let result = Array.zeroCreate<'a> a
                Array.Copy(data, count-a ,result, 0 , a)
                result
            | a when a <= count &&  a <= tail -> 
                let result = Array.zeroCreate<'a> a
                Array.Copy(data, head + 1 - a , result, 0, a);
                result
            | a when a <= count && tail > 0 -> 
                let result = Array.zeroCreate<'a> a
                let firstLength = tail
                let secondLength = a - firstLength
                let offset = count - secondLength
                Array.Copy(data, head + 1 - firstLength , result, a - firstLength, firstLength)
                Array.Copy(data, offset , result, 0, secondLength)
                result
            | a when a > count -> (this :> ISeries<'a>).ToArray(count)
            | _ -> [||]
        member this.ToArray() = (this :> ISeries<'a>).ToArray(count)

    interface IEnumerable<'a> with 
        member this.GetEnumerator() = 
          let mutable index = -1
          let mutable current = Unchecked.defaultof<'a>
          { new IEnumerator<'a> with
                member e.Current with get() = current
              interface System.IDisposable with
                member e.Dispose () = ()
              interface IEnumerator with
                member e.Reset ()  = index <- -1; current <- Unchecked.defaultof<'a> 
                member e.Current with get() = box current
                member e.MoveNext () = 
                  index <- index + 1
                  if (index >= count) then false
                  else current <- forward index; true  }
        member this.GetEnumerator () =
          (this:>IEnumerable<'a>).GetEnumerator() :> IEnumerator   


[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module Series = 

    let create<'a> capacity = 
        if capacity = 1 then SingletonSeries<'a>() :> ISeries<'a> else Series<'a>(capacity) :> ISeries<'a>
    
    let createFrom<'a> (data:'a[]) = Series<'a>(data) :> ISeries<'a>
    

    let inline nth (s:ISeries<'a>) n = s.[n]

    let inline map f (s:ISeries<'a>)  = 
        let l = s.Length
        let s' = Series<'b>(s.Capacity) :> ISeries<'b>
        for i = 0 to l-1 do
            s'.Add (f (s.Forward(i)))
        s'

    let inline mapi f (s:ISeries<'a>)  = 
        let l = s.Length
        let s' = Series<'b>(s.Capacity) :> ISeries<'b>
        for i = 0 to l-1 do
            s'.Add (f i (s.Forward(i)))
        s'

    //let inline mapRef (f: byref<'a> -> 'b) (s:Series<'a>)  = 
    //    let l = s.Length
    //    let s' = Series<'b>(s.Capacity)
    //    for i = 0 to l-1 do
    //        s'.Add (f (&s.ForwardRef(i)))
    //    s'

    
    let inline fold (f:'a -> 'a -> 'a) a (s:ISeries<'a>) = 
        let l = s.Length
        let mutable a = a
        for i=0 to l - 1 do
            a <- f a (s.Forward(i))
        a

    let inline foldBack f a (s:ISeries<'a>) = 
        let mutable a = a
        for i=0 to s.Length-1 do
            a <- f a s.[i]
        a
    
    let inline average (s:ISeries<'a>) : 'a 
            when 'a : (static member Zero: 'a)
            and  'a : (static member DivideByInt : 'a*int -> 'a)
            and 'a : (static member (+) : 'a * 'a -> 'a) =
        let l = s.Length
        let sum = fold (+) LanguagePrimitives.GenericZero s
        LanguagePrimitives.DivideByInt<'a> sum l

    let inline iter f (s:ISeries<'a>)  = 
        let l = s.Length
        for i = 0 to l-1 do
            f s.[l - 1 - i]

    let inline iteri f (s:ISeries<'a>)  = 
        let l = s.Length
        for i = 0 to l-1 do
            f i s.[l - 1 - i]
        
        
    

    //let averageBy f (s:Series<'a>) =

    //let contains x (s:Series<'a>) =

    //let reduce f (s:Series<'a>) =

    //let filter f (s:Series<'a>) =

