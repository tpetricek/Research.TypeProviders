#r "packages/FSharp.Data.2.0.14/lib/net40/FSharp.Data.dll"
open FSharp.Data

let url = "http://api.openweathermap.org/data/2.5/weather?q=Stockholm&units=metric"
type Weather = JsonProvider<"http://api.openweathermap.org/data/2.5/weather?q=Stockholm&units=metric">

let w = Weather.Load(url)
w.Main.
