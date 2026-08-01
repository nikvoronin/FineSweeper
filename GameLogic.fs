module GameLogic

open Types
open BoardSetup

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
