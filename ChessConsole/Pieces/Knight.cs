using System.Collections.Generic;

namespace ChessConsole.Pieces
{
    public class Knight : Piece
    {
        /// <summary>
        /// Possible places where the knight can jump
        /// </summary>
        private ChessBoard.Cell[] possibleCells = new ChessBoard.Cell[8];

        public Knight(PlayerColor color)
            : base(color)
        {
            for (int i = 0; i < 8; i++)
            {
                possibleCells[i] = null;
            }
        }

        public Knight(Piece promote)
            : this(promote.Color)
        {
            Moved = promote.Moved;
        }

        public override IEnumerable<ChessBoard.Cell> PossibleMoves
        {
            get
            {
                foreach (ChessBoard.Cell cell in possibleCells)
                {
                    if (cell != null && (cell.Piece == null || cell.Piece.Color != Color))
                        yield return cell;
                }
            }
        }

        public override void Recalculate()
        {
            //2 up 1 left
            possibleCells[0] = Parent.Open(-1, 2);
            //2 down 1 left
            possibleCells[1] = Parent.Open(-1, -2);
            //2 up 1 right
            possibleCells[2] = Parent.Open(1, 2);
            //2 down 1 right
            possibleCells[3] = Parent.Open(1, -2);
            //1 up 2 left
            possibleCells[4] = Parent.Open(-2, 1);
            //1 down 2 left
            possibleCells[5] = Parent.Open(-2, -1);
            //1 up 2 right
            possibleCells[6] = Parent.Open(2, 1);
            //1 down 2 right
            possibleCells[7] = Parent.Open(2, -1);

            for (int i = 0; i < 8; i++)
            {
                if (possibleCells[i] != null)
                    possibleCells[i].HitBy.Add(this);
            }
        }

        public override bool IsBlockedIfMove(ChessBoard.Cell from, ChessBoard.Cell to, ChessBoard.Cell blocked)
        {
            //The knight's hits cannot be blocked
            for (int i = 0; i < 8; i++)
                if (possibleCells[i] == blocked)
                    return false;

            return true;
        }

        public override char Char => 'H'; //H for horse as we are using K for king
    }
}
