using System.Collections.Generic;

namespace ChessConsole.Pieces
{
    public class Bishop : Piece
    {
        public override int PossibleMoveCount
        {
            get
            {
                int sum = 0;
                foreach (Direction direction in directions)
                {
                    sum += direction.GetPossibleMoveCount();
                }

                return sum;
            }
        }

        /// <summary>
        /// Represents the directions of movement
        /// </summary>
        private Direction[] directions = new Direction[4];

        public Bishop(PlayerColor color)
            : base(color)
        {
            for (int i = 0; i < 4; i++)
            {
                directions[i] = null;
            }
        }

        public Bishop(Piece promote)
            : this(promote.Color)
        {
            Moved = promote.Moved;
        }

        public override IEnumerable<ChessBoard.Cell> PossibleMoves
        {
            get
            {
                foreach (Direction direction in directions)
                {
                    foreach (ChessBoard.Cell node in direction.GetPossibleMoves())
                    {
                        yield return node;
                    }
                }
            }
        }

        protected override void recalculatePossibleMoves()
        {
            foreach (Direction direction in directions)
            {
                if (direction != null) direction.Dispose();
            }

            //Open up left direction and listen to it
            directions[0] = new Direction(this, -1, 1);
            //Open up right direction and listen to it
            directions[1] = new Direction(this, 1, 1);
            //Open down left direction and listen to it
            directions[2] = new Direction(this, -1, -1);
            //Open down right direction and listen to it
            directions[3] = new Direction(this, 1, -1);
        }

        public override bool IsBlockedIfMove(ChessBoard.Cell from, ChessBoard.Cell to, ChessBoard.Cell blocked)
        {
            foreach (Direction direction in directions)
            {
                if (!direction.IsBlockedIfMove(from, to, blocked)) return false;
            }

            return true;
        }

        public override char Char => 'B';
    }
}
