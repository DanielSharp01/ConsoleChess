using ChessConsole.Pieces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChessConsole
{
    /// <summary>
    /// Provides the data structure and algorithms for dealing with a chess board
    /// </summary>
    public class ChessBoard
    {
        /// <summary>
        /// Represents one chess cell in the <seealso cref="ChessBoard.cells"/>
        /// </summary>
        public class Cell
        {
            /// <summary>
            /// The parent chess board
            /// </summary>
            public ChessBoard Parent
            {
                private set;
                get;
            }

            /// <summary>
            /// 0-7 -> A-H mapping on an actual chessboard 
            /// </summary>
            public int X;
            /// <summary>
            /// 0-7 -> 1-8 mapping on an actual chessboard
            /// </summary>
            public int Y;

            /// <summary>
            /// The piece present at the cell (can be null). Should only be set by <see cref="ChessBoard"/>.
            /// </summary>
            public Piece Piece;

            /// <summary>
            /// All the pieces that can hit this cell.
            /// </summary>
            public List<Piece> HitBy;

            public Cell(ChessBoard parent, int x, int y)
            {
                Parent = parent;
                HitBy = new List<Piece>();
                X = x;
                Y = y;
            }

            /// <summary>
            /// Returns cells on the board in a relative direction to this one.
            /// </summary>
            /// <param name="dirX">The X-direction component in which to search</param>
            /// <param name="dirY">The Y-direction component in which to search</param>
            /// <param name="desiredCount">The ammount of consecutive cells to return (until outside of the board)</param>
            /// <returns>Collection of cells which are in the line of sight</returns>
            public IEnumerable<Cell> OpenLineOfSight(int dirX, int dirY, int desiredCount = 1)
            {
                for (int i = 1; i <= desiredCount; i++)
                {
                    //Query the parent for a cell, if null the cell is out of the board and we should stop
                    Cell cell = Parent.GetCell(X + dirX * i, Y + dirY * i);
                    if (cell == null) yield break;

                    yield return cell;

                    //Stop anyway as line of sight is blocked
                    if (cell.Piece != null)
                        yield break;
                }
            }

            /// <summary>
            /// Returns a cell on the board relative to this one.
            /// </summary>
            /// <param name="x">Relative X-coordinate of the cell</param>
            /// <param name="y">Relative X-coordinate of the cell</param>
            /// <returns>Node at (x, y) position or null if index is out of bounds</returns>
            public Cell Open(int x, int y)
            {
                //Query the parent for a cell, if null the cell is out of the board and we should not return
                Cell cell = Parent.GetCell(X + x, Y + y);
                return cell ?? null;
            }
        }

        /// <summary>
        /// Contains information about the cells and the links between them
        /// </summary>
        private Cell[,] cells;

        /// <summary>
        /// The cell to hit for en passant
        /// </summary>
        public Cell EnPassant
        {
            private set;
            get;
        }

        /// <summary>
        /// The cell where the pawn will be captured after en passant is performed
        /// </summary>
        public Cell EnPassantCapture

        {
            private set;
            get;
        }

        /// <summary>
        /// List holding all the existing pieces
        /// </summary>
        private List<Piece> pieces = new List<Piece>();

        private Piece blackKing;
        private Piece whiteKing;

        /// <summary>
        /// Caches the <see cref = "IsInCheck" /> method's result
        /// </summary>
        private bool inCheck;

        public ChessBoard()
        {
            Reset();
        }

        #region Getters

        /// <summary>
        /// Get cell by absolute coordinates
        /// </summary>
        /// <param name="x">Absolute X-coord</param>
        /// <param name="y">Absolute Y-coord</param>
        /// <returns>Node at (x, y) position or null if index is out of bounds</returns>
        public Cell GetCell(int x, int y)
        {
            if (x < 0 || cells.GetLength(0) <= x || y < 0 || cells.GetLength(1) <= y) return null;

            return cells[x, y];
        }

        #endregion

        #region HelperMethods

        /// <summary>
        /// Adds a piece in the beggining of the chess game, can also be used for promotion
        /// </summary>
        /// <param name="cell">The cell to add to</param>
        /// <param name="piece"></param>
        private void addPiece(Cell cell, Piece piece)
        {
            cell.Piece = piece;
            pieces.Add(piece);
            piece.OnPlace(cell);
        }

        #endregion

        #region InterfaceMethods

        /// <summary>
        /// Resets the board state
        /// </summary>
        public void Reset()
        {
            cells = new Cell[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    cells[i, j] = new Cell(this, i, j);
                }
            }

            pieces.Clear();

            EnPassant = null;
            EnPassantCapture = null;

            addPiece(cells[0, 0], new Rook(PlayerColor.White));
            addPiece(cells[1, 0], new Knight(PlayerColor.White));
            addPiece(cells[2, 0], new Bishop(PlayerColor.White));
            addPiece(cells[3, 0], new Queen(PlayerColor.White));
            addPiece(cells[4, 0], (whiteKing = new King(PlayerColor.White)));
            addPiece(cells[5, 0], new Bishop(PlayerColor.White));
            addPiece(cells[6, 0], new Knight(PlayerColor.White));
            addPiece(cells[7, 0], new Rook(PlayerColor.White));

            addPiece(cells[0, 1], new Pawn(PlayerColor.White));
            addPiece(cells[1, 1], new Pawn(PlayerColor.White));
            addPiece(cells[2, 1], new Pawn(PlayerColor.White));
            addPiece(cells[3, 1], new Pawn(PlayerColor.White));
            addPiece(cells[4, 1], new Pawn(PlayerColor.White));
            addPiece(cells[5, 1], new Pawn(PlayerColor.White));
            addPiece(cells[6, 1], new Pawn(PlayerColor.White));
            addPiece(cells[7, 1], new Pawn(PlayerColor.White));

            addPiece(cells[0, 6], new Pawn(PlayerColor.Black));
            addPiece(cells[1, 6], new Pawn(PlayerColor.Black));
            addPiece(cells[2, 6], new Pawn(PlayerColor.Black));
            addPiece(cells[3, 6], new Pawn(PlayerColor.Black));
            addPiece(cells[4, 6], new Pawn(PlayerColor.Black));
            addPiece(cells[5, 6], new Pawn(PlayerColor.Black));
            addPiece(cells[6, 6], new Pawn(PlayerColor.Black));
            addPiece(cells[7, 6], new Pawn(PlayerColor.Black));

            addPiece(cells[0, 7], new Rook(PlayerColor.Black));
            addPiece(cells[1, 7], new Knight(PlayerColor.Black));
            addPiece(cells[2, 7], new Bishop(PlayerColor.Black));
            addPiece(cells[3, 7], new Queen(PlayerColor.Black));
            addPiece(cells[4, 7], (blackKing = new King(PlayerColor.Black)));
            addPiece(cells[5, 7], new Bishop(PlayerColor.Black));
            addPiece(cells[6, 7], new Knight(PlayerColor.Black));
            addPiece(cells[7, 7], new Rook(PlayerColor.Black));

            foreach (Piece piece in pieces)
            {
                piece.Recalculate();
            }
        }

        /// <summary>
        /// Called at the start of every turn. Recalcualtes legal moves.
        /// </summary>
        /// <param name="currentPlayer">The player whose turn is to move</param>
        /// <returns>Whether the player had any moves</returns>
        public bool TurnStart(PlayerColor currentPlayer)
        {
            inCheck = IsInCheck(currentPlayer, false);
            bool anyLegalMove = false;
            //Clear cell hit lists
            foreach (Cell cell in cells)
            {
                cell.HitBy.Clear();
            }

            //Recalculate possible moves and hit lists for cells
            foreach (Piece piece in pieces)
            {
                piece.Recalculate();
            }

            //Calculate legal moves
            foreach (Piece piece in pieces)
            {
                piece.LegalMoves.Clear();
                foreach (Cell move in piece.PossibleMoves)
                {
                    if (piece.Color == currentPlayer && isMoveLegal(piece, move))
                    {
                        piece.LegalMoves.Add(move);
                        anyLegalMove = true;
                    }
                }
            }

            return anyLegalMove;
        }

        /// <summary>
        /// Validates if a move is legal for a given piece
        /// </summary>
        /// <param name="piece">Piece to move</param>
        /// <param name="move">Where the piece moves</param>
        /// <returns></returns>
        private bool isMoveLegal(Piece piece, Cell move)
        {
            Piece currentKing = piece.Color == PlayerColor.White ? whiteKing : blackKing; 
            //The strategy is to try everything that can fail and return true only if nothing fails

            //If it's the king check if it moved into a check (or didn't move out that's really the same thing)
            if (piece is King)
            {
                //If some enemy hits where we move we can't move with the king
                foreach (Piece hitter in move.HitBy)
                {
                    if (hitter.Parent != move && hitter.Color != piece.Color)
                        return false;
                }

                //Validate castling
                if (Math.Abs(move.X - piece.Parent.X) == 2)
                {
                    //You can't castle in check
                    if (inCheck)
                        return false;

                    //Check if some enemy hits the middle castling
                    foreach (Piece hitter in GetCell(move.X > piece.Parent.X ? move.X - 1 : move.X + 1, move.Y).HitBy)
                    {
                        if (hitter.Color != piece.Color)
                            return false;
                    }
                }
            }
            else //Non-king pieces
            {
                if (inCheck) //If player is in in check and if move resolves that
                {
                    //Let's try capturing or blocking the attacker, keep in mind that we can't unblock another attacker
                    foreach (Piece hitter in currentKing.Parent.HitBy)
                    {
                        if (hitter.Color == currentKing.Color) continue; //Same color don't care
                        if (hitter.Parent == move) continue; //Was captured
                        if (hitter.IsBlockedIfMove(piece.Parent, move, currentKing.Parent)) continue;

                        return false;
                    }
                }

                //Check if a blocker moving away results in a check
                //This also prevents pieces capturing an attacker and exposing the king to another
                foreach (Piece hitter in piece.Parent.HitBy)
                {
                    if (hitter.Color == currentKing.Color) continue; //If it's the same color we don't care
                    if (hitter.Parent == move) continue; //If we hit it it can not block

                    if (!hitter.IsBlockedIfMove(piece.Parent, move, currentKing.Parent))
                        return false;
                }
            }


            return true;
        }

        /// <summary>
        /// Checks if a player is currently in check (their king can be hit)
        /// </summary>
        /// <param name="player">The player's color to check</param>
        /// <param name="useCache">Uses the cached value for the current turn</param>
        /// <returns></returns>
        public bool IsInCheck(PlayerColor player, bool useCache = true)
        {
            if (useCache)
                return inCheck;

            if (player == PlayerColor.White)
                return whiteKing.Parent.HitBy.Any(hitter => hitter.Color != player);
            else
                return blackKing.Parent.HitBy.Any(hitter => hitter.Color != player);
        }

        /// <summary>
        /// Move a piece from one cell the the other, after this function is called the turn MUST end
        /// </summary>
        /// <param name="from">Node where the moved piece is</param>
        /// <param name="to">Node to move to</param>
        /// <param name="promoteOption">The option chosed when promoting a pawn, will be ignored if the movement does not involve pormotion.</param>
        public void Move(Cell from, Cell to, PromoteOptions promoteOption)
        {
            //Capture a piece if moved on it
            if (to.Piece != null)
                pieces.Remove(to.Piece);

            to.Piece = from.Piece;
            from.Piece = null;

            //Handles en passant captures
            if (to == EnPassant && to.Piece is Pawn)
            {
                pieces.Remove(EnPassantCapture.Piece);
                EnPassantCapture.Piece = null;
            }

            //Castling to the right
            if (to.Piece is King && to.X - from.X == 2)
            {
                Move(GetCell(7, to.Y), GetCell(to.X - 1, to.Y), promoteOption); //Move the rook as well
            }

            //Castling to the left
            if (to.Piece is King && to.X - from.X == -2)
            {
                Move(GetCell(0, to.Y), GetCell(to.X + 1, to.Y), promoteOption); //Move the rook as well
            }

            //Handles promotion
            if (to.Piece is Pawn && to.Y == (to.Piece.Color == PlayerColor.White ? 7 : 0))
            {
                Piece promoted = null; //we have to set it to null cuz C# complains
                switch (promoteOption)
                {
                    case PromoteOptions.Queen:
                        promoted = new Queen(to.Piece);
                        break;
                    case PromoteOptions.Rook:
                        promoted = new Rook(to.Piece);
                        break;
                    case PromoteOptions.Bishop:
                        promoted = new Bishop(to.Piece);
                        break;
                    case PromoteOptions.Knight:
                        promoted = new Knight(to.Piece);
                        break;
                }

                //Update the list with the new promoted piece
                pieces.Remove(to.Piece);
                to.Piece = promoted;
                promoted.OnPlace(to); //Place it otherwise weird bugs occur
                pieces.Add(promoted);
            }

            //The code has to be in this exact order to prevent from listeners firing when we move into our own listened cells.
            //Recalculate possible moves
            to.Piece.OnMove(to);
            to.Piece.Recalculate();

            //Resets en passant
            EnPassant = null;
            EnPassantCapture = null;

            //Handles en passant detection
            if (to.Piece is Pawn && Math.Abs(to.Y - from.Y) == 2)
            {
                EnPassant = GetCell(to.X, (from.Y > to.Y) ? from.Y - 1 : from.Y + 1);
                EnPassantCapture = to;
            }
        }

        /// <summary>
        /// Is a piece (usually a pawn) promotable if it's moving from a cell to another
        /// </summary>
        /// <param name="from">The cell where the piece moves from</param>
        /// <param name="to">The cell where the piece moves to</param>
        /// <returns>Whether the piece on from is promotable</returns>
        public bool IsPromotable(Cell from, Cell to)
        {
            return from.Piece is Pawn && to.Y == (from.Piece.Color == PlayerColor.White ? 7 : 0);
        }

        #endregion
    }
}
