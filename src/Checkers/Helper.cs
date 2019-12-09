﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Checkers
{
    public static class Helper
    {
        public static IEnumerable<Move> CalculateMoves(Game board, int position)
        {
            return CalculateMoves(board, Position.FromNumber(position));
        }

        public static IEnumerable<Move> CalculateMoves(Game board, Position position)
        {
            var moves = new List<Move>();

            if (CanPieceMoveRight(board, position, out var moveR))
                moves.Add(moveR);

            if (CanPieceMoveLeft(board, position, out var moveL))
                moves.Add(moveL);

            //Eat is obligatory
            if (CanPieceEat(board, position, out var moveE) && moveE.Any())
                return moveE;

            return moves;
        }

        public static Tile ChangeColor(Tile a)
        {
            if (a == Tile.White || a == Tile.QueenWhite)
                return Tile.Black;
            if (a == Tile.Black || a == Tile.QueenBlack)
                return Tile.White;
            return Tile.Empty;
        }
        public static Tile[] GetTileColors(Tile a)
        {
            if (a == Tile.White)
                return new[] { Tile.White, Tile.QueenWhite };
            if (a == Tile.Black)
                return new[] { Tile.Black, Tile.QueenBlack };
            return null;
        }


        #region Simple Move
        static bool CanMovePiece(Game board, Position position, int rowDirection, int columnDirection, out Move move)
        {
            move = null;

            if (!Position.IsCoordValid(position.Row + rowDirection, position.Column + columnDirection))
                return false;

            Position newPosition = Position.FromCoors(position.Row + rowDirection, position.Column + columnDirection);
            var moveTile = board.GetTile(newPosition);
            if (moveTile == Tile.Empty)
            {
                move = new Move
                {
                    From = position,
                    To = newPosition
                };
                return true;
            }


            return false;
        }

        static bool CanPieceMoveRight(Game board, Position position, out Move move)
        {
            var tile = board.GetTile(position);
            move = null;

            if (tile == Tile.Empty)
                return false;

            if (tile == Tile.White)
            {
                return CanMovePiece(board, position, 1, 1, out move);
            }

            else if (tile == Tile.Black)
            {
                return CanMovePiece(board, position, -1, -1, out move);
            }

            return false;
        }

        static bool CanPieceMoveLeft(Game board, Position position, out Move move)
        {
            var tile = board.GetTile(position);
            move = null;

            if (tile == Tile.Empty)
                return false;

            if (tile == Tile.White)
            {
                return CanMovePiece(board, position, 1, -1, out move);
            }

            else if (tile == Tile.Black)
            {
                return CanMovePiece(board, position, -1, 1, out move);
            }

            return false;
        }
        #endregion

        #region Eat

        static bool CanPieceEat(Game board, Position position, out List<Eat> move)
        {
            var tile = board.GetTile(position);
            move = new List<Eat>();

            if (tile == Tile.Empty)
                return false;

            move = CanPieceEat(board, position);
            return true;
        }

        static List<Eat> CanPieceEat(Game board, Position position)
        {
            var moves = new List<Eat>();
            var tile = board.GetTile(position);

            if (tile == Tile.Empty)
                return moves;

            var moveDirections = new[] {
                Tuple.Create(1,1),
                Tuple.Create(1,-1),
                Tuple.Create(-1,-1),
                Tuple.Create(-1,1)
            };

            foreach (var direction in moveDirections)
            {
                var rowDirection = direction.Item1;
                var columnDirection = direction.Item2;

                bool pieceInsideTheBoard = Position.Try(position.Row + rowDirection, position.Column + columnDirection, out Position toEatPosition);
                bool isAEnemyTile = pieceInsideTheBoard && EnemyTile(board.GetTile(toEatPosition), tile);
                bool positionIsValidAfterMove = Position.IsCoordValid(position.Row + (rowDirection * 2), position.Column + (columnDirection * 2));
                bool thePositionAfterMoveIsEmpty = positionIsValidAfterMove && board.GetTile(Position.FromCoors(position.Row + (rowDirection * 2), position.Column + (columnDirection * 2))) == Tile.Empty;

                if (pieceInsideTheBoard &&
                    isAEnemyTile &&
                    positionIsValidAfterMove &&
                    thePositionAfterMoveIsEmpty)
                {
                    var to = Position.FromCoors(position.Row + (rowDirection * 2), position.Column + (columnDirection * 2));
                    var from = position;

                    var eatMove = new Eat
                    {
                        Eaten = new List<Position> { toEatPosition },
                        From = from,
                        To = to
                    };
                    var newBoard = board.Move(eatMove);
                    var eatMoves = CanPieceEat(newBoard, to);

                    if (eatMoves.Any())
                        moves.AddRange(eatMoves.Select(m =>
                        {
                            var list = new List<Position>() { toEatPosition };
                            list.AddRange(m.Eaten);
                            return new Eat
                            {
                                From = from,
                                To = m.To,
                                Eaten = list
                            };
                        }));
                    else
                        moves.Add(eatMove);
                }
            }

            return moves;
        }

        public static bool EnemyTile(Tile a, Tile b)
        {
            switch (a)
            {
                case Tile.Black:
                    return b == Tile.White || b == Tile.QueenWhite;
                case Tile.White:
                    return b == Tile.Black || b == Tile.QueenBlack;
            }

            return false;
        }

        #endregion
    }
}
