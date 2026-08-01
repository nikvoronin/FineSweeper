module Types

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
