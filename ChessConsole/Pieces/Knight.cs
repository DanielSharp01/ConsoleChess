using System.Collections.Generic;

namespace ChessConsole.Pieces
{
    public class Knight : Piece
    {
        public override int PossibleMoveCount
        {
            get
            {
                int sum = 0;
                foreach (ChessBoard.Cell node in possibleNodes)
                {
                    if (node != null && (node.Piece == null || node.Piece.Color != Color))
                        sum++;
                }

                return sum;
            }
        }

        /// <summary>
        /// Possible places where the knight can jump
        /// </summary>
        private ChessBoard.Cell[] possibleNodes = new ChessBoard.Cell[8];

        public Knight(PlayerColor color)
            : base(color)
        {
            for (int i = 0; i < 8; i++)
            {
                possibleNodes[i] = null;
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
                foreach (ChessBoard.Cell node in possibleNodes)
                {
                    if (node != null && (node.Piece == null || node.Piece.Color != Color))
                        yield return node;
                }
            }
        }

        protected override void recalculatePossibleMoves()
        {
            //2 up 1 left
            possibleNodes[0] = Parent.Open(-1, 2);
            //2 down 1 left
            possibleNodes[1] = Parent.Open(-1, -2);
            //2 up 1 right
            possibleNodes[2] = Parent.Open(1, 2);
            //2 down 1 right
            possibleNodes[3] = Parent.Open(1, -2);
            //1 up 2 left
            possibleNodes[4] = Parent.Open(-2, 1);
            //1 down 2 left
            possibleNodes[5] = Parent.Open(-2, -1);
            //1 up 2 right
            possibleNodes[6] = Parent.Open(2, 1);
            //1 down 2 right
            possibleNodes[7] = Parent.Open(2, -1);

            for (int i = 0; i < 8; i++)
            {
                if (possibleNodes[i] != null)
                {
                    possibleNodes[i].HitBy.Add(this);
                    Hitting.Add(possibleNodes[i]);
                }
            }
        }

        public override bool IsBlockedIfMove(ChessBoard.Cell from, ChessBoard.Cell to, ChessBoard.Cell blocked)
        {
            //The knight's hits cannot be blocked
            for (int i = 0; i < 8; i++)
                if (possibleNodes[i] == blocked)
                    return false;

            return true;
        }

        public override char Char => 'H'; //H for hose as we are using K for king
    }
}
