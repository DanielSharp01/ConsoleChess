using System;
using System.Linq;

namespace ChessConsole
{
    public enum PlayerColor
    {
        White, Black
    }

    public enum PlayerState
    {
        Idle, Holding, AwaitPromote, GameOver
    }

    public enum PromoteOptions
    {
        Queen = 0, Rook = 1, Bishop = 2, Knight = 3
    }

    public class ChessGame
    {
        /// <summary>
        /// False indicates the game should exit
        /// </summary>
        public bool Running
        {
            private set;
            get;
        }

        private PlayerState playerState;
        
        /// <summary>
        /// Currently selected promote option
        /// </summary>
        private PromoteOptions promoteOption;

        /// <summary>
        /// True for white, false for black
        /// </summary>
        private PlayerColor currentPlayer;
        
        /// <summary>
        /// Coordinates for the virtual cursor on the board
        /// </summary>
        private int cursorX, cursorY;

        /// <summary>
        /// The actual chess board
        /// </summary>
        private ChessBoard board;

        /// <summary>
        /// Currently holded piece's parent cell
        /// </summary>
        private ChessBoard.Cell holdedNode = null;

        /// <summary>
        /// Where to move
        /// </summary>
        private ChessBoard.Cell moveTo = null;

        public ChessGame()
        {
            Running = true;
            board = new ChessBoard();
            currentPlayer = PlayerColor.White;
            turnStart();
        }

        #region PublicInterfaceCommands
        public void Update()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.LeftArrow && cursorX > 0 && playerState != PlayerState.AwaitPromote)
                    cursorX--;
                else if (keyInfo.Key == ConsoleKey.RightArrow && cursorX < 7 && playerState != PlayerState.AwaitPromote)
                    cursorX++;
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    if (playerState != PlayerState.AwaitPromote && cursorY < 7)
                        cursorY++;
                    else if ((int)promoteOption > 0)
                        promoteOption--;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    if (playerState != PlayerState.AwaitPromote && cursorY > 0)
                        cursorY--;
                    else if ((int)promoteOption < 3)
                        promoteOption++;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                    interact();
                else if (keyInfo.Key == ConsoleKey.D)
                    debugInteract();
                else if (keyInfo.Key == ConsoleKey.Escape)
                    cancel();
            }
        }

        /// <summary>
        /// Draws the game
        /// </summary>
        /// <param name="g">ConsoleGraphics object to draw with/to</param>
        public void Draw(ConsoleGraphics g)
        {
            g.FillArea(new CChar(' ', ConsoleColor.Black, ConsoleColor.DarkGray), 10, 5, 8, 8);

           //7-j everywhere cuz it's reversed in chess
           for (int i = 0; i < 8; i++)
           {
                for (int j = 0; j < 8; j++)
                {
                    //Draw the symbol
                    ChessBoard.Cell cell = board.GetCell(i, j);
                    if (cell.Piece != null)
                    {
                        g.DrawTransparent(cell.Piece.Char, (cell.Piece.Color == PlayerColor.White) ? ConsoleColor.White : ConsoleColor.Black, 10 + i, 5 + (7 - j));
                        if (cell.Piece.LegalMoves.Count == 0)
                        {
                            g.SetBackground(ConsoleColor.DarkRed, 10 + i, 5 + (7 - j));
                        }
                    }

                    if (cell.HitBy.Contains(debugPiece))
                        g.SetBackground(ConsoleColor.DarkMagenta, 10 + i, 5 + (7 - j));
                }
            }

            if (holdedNode != null && playerState == PlayerState.Holding)
            {
                //Highlight legal moves
                foreach (ChessBoard.Cell move in holdedNode.Piece.LegalMoves)
                {
                    g.SetBackground(ConsoleColor.DarkGreen, 10 + move.X, 5 + (7 - move.Y));
                }
            }
            
            //Sets the cursor color -> yellow
            g.SetBackground(ConsoleColor.DarkYellow, 10 + cursorX, 5 + (7 - cursorY));

            //TODO: Remove en passant testing
            /*if (board.EnPassant != null)
                g.SetBackground(ConsoleColor.DarkCyan, 10 + board.EnPassant.X, 5 + (7 - board.EnPassant.Y));

            if (board.EnPassantCapture != null)
                g.SetBackground(ConsoleColor.DarkMagenta, 10 + board.EnPassantCapture.X, 5 + (7 - board.EnPassantCapture.Y));*/

            //Lighten for checkerboard pattern
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i + j) % 2 == 1) g.LightenBackground(10 + i, 5 + j);
                }
            }

            //Promotion option menu

            if (playerState == PlayerState.AwaitPromote)
            {
                g.DrawTextTrasparent("Queen", promoteOption == PromoteOptions.Queen ? ConsoleColor.Yellow : ConsoleColor.White, 22, 7);
                g.DrawTextTrasparent("Rook", promoteOption == PromoteOptions.Rook ? ConsoleColor.Yellow : ConsoleColor.White, 22, 9);
                g.DrawTextTrasparent("Bishop", promoteOption == PromoteOptions.Bishop ? ConsoleColor.Yellow : ConsoleColor.White, 22, 11);
                g.DrawTextTrasparent("Knight", promoteOption == PromoteOptions.Knight ? ConsoleColor.Yellow : ConsoleColor.White, 22, 13);
            }
            else
            {
                g.ClearArea(22, 7, 6, 7);
            }
        }

        #endregion

        #region EventHandlerLikeMethods

        /// <summary>
        /// Happens when the user presses the enter key
        /// </summary>
        private void interact()
        {
            switch (playerState)
            {
                case PlayerState.Idle:
                    holdedNode = board.GetCell(cursorX, cursorY);

                    if (holdedNode.Piece == null || holdedNode.Piece.Color != currentPlayer || holdedNode.Piece.LegalMoves.Count == 0)
                    {
                        holdedNode = null;
                        return;
                    }
                    else playerState = PlayerState.Holding;


                    break;
                case PlayerState.Holding:
                    playerState = PlayerState.Holding;

                    moveTo = board.GetCell(cursorX, cursorY);

                    if (!holdedNode.Piece.LegalMoves.Contains(moveTo))
                    {
                        moveTo = null;
                        return;
                    }

                    if (board.IsPromotable(holdedNode, moveTo))
                        showPromote();
                    else
                        turnOver();
                    
                    break;
                case PlayerState.AwaitPromote:
                    turnOver();
                    break;
                case PlayerState.GameOver:
                    Running = false;
                    break;
            }
        }


        private Piece debugPiece;
        private void debugInteract()
        {
            debugPiece = board.GetCell(cursorX, cursorY).Piece;
        }

        /// <summary>
        /// Happens when the user presses the escape key
        /// </summary>
        private void cancel()
        {
            playerState = PlayerState.Idle;
            holdedNode = null;
        }

        #endregion

        #region EventLikeMethods
        /// <summary>
        /// Called on every turn start
        /// </summary>
        private void turnStart()
        {
            board.TurnStart(currentPlayer);
        }

        /// <summary>
        /// Shows promotion dialog (set's the state)
        /// </summary>
        private void showPromote()
        {
            playerState = PlayerState.AwaitPromote;
            promoteOption = PromoteOptions.Queen; //reset the menu
        }

        /// <summary>
        /// Called when the turn is passed to the other player
        /// </summary>
        private void turnOver()
        {
            board.Move(holdedNode, moveTo, promoteOption);
            holdedNode = null;
            moveTo = null;
            playerState = PlayerState.Idle;
            currentPlayer = currentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;
            turnStart();
        }
        #endregion
    }
}
