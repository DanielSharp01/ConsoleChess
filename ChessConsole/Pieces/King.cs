using System.Collections.Generic;

namespace ChessConsole.Pieces
{
    public class King : Piece
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
        private Direction[] directions = new Direction[8];

        /// <summary>
        /// All the nodes we are listening to to perform castling to the left
        /// </summary>
        private ChessBoard.Cell[] listenedForCastleLeft = new ChessBoard.Cell[4];

        /// <summary>
        /// Shows if we can castle to the left
        /// </summary>
        private bool canCastleLeft;

        /// <summary>
        /// All the nodes we are listening to to perform castling to the right
        /// </summary>
        private ChessBoard.Cell[] listenedForCastleRight = new ChessBoard.Cell[3];

        /// <summary>
        /// Shows if we can castle to the right
        /// </summary>
        private bool canCastleRight;

        public King(PlayerColor color)
            : base(color)
        {
            for (int i = 0; i < 8; i++)
            {
                directions[i] = null;
            }
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

                    if (canCastleLeft)
                    {
                        yield return listenedForCastleLeft[2];
                    }

                    if (canCastleRight)
                    {
                        yield return listenedForCastleRight[1];
                    }
                }
            }
        }

        protected override void recalculatePossibleMoves()
        {
            //If moved castling is not possible anymore an we should also remove listeners
            if (Moved)
            {
                if (listenedForCastleLeft != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        listenedForCastleLeft[i].NodeChanged -= CastleLeftChanged;
                    }
                    listenedForCastleLeft = null;
                }

                if (listenedForCastleRight != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        listenedForCastleRight[i].NodeChanged -= CastleRightChanged;
                    }
                    listenedForCastleLeft = null;
                }
                canCastleLeft = false;
                canCastleRight = false;
            }

            foreach (Direction direction in directions)
            {
                if (direction != null) direction.Dispose();
            }

            //Open upward direction and listen to it
            directions[0] = new Direction(this, 0, 1, 1);
            //Open downward direction and listen to it
            directions[1] = new Direction(this, 0, -1, 1);
            //Open leftward direction and listen to it
            directions[2] = new Direction(this, -1, 0, 1);
            //Open rightward direction and listen to it
            directions[3] = new Direction(this, 1, 0, 1);
            //Open up left direction and listen to it
            directions[4] = new Direction(this, -1, 1, 1);
            //Open up right direction and listen to it
            directions[5] = new Direction(this, 1, 1, 1);
            //Open down left direction and listen to it
            directions[6] = new Direction(this, -1, -1, 1);
            //Open down right direction and listen to it
            directions[7] = new Direction(this, 1, -1, 1);

            //We only have to set up castling on the first move
            if (!Moved)
            {
                int castleRow = (Color == PlayerColor.White) ? 0 : 7;
                listenedForCastleLeft[0] = Parent.Parent.GetNode(0, castleRow);
                listenedForCastleLeft[1] = Parent.Parent.GetNode(1, castleRow);
                listenedForCastleLeft[2] = Parent.Parent.GetNode(2, castleRow);
                listenedForCastleLeft[3] = Parent.Parent.GetNode(3, castleRow);


                for (int i = 0; i < 4; i++)
                {
                    listenedForCastleLeft[i].NodeChanged += CastleLeftChanged;
                }

                //Invoke so that if we can castle in the beginning (in a real game this never happens)
                CastleLeftChanged(null);

                listenedForCastleRight[0] = Parent.Parent.GetNode(5, castleRow);
                listenedForCastleRight[1] = Parent.Parent.GetNode(6, castleRow);
                listenedForCastleRight[2] = Parent.Parent.GetNode(7, castleRow);

                for (int i = 0; i < 3; i++)
                {
                    listenedForCastleRight[i].NodeChanged += CastleRightChanged;
                }

                //Invoke so that if we can castle in the beginning (in a real game this never happens)
                CastleRightChanged(null);
            }
        }

        public override bool IsBlockedIfMove(ChessBoard.Cell from, ChessBoard.Cell to, ChessBoard.Cell blocked)
        {
            foreach (Direction direction in directions)
            {
                if (!direction.IsBlockedIfMove(from, to, blocked)) return false;
            }

            return true;
        }

        private void CastleLeftChanged(ChessBoard.Cell node)
        {
            //The left rook moved so remove the listeners and set canCastleLeft to false indefinetely
            if (node == listenedForCastleLeft[0])
            {
                if (listenedForCastleLeft != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        listenedForCastleLeft[i].NodeChanged -= CastleLeftChanged;
                    }
                }
                listenedForCastleLeft = null;

                canCastleLeft = false;
                return;
            }

            //If all intermediates are gone then we can castle
            if (listenedForCastleLeft[1].Piece == null && listenedForCastleLeft[2].Piece == null && listenedForCastleLeft[3].Piece == null)
            {
                canCastleLeft = true;
            }
        }

        private void CastleRightChanged(ChessBoard.Cell node)
        {
            //The right rook moved so remove the listeners and set canCastleRight to false indefinetely
            if (node == listenedForCastleRight[2])
            {
                if (listenedForCastleRight != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        listenedForCastleRight[i].NodeChanged -= CastleLeftChanged;
                    }
                }
                listenedForCastleRight = null;

                canCastleRight = false;
                return;
            }

            //If all intermediates are gone then we can castle
            if (listenedForCastleRight[0].Piece == null && listenedForCastleRight[1].Piece == null)
            {
                canCastleRight = true;
            }
        }

        public override char Char => 'K';
    }
}
