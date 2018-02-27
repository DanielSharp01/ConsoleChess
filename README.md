# ConsoleChess

A simple chess game in the C# console

## If you find an issue:

To help development please provide the moves performed to reproduce this bug. If the bug is not always reproducable, steps can still help.

Moves should be provided as:

`FROM -> TO (promotion if promoted)`
eg. `G2->F2`

## ChessBoard Features:

- [x] All pieces with basic movement
- [x] Line of sight testing for appropriate pieces
- [x] En passant
- [x] Castling
- [x] Promotion
- [x] Hit graphs for legal move resolution
- [x] Legal move resolution - so that you can't put yourself in check
- [x] Check and any legal move detection

## Documentation for the standalone ChessBoard:

This section provides information to use the `ChessBoard` class with your own `Game` class.

### TurnStart

You have to call this method everyturn and after every move.

At turn start you might want to check for win and draw conditions. The two easiest (checkmate/stalemate) can be done this way:

```csharp
bool hasMoves = board.TurnStart(currentPlayer);
if (!hasMoves)
{
    //Turn start sets up a cached inCheck value as well
    if (board.IsInCheck(currentPlayer))
    {
        //Checkmate, win for (currentPlayer == PlayerColor.White) ? Black : White
    }
    else
    {
        //Stalemate, draw
    }
}
```

### Moving

The board has no way of knowing that you picked a piece or are about to put down a piece.

I'm going to provide you with typical examples of what you would want to accomplish.

#### Picking a piece

```csharp
Piece holding = board.GetCell(cursorX, cursorY).Piece;

if (holding != null)
{
    if (holding.LegalMoves.Count == 0)
    {
        //Do some sort of unpicking logic so that you can't get stuck with picking a piece that can't move
        holding = null; //As an example
    }
    else
    {
        //Set internal state variables to allow for movement etc.
    }
}
```

#### Attempting to put down a piece

Before attempting to put down a piece, you should ask the board if a promotion may occur. The system was designed to be able to cancel this promotion process. And you obviously need to not move before being able to cancel it. This is why the `Move` metod takes `PromoteOptions` as parameter. It will automatically perform the promotion of a pawn if it has to. If a promotion will however not occur, this parameter will be ignored so you can safely pass anything as your default.

To decide if a promotion is about to occur, then use the `IsPromotable` function.

```csharp
to = board.GetCell(cursorX, cursorY);

if (holding != null) //We'll check, even though you should have logic to not be able to try move if you don't hold anything
{
    //Check if it's contained as a legal move
    if (!holding.LegalMoves.Contains(to))
    {
        //You might wanna cancel the current move here

        return; //assuming this is a function
    }

    //This function tells if your piece (usually a pawn) is about to be promoted
    if (board.IsPromotable(holding.Parent, to))
    {
        //Handle promotion...
    }

    //Finally tell the board to move the piece
    board.Move(holding.Parent, to, currentPromoteOption);
}
```

### Starting a new game

Constructing a chess board automatically resets the game, so that's a viable option.

If you'd like the board also contains a Reset method which does exactly the same thing as the constructor.

```csharp

board = new ChessBoard();
//...
//Worst game loop ever
while (!exit)
{
    //...
    while (!gameOver)
    {
        //...
    }

    //...
    board.Reset();
}
```