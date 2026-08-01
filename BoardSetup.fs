module BoardSetup

open System
open Types

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

let placeMines (rng:Random) w h m safe =
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
