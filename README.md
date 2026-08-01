# FineSweeper

A minimalistic, functional Minesweeper implementation in F# for the terminal. No mutable state, pure console gameplay with colored characters.

```plain
>
┏━┛┛┏━ ┏━┛┏━┛┃┃┃┏━┛┏━┛┏━┃┏━┛┏━┃
┏━┛┃┃ ┃┏━┛━━┃┃┃┃┏━┛┏━┛┏━┛┏━┛┏┏┛
┛  ┛┛ ┛━━┛━━┛━━┛━━┛━━┛┛  ━━┛┛ ┛
```

- [Interface](#interface)
    - [Command-line parameters](#command-line-parameters)
    - [In-Game Commands](#in-game-commands)
- [How-To Build Release](#how-to-build-release)

**System requirements:** Windows 10 x64, .NET Desktop Runtime 10.0.\
Linux and macOS are also supported with the corresponding .NET runtime.

> [!NOTE]
> For the best experience, use a modern UTF-8 terminal with emoji support, such as Windows Terminal (wt).

## Interface

![finesweeper_console-001](https://github.com/user-attachments/assets/12a47cee-35c4-419a-b104-a78f810942f9)

### Command-line parameters

> finesweeper [width] [height] [mines] [seed]

#### Arguments

- width  - board width [3..26] default 12
- height - board height [3..26] default 12
- mines  - number of mines [3..99] default 10
- seed   - optional random seed (signed integer)

> [!NOTE]
> The board will be the same for the same seed value.

#### Example runs

```shell
finesweeper
finesweeper 10 10 20
finesweeper 15 15 30 54321
finesweeper --help -h /? help
```

### In-Game Commands

- `r` X Y (r-Reveal) - Reveal the contents of the cell at column `X` and row `Y`.
    - `X` is a letter representing the horizontal coordinate (column).
    - `Y` is a number representing the vertical coordinate (row).
- `f` X Y (f-Flag) - Place or remove a flag on the cell at column `X` and row `Y` to mark a suspected mine.
- `q` (q-Quit) - Exit the game immediately.

For example:

```shell
|> r a 1
|> f e 3
|> q
```

## How-To Build Release

```shell
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true
```
