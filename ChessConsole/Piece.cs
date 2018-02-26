using System.Collections.Generic;

namespace ChessConsole
{
    /// <summary>
    /// Represents an abstract chess piece
    /// </summary>
    public abstract class Piece
    {
        /// <summary>
        /// Color of piece
        /// </summary>
        public PlayerColor Color
        {
            private set;
            get;
        }

        /// <summary>
        /// False by default, set to true upon first move
        /// </summary>
        public bool Moved
        {
            protected set;
            get;
        }

        /// <summary>
        /// All the moves possible to make with this piece
        /// </summary>
        public abstract IEnumerable<ChessBoard.Cell> PossibleMoves
        {
            get;
        }

        /// <summary>
        /// All the moves legal to make with this piece. It's a subset of <see cref="PossibleMoves"/>.
        /// See also <seealso cref="ChessBoard.IsMoveLegal(Piece, ChessBoard.Cell)"/>.
        /// </summary>
        public List<ChessBoard.Cell> LegalMoves
        {
            private set;
            get;
        }

        /// <summary>
        /// All the nodes that this piece can hit. See also <seealso cref="ChessBoard.Cell.HitBy"/>.
        /// </summary>
        public List<ChessBoard.Cell> Hitting = new List<ChessBoard.Cell>();

        /// <summary>
        /// The number of possible moves. NOTE: Not the same as legal moves, that is not handled by the piece.
        /// </summary>
        public abstract int PossibleMoveCount { get; }

        public ChessBoard.Cell Parent
        {
            private set;
            get;
        }

        public Piece(PlayerColor color)
        {
            Color = color;
            Moved = false;
            LegalMoves = new List<ChessBoard.Cell>();
        }

        /// <summary>
        /// Called when the piece is first placed or when the piece is replaced after promotion.
        /// Does not recalculate just yet, you have to call <see cref="recalculatePossibleMoves"/> for that.
        /// </summary>
        public void OnPlace(ChessBoard.Cell node)
        {
            Parent = node;
        }

        /// <summary>
        /// Called when the piece is moved.
        /// Does not recalculate just yet, you have to call <see cref="recalculatePossibleMoves"/> for that.
        /// </summary>
        public void OnMove(ChessBoard.Cell node)
        {
            Parent = node;
            Moved = true;
        }

        /// <summary>
        /// Called when the piece is created or moved to recalculate possible moves.
        /// </summary>
        public void Recalculate()
        {
            foreach (ChessBoard.Cell hitNode in Hitting)
            {
                hitNode.HitBy.Remove(this);
            }
            Hitting.Clear();

            recalculatePossibleMoves();
        }

        /// <summary>
        /// Recalculates the possible moves and updates the hit graph
        /// </summary>
        protected abstract void recalculatePossibleMoves();

        /// <summary>
        /// Tells if the moved piece on the node changed the hit state of the blocked 
        /// </summary>
        /// <param name="from">Where the piece stands right now</param>
        /// <param name="to">Where the piece is moved</param>
        /// <param name="blocked">Hit tests this piece</param>
        /// <returns>If blocked is hittable after moving the from</returns>
        public abstract bool IsBlockedIfMove(ChessBoard.Cell from, ChessBoard.Cell to, ChessBoard.Cell blocked);

        public abstract char Char { get; }

        protected virtual bool canHit(ChessBoard.Cell node)
        {
            return node != null && node.Piece != null && node.Piece.Color != Color;
        }
    }
}
