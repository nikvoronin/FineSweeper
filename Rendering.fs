module Rendering

open System
open Types

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
    Map [
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
                    let color = ccNumbers |> Map.tryFind c.Adjacent |> Option.defaultValue ccGray
                    cprintf color $"{c.Adjacent} "
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
