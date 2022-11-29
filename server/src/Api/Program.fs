open Suave
open Suave.Logging

open MyCheckers.WebApp

type CmdArgs = { IP: string; Port: int }

[<EntryPoint>]
let main argv =
  let args =
    let (|Int|_|) (str: string) =
       match System.Int32.TryParse(str) with
       | (true, int) -> Some(int)
       | _ -> None

    let defaultArgs = { 
      IP = "127.0.0.1"; 
      Port = 8080
    }

    let rec parseArgs b args =
      match args with
      | [] -> b
      | "--ip" :: ip :: xs -> parseArgs { b with IP = ip } xs
      | "--port" :: Int p :: xs -> parseArgs { b with Port = p } xs
      | invalidArgs ->
          printfn "error: invalid arguments %A" invalidArgs
          printfn "Usage:"
          printfn "    --ip ADDRESS      ip address (Default: %O)" defaultArgs.IP
          printfn "    --port PORT       port (Default: %i)" defaultArgs.Port
          exit 1

    argv |> List.ofArray |> parseArgs defaultArgs

  startWebServer { defaultConfig with 
                    logger = Targets.create Verbose [||]
                    bindings = [ HttpBinding.createSimple HTTP args.IP args.Port ]
                    homeFolder = Some (System.IO.Path.GetFullPath "wwwroot") } app
  0