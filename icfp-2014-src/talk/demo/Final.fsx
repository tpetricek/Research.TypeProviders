#r "packages/FSharp.Data.2.0.14/lib/net40/FSharp.Data.dll"
open FSharp.Data

type Weather = JsonProvider<"http://api.openweathermap.org/data/2.5/weather?q=London">

let url = "http://api.openweathermap.org/data/2.5/weather?q=Stockholm&units=metric"
let weather = Weather.Load(url)

weather.Main.Temp