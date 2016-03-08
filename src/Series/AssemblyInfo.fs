namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Series")>]
[<assembly: AssemblyProductAttribute("Series")>]
[<assembly: AssemblyDescriptionAttribute("Fast, simple time series based on an allocationless circular buffer")>]
[<assembly: AssemblyVersionAttribute("0.0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.0.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.1"
