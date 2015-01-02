#r "packages/FSharp.Data.2.0.14/lib/net40/FSharp.Data.dll"
open FSharp.Data

let url = "http://api.openweathermap.org/data/2.5/weather?q=Stockholm&units=metric"
let weather = JsonValue.Load(url)
match weather with
| JsonValue.Record(fields) -> 
    match Array.find (fun (k,v) -> k = "main") fields with
    | _, JsonValue.Record(fields) -> 
      match Array.find (fun (k,v) -> k = "temp") fields with
      | _, JsonValue.Number n -> printfn "%f" n
      | _ -> failwith "Wrong format"
    | _ -> failwith "Wrong format"
| _ -> failwith "Wrong format"
