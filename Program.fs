open System
open BoardSetup
open GameLoop

[<Literal>]
let AppName = "FineSweeper"

let AppVersion =
    let v = Reflection.Assembly.GetExecutingAssembly().GetName().Version
    $"{v.Major}.{v.Minor}.{v.Build}"

[<Literal>]
let AppProjectUrl = "https://github.com/nikvoronin/FineSweeper"

[<EntryPoint>]
let main argv =
    Console.OutputEncoding <- Text.Encoding.UTF8

    let printHelp () =
        printfn ""
        printfn $"{AppName} v{AppVersion} |> F# Minesweeper"
        printfn AppProjectUrl
        printfn ""
        printfn  "Usage:"
        printfn  "  finesweeper [width] [height] [mines] [seed]"
        printfn ""
        printfn  "Arguments:"
        printfn $"  width  - board width [{Limits.MinBoardSize}..{Limits.MaxBoardSize}] default {Limits.DefaultWidth}"
        printfn $"  height - board height [{Limits.MinBoardSize}..{Limits.MaxBoardSize}] default {Limits.DefaultHeight}"
        printfn $"  mines  - number of mines [{Limits.MinMineCount}..{Limits.MaxMineCount}] default {Limits.DefaultMineCount}"
        printfn  "  seed   - optional random seed (integer, optional)"
        printfn ""
        printfn  "Example runs:"
        printfn  "  finesweeper"
        printfn  "  finesweeper 10 10 20"
        printfn  "  finesweeper 15 15 30 54321"
        printfn  "  finesweeper --help -h /? help"
        printfn ""
        0

    let helpArgKeys =
        set [
            "--help"
            "help"
            "-h"
            "/?"
        ]

    let isHelpRequested =
        argv 
        |> Array.exists (fun a ->
            helpArgKeys.Contains a
        )

    if isHelpRequested then
        printHelp ()
        |> exit
    
    // Command-line argument parsing ------------------------------------------

    let parseArg idx defaultValue =
        if argv.Length > idx then
            match Int32.TryParse argv.[idx] with
            | true, v -> v
            | _ -> defaultValue
        else
            defaultValue

    let w = parseArg 0 Limits.DefaultWidth
    let h = parseArg 1 Limits.DefaultHeight
    let m = parseArg 2 Limits.DefaultMineCount
    let seed = parseArg 3 (int DateTime.UtcNow.Ticks)

    let rng = Random seed

    // Limits -----------------------------------------------------------------

    let width = max (min w Limits.MaxBoardSize) Limits.MinBoardSize
    let height = max (min h Limits.MaxBoardSize) Limits.MinBoardSize
    let mines = max (min m Limits.MaxMineCount) Limits.MinMineCount

    if mines >= width * height then
        printfn "Error: too many mines for this board size!"
        exit 1

    let board = createBoard width height mines

    printfn ""
    printfn "┏━┛┛┏━ ┏━┛┏━┛┃┃┃┏━┛┏━┛┏━┃┏━┛┏━┃"
    printfn "┏━┛┃┃ ┃┏━┛━━┃┃┃┃┏━┛┏━┛┏━┛┏━┛┏┏┛"
    printfn "┛  ┛┛ ┛━━┛━━┛━━┛━━┛━━┛┛  ━━┛┛ ┛"
    printfn $"v{AppVersion}"
    printfn AppProjectUrl
    printfn ""
    printfn $"Board size: {width}x{height}, Mines: {mines}, Seed: {seed}"
    printfn CommandsHint

    gameLoop rng board
    0