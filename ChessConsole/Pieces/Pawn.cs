using System.Collections.Generic;

namespace ChessConsole.Pieces
{
    public class Pawn : Piece
    {
        public override int PossibleMoveCount => forward.GetPossibleMoveCount(false) + (canHit(hits[0]) ? 1 : 0) + (canHit(hits[1]) ? 1 : 0);

        /// <summary>
        /// Represents the forward direction moves of the pawn
        /// </summary>
        private Direction forward = null;

        /// <summary>
        /// Represents the hitables of the pawn
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
                foreach (ChessBoard.Cell node in forward.GetPossibleMoves(false))
                {
                    yield return node;
                }

                //We don't use listeners for these guys as they are fairly staright forward to check
                if (canHit(hits[0]))
                    yield return hits[0];
                if (canHit(hits[1]))
                    yield return hits[1];
            }
        }

        protected override void recalculatePossibleMoves()
        {
            //Clear previous listeners
            if (forward != null)
                forward.Dispose();

            //Open forward direction and listen to it
            forward = new Direction(this, 0, (Color == PlayerColor.White) ? 1 : -1, Moved ? 1 : 2, false);

            hits[0] = Parent.Open(-1, (Color == PlayerColor.White) ? 1 : -1);
            hits[1] = Parent.Open( 1, (Color == PlayerColor.White) ? 1 : -1);

            if (hits[0] != null)
            {
                hits[0].HitBy.Add(this);
                Hitting.Add(hits[0]);
            }
            if (hits[1] != null)
            {
                hits[1].HitBy.Add(this);
                Hitting.Add(hits[1]);
            }
        }

        public override bool IsBlockedIfMove(ChessBoard.Cell from, ChessBoard.Cell to, ChessBoard.Cell blocked)
        {
            //The pawn's hits cannot be blocked
            return hits[0] != blocked && hits[1] != blocked;
        }

        public override char Char => 'P';

        protected override bool canHit(ChessBoard.Cell node)
        {
            //Handling en passant over here
            return base.canHit(node) || (node != null && node == node.Parent.EnPassant);
        }
    }
}
