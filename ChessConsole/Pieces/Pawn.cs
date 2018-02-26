using System.Collections.Generic;

namespace ChessConsole.Pieces
{
    public class Pawn : Piece
    {
        /// <summary>
        /// Represents the forward direction moves of the pawn
        /// </summary>
        private Direction forward = null;

        /// <summary>
        /// Represents the hittables of the pawn
        /// </summary>
        private ChessBoard.Cell[] hits = new ChessBoard.Cell[2];

        public Pawn(PlayerColor color)
            : base(color)
        {
            hits[0] = hits[1] = null;
        }

        public override IEnumerable<ChessBoard.Cell> PossibleMoves
        {
            get
            {
                foreach (ChessBoard.Cell cell in forward.GetPossibleMoves(false))
                {
                    yield return cell;
                }

                if (canHit(hits[0]))
                    yield return hits[0];
                if (canHit(hits[1]))
                    yield return hits[1];
            }
        }

        public override void Recalculate()
        {
            //Open forward direction and listen to it
            forward = new Direction(this, 0, (Color == PlayerColor.White) ? 1 : -1, Moved ? 1 : 2, false);

            hits[0] = Parent.Open(-1, (Color == PlayerColor.White) ? 1 : -1);
            hits[1] = Parent.Open( 1, (Color == PlayerColor.White) ? 1 : -1);

            if (hits[0] != null)
                hits[0].HitBy.Add(this);
            if (hits[1] != null)
                hits[1].HitBy.Add(this);
        }

        public override bool IsBlockedIfMove(ChessBoard.Cell from, ChessBoard.Cell to, ChessBoard.Cell blocked)
        {
            //The pawn's hits cannot be blocked
            return hits[0] != blocked && hits[1] != blocked;
        }

        public override char Char => 'P';

        protected override bool canHit(ChessBoard.Cell cell)
        {
            //Handling en passant over here
            return base.canHit(cell) || (cell != null && cell == cell.Parent.EnPassant);
        }
    }
}
