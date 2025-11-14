open System

// Types ----------------------------------------------------------------------

type Cell = {
    IsMine : bool
    IsRevealed : bool
    IsFlagged : bool
    Adjacent : int
}

type Board = {
    Width : int
    Height : int
    MineCount : int
    Cells : Map<int*int, Cell>
    FirstMoveDone : bool 
}

let inBounds w h (x, y) =
    x >= 0 
    && y >= 0 
    && x < w 
    && y < h

let neighbors w h (x, y) =
    [ for dx in -1..1 do
        for dy in -1..1 do
            if not (dx = 0 && dy = 0) then
                let nx, ny = 
                    x + dx,
                    y + dy

                if inBounds w h (nx, ny) then
                    yield (nx, ny) 
    ]

// Board creation and mine placement ------------------------------------------

let createBoard w h m =
    if w <= 0 || h <= 0 then
        invalidArg "size" "Width/Height must be positive."
    if m <= 0 || m >= w * h then
        invalidArg "mines" "Mine count must be between 1 and (width * height - 1)."

    let cells =
        [ for y in 0..h - 1 do
            for x in 0..w - 1 ->
                (x, y),
                {   IsMine = false
                    IsRevealed = false
                    IsFlagged = false
                    Adjacent = 0
                }
        ]
        |> Map.ofList

    {   Width = w
        Height = h
        MineCount = m
        Cells = cells
        FirstMoveDone = false
    }

let private placeMines (rng:Random) w h m safe =
    let forbidden = safe :: neighbors w h safe

    let rec loop mines count =
        if count = m then
            mines
        else
            let x = rng.Next w
            let y = rng.Next h

            if List.contains (x, y) forbidden
                || Set.contains (x, y) mines
            then
                loop mines count
            else
                loop (Set.add (x, y) mines) (count + 1)
                
    loop Set.empty 0

let private computeAdjacency w h mines =
    [ for y in 0..h-1 do
        for x in 0..w-1 ->
            let adj =
                if Set.contains (x, y) mines then
                    0
                else
                    neighbors w h (x, y) 
                    |> List.sumBy (fun n ->
                        if Set.contains n mines then 1 else 0
                    )

            (x, y), adj
    ]

let applyMinesToBoard b mines =
    let adjMap =
        computeAdjacency b.Width b.Height mines
        |> Map.ofList

    let cells =
        b.Cells
        |> Map.map (fun (x, y) _ ->
            {   IsMine = Set.contains (x, y) mines
                IsRevealed = false
                IsFlagged = false
                Adjacent = adjMap.[(x, y)]
            })

    { b with
        Cells = cells
        FirstMoveDone = true 
    }

// Reveal / Flag logic --------------------------------------------------------

let rec private floodReveal b queue (seen:Set<int*int>) =
    match queue with
    | [] -> b
    | (x, y)::rest ->
        if seen.Contains(x, y) then
            floodReveal b rest seen
        else
            let c = b.Cells.[(x, y)]
            let marked = 
                c.IsRevealed
                || c.IsFlagged
                || c.IsMine

            if marked then
                floodReveal b rest (seen.Add(x, y))
            else
                let updatedCell =
                    { c with IsRevealed = true }
                let newBoard = 
                    { b with Cells = b.Cells.Add((x, y), updatedCell) }

                if c.Adjacent = 0 then
                    let neigh = neighbors b.Width b.Height (x, y)
                    floodReveal newBoard (rest @ neigh) (seen.Add(x, y))
                else
                    floodReveal newBoard rest (seen.Add(x, y))

let toggleFlag b (x, y) =
    match b.Cells.TryFind(x, y) with
    | Some c when not c.IsRevealed ->
        let updated =
            { c with IsFlagged = not c.IsFlagged }

        { b with Cells = b.Cells.Add((x, y), updated) }
    | _ -> b

type RevealResult =
    | Revealed of Board
    | HitMine of Board

let reveal rng b (x, y) =
    if not (inBounds b.Width b.Height (x, y)) then
        Revealed b
    else
        let b2 =
            if b.FirstMoveDone then
                b
            else
                let mines =
                    placeMines rng b.Width b.Height b.MineCount (x, y)
                
                applyMinesToBoard b mines
        
        let c = b2.Cells.[(x, y)]

        if c.IsFlagged || c.IsRevealed then
            Revealed b2
        elif c.IsMine then
            let revealedAll =
                b2.Cells
                |> Map.map (fun _ cell -> 
                    if cell.IsMine then
                        { cell with IsRevealed = true } 
                    else
                        cell
                )

            HitMine { b2 with Cells = revealedAll }
        else
            Revealed (floodReveal b2 [(x, y)] Set.empty)

let isWin b =
    b.Cells
    |> Map.forall (fun _ c ->
        c.IsMine || c.IsRevealed
    )

// Rendering ------------------------------------------------------------------

let cprintf color text =
    let old = Console.ForegroundColor
    Console.ForegroundColor <- color
    printf "%s" text
    Console.ForegroundColor <- old

let cprintfn color text =
    cprintf color text
    printfn ""

let ccRed = ConsoleColor.Red
let ccGray = ConsoleColor.Gray
let ccYellow = ConsoleColor.Yellow
let ccGreen = ConsoleColor.Green

let ccNumbers =
    dict [
        1, ConsoleColor.Blue
        2, ConsoleColor.Green
        3, ConsoleColor.Red
        4, ConsoleColor.DarkBlue
        5, ConsoleColor.DarkRed
        6, ConsoleColor.Cyan
        7, ConsoleColor.Magenta
        8, ConsoleColor.DarkGray
    ]

let printHCharsLine width =
    printfn
        "    %s"
        (String.init width (fun i -> 
            string(char(int 'a' + i)) + " "
        ))

let printHLine width =
    for _ in 1..width do printf "--"

let drawBoard b =
    printfn ""
    printHCharsLine b.Width
    printf "   +"
    printHLine b.Width
    printfn "+"

    for y in 0..b.Height - 1 do
        printf "%3d|" (y + 1)

        for x in 0..b.Width - 1 do
            let c = b.Cells.[(x, y)]

            if c.IsRevealed then
                if c.IsMine then
                    cprintf ccRed "x "
                elif c.Adjacent = 0 then
                    printf ". "
                else
                    Console.ForegroundColor <- ccNumbers.[c.Adjacent] 
                    printf "%d " c.Adjacent
                    Console.ResetColor()
            else
                if c.IsFlagged then
                    cprintf ccYellow "⚑ "
                else
                    cprintf ccGray "██"
        printf "|%d" (y + 1)
        printfn ""

    printf "   +"
    printHLine b.Width
    printfn "+"
    printHCharsLine b.Width

// Game loop ------------------------------------------------------------------

[<Literal>]
let CommandsHint = "Commands |> r X Y (r-Reveal) | f X Y (f-Flag) | q (q-Quit)"

let waitBeforeClose () =
    printf "↳ Enter to close..."
    Console.ReadLine () |> ignore

let rec gameLoop rng b =
    drawBoard b

    if isWin b then
        cprintfn ccGreen "~ You win! $_$"
        waitBeforeClose ()
        exit 0

    printf "|> "
    let input = Console.ReadLine()

    match input with
    | null -> ()
    | s ->
        let parts =
            s.Trim().Split(
                [|' '; '\t'|],
                StringSplitOptions.RemoveEmptyEntries)

        let isNumber (ys:string) =
            Int32.TryParse ys 
            |> fst

        let getCoords (xs:string) (ys:string) =
            int xs.[0] - int 'a',
            int ys - 1

        match parts |> Array.toList with
        | ["q" | "quit" | "exit"] -> ()
        | ["f"; xs; ys] when ys |> isNumber ->
            getCoords xs ys
            |> toggleFlag b
            |> gameLoop rng
        | ["r"; xs; ys] when ys |> isNumber ->
            let step =
                getCoords xs ys
                |> reveal rng b

            match step with
            | Revealed b2 -> 
                gameLoop rng b2
            | HitMine b2 ->
                drawBoard b2
                cprintfn ccRed "~ Boom! You hit a mine x_X"
                waitBeforeClose ()
        | _ ->
            printfn CommandsHint
            gameLoop rng b

[<Literal>]
let AppName = "FineSweeper"
[<Literal>]
let AppVersion = "0.11.14"
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
        printfn "Usage:"
        printfn "  finesweeper [width] [height] [mines] [seed]"
        printfn ""
        printfn "Arguments:"
        printfn "  width  - board width [3..26] default 12"
        printfn "  height - board height [3..26] default 12"
        printfn "  mines  - number of mines [3..99] default 10"
        printfn "  seed   - optional random seed (integer, optional)"
        printfn ""
        printfn "Example runs:"
        printfn "  finesweeper"
        printfn "  finesweeper 10 10 20"
        printfn "  finesweeper 15 15 30 54321"
        printfn "  finesweeper --help -h /? help"
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

    let w = parseArg 0 12
    let h = parseArg 1 12
    let m = parseArg 2 10
    let seed = parseArg 3 (int DateTime.UtcNow.Ticks)
    
    let rng = Random seed

    // Limits -----------------------------------------------------------------

    let width = max (min w 26) 3
    let height = max (min h 26) 3
    let mines = max (min m 99) 3

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