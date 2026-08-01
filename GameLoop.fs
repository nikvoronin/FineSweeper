module GameLoop

open System
open GameLogic
open Rendering

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

        let isValidX (xs:string) =
            xs.Length = 1
            && xs.[0] >= 'a'
            && xs.[0] <= 'z'

        let getCoords (xs:string) (ys:string) =
            int xs.[0] - int 'a',
            int ys - 1

        match parts |> Array.toList with
        | ["q" | "quit" | "exit"] -> ()
        | ["f"; xs; ys] when xs |> isValidX && ys |> isNumber ->
            getCoords xs ys
            |> toggleFlag b
            |> gameLoop rng
        | ["r"; xs; ys] when xs |> isValidX && ys |> isNumber ->
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
