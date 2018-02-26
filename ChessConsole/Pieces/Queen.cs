using System.Collections.Generic;

namespace ChessConsole.Pieces
{
    public class Queen : Piece
    {
        /// <summary>
        /// Represents the directions of movement
        /// </summary>
        private Direction[] directions = new Direction[8];

        public Queen(PlayerColor color)
            : base(color)
        {
            for (int i = 0; i < 8; i++)
            {
                directions[i] = null;
            }
        }

        public Queen(Piece promote)
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
                    foreach (ChessBoard.Cell cell in direction.GetPossibleMoves())
                    {
                        yield return cell;
                    }
                }
            }
        }

        public override void Recalculate()
        {
            //Open upward direction and listen to it
            directions[0] = new Direction(this, 0, 1);
            //Open downward direction and listen to it
            directions[1] = new Direction(this, 0, -1);
            //Open leftward direction and listen to it
            directions[2] = new Direction(this, -1, 0);
            //Open rightward direction and listen to it
            directions[3] = new Direction(this, 1, 0);
            //Open up left direction and listen to it
            directions[4] = new Direction(this, -1, 1);
            //Open up right direction and listen to it
            directions[5] = new Direction(this, 1, 1);
            //Open down left direction and listen to it
            directions[6] = new Direction(this, -1, -1);
            //Open down right direction and listen to it
            directions[7] = new Direction(this, 1, -1);
        }

        public override bool IsBlockedIfMove(ChessBoard.Cell from, ChessBoard.Cell to, ChessBoard.Cell blocked)
        {
            foreach (Direction direction in directions)
            {
                //If any direction can hit the blocked return false
                if (!direction.IsBlockedIfMove(from, to, blocked)) return false;
            }

            return true;
        }

        public override char Char => 'Q';
    }
}
