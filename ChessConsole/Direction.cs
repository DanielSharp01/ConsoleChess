using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessConsole
{
    /// <summary>
    /// Contains possible moves and handles line of sight checks
    /// </summary>
    public class Direction : IDisposable
    {
        /// <summary>
        /// The piece whose moves are represented by this object
        /// </summary>
        public Piece Piece
        {
            private set;
            get;
        }

        /// <summary>
        /// The X-direction
        /// </summary>
        public int X
        {
            private set;
            get;
        }

        /// <summary>
        /// The Y-direction
        /// </summary>
        public int Y
        {
            private set;
            get;
        }

        /// <summary>
        /// The possible moves you can make in this direction including the line of sight blocking piece that may or may not be hittable.
        /// See also <seealso cref="GetPossibleMoves"/>
        /// </summary>
        private List<ChessBoard.Cell> possibleMoves;

        /// <summary>
        /// The possible moves you can make in this direction
        /// </summary>
        /// <param name="enemyHittable">Are the enemy pieces hittable</param>
        /// <returns>An enumeration of possible moves</returns>
        public IEnumerable<ChessBoard.Cell> GetPossibleMoves(bool enemyHittable = true)
        {
            if (possibleMoves.Count == 0)
                yield break;

            for (int i = 0; i < possibleMoves.Count - 1; i++)
            {
                yield return possibleMoves[i];
            }

            if (possibleMoves.Last().Piece == null)
                yield return possibleMoves.Last();
            else if (enemyHittable && possibleMoves.Last().Piece.Color != Piece.Color)
                yield return possibleMoves.Last();
        }

        /// <summary>
        /// The count of possible moves
        /// </summary>
        /// <param name="enemyHittable">Are the enemy pieces hittable</param>
        /// <returns>The count of possible moves</returns>
        public int GetPossibleMoveCount(bool enemyHittable = true)
        {
            if (possibleMoves.Count == 0)
                return 0;

            if (possibleMoves.Last().Piece == null)
                return possibleMoves.Count;
            else if (!enemyHittable || possibleMoves.Last().Piece.Color == Piece.Color)
                return possibleMoves.Count - 1;
            else
                return possibleMoves.Count;
        }

        /// <summary>
        /// The number of moves that we could take, considering no blocking or out of board.
        /// </summary>
        public int DesiredCount
        {
            private set;
            get;
        }

        /// <summary>
        /// Tells if the direction should update the hit graph of possible move nodes
        /// </summary>
        private bool updateHitGraph;

        public Direction(Piece piece, int x, int y, int desiredCount = 8, bool updateHitGraph = true)
        {
            Piece = piece;
            X = x;
            Y = y;
            DesiredCount = desiredCount;
            this.updateHitGraph = updateHitGraph;

            possibleMoves = new List<ChessBoard.Cell>();
            possibleMoves.AddRange(piece.Parent.OpenLineOfSight(x, y, desiredCount));

            foreach (ChessBoard.Cell move in possibleMoves)
            {
                move.NodeChanged += pathNodeChanged;
                if (updateHitGraph)
                {
                    move.HitBy.Add(Piece);
                    Piece.Hitting.Add(move);
                }
            }
        }

        private void pathNodeChanged(ChessBoard.Cell node)
        {
            //If the node.Piece changed into a null then that was only a change if a piece was there blocking our sight
            if (node.Piece == null && node == possibleMoves.Last())
            {
                foreach (ChessBoard.Cell move in node.OpenLineOfSight(X, Y, DesiredCount - possibleMoves.Count))
                {
                    move.NodeChanged += pathNodeChanged;
                    if (updateHitGraph)
                    {
                        move.HitBy.Add(Piece);
                        Piece.Hitting.Add(move);
                    }
                    possibleMoves.Add(move);
                }
            }
            //If we got a new intermediate node then that blocks our sight
            else if (node.Piece != null)
            {
                //If a node changed that's in the line of sight and not the blocker than logically it has to be that a new blocker was introduced
                int index = possibleMoves.IndexOf(node);
                for (int i = index + 1; i < possibleMoves.Count; ) //NOTE: We are not incrementing i as we would have to decrement it every iteration
                {
                    possibleMoves[i].NodeChanged -= pathNodeChanged;
                    if (updateHitGraph)
                    {
                        possibleMoves[i].HitBy.Remove(Piece);
                        Piece.Hitting.Remove(possibleMoves[i]);
                    }
                    possibleMoves.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Tells if the moved piece on the node changed the hit state of the blocked 
        /// </summary>
        /// <param name="from">Where the piece stands right now</param>
        /// <param name="to">Where the piece is moved</param>
        /// <param name="blocked">Hit tests this piece</param>
        /// <returns>If blocked is hittable after moving the from</returns>
        public bool IsBlockedIfMove(ChessBoard.Cell from, ChessBoard.Cell to, ChessBoard.Cell blocked)
        {
            if (possibleMoves.Contains(blocked) && !possibleMoves.Contains(to))
            {
                //The blocked is hittable to begin with and we don't block it with a new blocker
                //To may still equal blocked but direction should not care about that
                return false;
            }
            else if (possibleMoves.Contains(from))
            {
                int toIndex = possibleMoves.IndexOf(to);
                if (0 <= toIndex && toIndex < possibleMoves.Count - 1)
                    return true; //The blocker closer to the piece
                else
                {
                    //If we moved further
                    foreach (ChessBoard.Cell move in from.OpenLineOfSight(X, Y, DesiredCount - possibleMoves.Count))
                    {
                        if (move == to) //The blocker moved into the new path
                            return true;
                        if (move == blocked) //The blocked is hittable
                            return false;
                    }
                }
            }

            //Happens when the blocker was not cotained and the blocked was not contained a perfect combination for nothing happening
            return true;
        }

        /// <summary>
        /// Removes the listeners to avoid memory leak
        /// </summary>
        public void Dispose()
        {
            foreach (ChessBoard.Cell move in possibleMoves)
            {
                move.NodeChanged -= pathNodeChanged;
            }
        }
    }
}
