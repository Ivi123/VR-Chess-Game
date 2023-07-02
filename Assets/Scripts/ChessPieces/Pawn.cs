using System.Collections.Generic;
using System.Linq;
using ChessLogic;
using Managers;
using UnityEngine;

namespace ChessPieces
{
    public class Pawn : ChessPiece
    {
        public void Awake()
        {
            pieceScore = 1;
        }
        
        public bool IsEnPassantTarget { get; set; }

        public override void CalculateAvailablePositions(ChessPiece[,] board, Tile[,] tiles)
        {
            protectsKing = false;
            Moves = new List<Move>();
            var direction = Shared.TeamType.White.Equals(team) ? 1 : -1;
            var isPromotion = isMoved && (currentX is 1 or 6);

            Vector2Int possibleMoveOneTileAhead = new(currentX + (direction * 1), currentY);
            if (Shared.TileOccupiedBy.None == MovementManager.CalculateSpaceOccupation(board, possibleMoveOneTileAhead, team))
            {
                Moves.Add(new Move(possibleMoveOneTileAhead,
                    isPromotion ? Shared.MoveType.Promotion : Shared.MoveType.Normal));
                if (!isMoved)
                {
                    Vector2Int possibleMoveTwoTilesAhead = new(currentX + (direction * 2), currentY);
                    if (Shared.TileOccupiedBy.None ==
                        MovementManager.CalculateSpaceOccupation(board, possibleMoveTwoTilesAhead, team))
                        Moves.Add(new Move(possibleMoveTwoTilesAhead,
                            isPromotion ? Shared.MoveType.Promotion : Shared.MoveType.Normal));
                }
            }

            Vector2Int attackMoveLeft = new(currentX + (direction * 1), currentY + 1);
            var leftOccupationStatus = MovementManager.CalculateSpaceOccupation(board, attackMoveLeft, team);
            if (leftOccupationStatus != Shared.TileOccupiedBy.EndOfTable)
            {
                AddToTileAttackingPieces(tiles, attackMoveLeft);
                if (leftOccupationStatus == Shared.TileOccupiedBy.EnemyPiece)
                    Moves.Add(new Move(attackMoveLeft,
                        isPromotion ? Shared.MoveType.AttackPromotion : Shared.MoveType.Attack));
            }

            Vector2Int attackMoveRight = new(currentX + (direction * 1), currentY - 1);
            var rightOccupationSpace = MovementManager.CalculateSpaceOccupation(board, attackMoveRight, team);
            if (rightOccupationSpace != Shared.TileOccupiedBy.EndOfTable)
            {
                AddToTileAttackingPieces(tiles, attackMoveRight);
                if (rightOccupationSpace == Shared.TileOccupiedBy.EnemyPiece)
                    Moves.Add(new Move(attackMoveRight,
                        isPromotion ? Shared.MoveType.AttackPromotion : Shared.MoveType.Attack));
            }

            Moves.AddRange(CalculateSpecialMoves(board, tiles));
        }

        public override void MarkAttackedTiles(ChessPiece[,] board, Tile[,] tiles)
        {
            var direction = Shared.TeamType.White.Equals(team) ? 1 : -1;
            Vector2Int attackMoveLeft = new(currentX + (direction * 1), currentY + 1);
            Vector2Int attackMoveRight = new(currentX + (direction * 1), currentY - 1);

            var leftOccupationSpace = MovementManager.CalculateSpaceOccupation(board, attackMoveLeft, team);
            if (leftOccupationSpace != Shared.TileOccupiedBy.EndOfTable)
                AddToTileAttackingPieces(tiles, attackMoveLeft);
            
            var rightOccupationSpace = MovementManager.CalculateSpaceOccupation(board, attackMoveRight, team);
            if (rightOccupationSpace != Shared.TileOccupiedBy.EndOfTable)
                AddToTileAttackingPieces(tiles, attackMoveRight);

            var specialMoves = Moves.FindAll(m => m.Type == Shared.MoveType.EnPassant).ToList();
            foreach (var specialMove in specialMoves)
                AddToTileAttackingPieces(tiles, specialMove.Coords);
        }

        private List<Move> CalculateSpecialMoves(ChessPiece[,] board, Tile[,] tiles)
        {
            List<Move> moves = new();
            
            var direction = Shared.TeamType.White.Equals(team) ? 1 : -1;
            List<Vector2Int> possibleEnPassantTarget = new()
            {
                new Vector2Int(currentX, currentY + 1),
                new Vector2Int(currentX, currentY - 1)
            };
            
            List<Vector2Int> possibleEnPassantAttack = new()
            {
                new Vector2Int(currentX + (direction * 1), currentY + 1),
                new Vector2Int(currentX + (direction * 1), currentY - 1)
            };

            for (var i = 0; i < possibleEnPassantTarget.Count; i++)
            {
                var enPassantOccupation =
                    MovementManager.CalculateSpaceOccupation(board, possibleEnPassantTarget[i], team);
                if (enPassantOccupation != Shared.TileOccupiedBy.EnemyPiece) continue;
                
                var enemyPiece = board[possibleEnPassantTarget[i].x, possibleEnPassantTarget[i].y];
                if (enemyPiece is not Pawn pawn || !pawn.IsEnPassantTarget) continue;
                
                var specialMove = new Move(possibleEnPassantAttack[i], Shared.MoveType.EnPassant);
                moves.Add(specialMove);
                AddToTileAttackingPieces(tiles, possibleEnPassantAttack[i]);
            }
            
            return moves;
        }
    }
}
