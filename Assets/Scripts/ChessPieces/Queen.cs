using System.Collections.Generic;
using ChessLogic;

namespace ChessPieces
{
    public class Queen : ChessPiece
    {
        public void Awake()
        {
            pieceScore = 9;
        }

        public override void CalculateAvailablePositions(ChessPiece[,] board, Tile[,] tiles)
        {
            protectsKing = false;
            Moves = new List<Move>();

            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(board, tiles, this, 1, 0));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(board, tiles, this, 0, 1));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(board, tiles, this, -1, 0));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(board, tiles, this, 0, -1));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(board, tiles, this, 1, 1));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(board, tiles, this, -1, 1));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(board, tiles, this, 1, -1));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(board, tiles, this, -1, -1));
        }

        public override void MarkAttackedTiles(ChessPiece[,] board, Tile[,] tiles)
        {
            var markForExclusion = new List<Move>();
            foreach (var move in Moves)
            {
                AddToTileAttackingPieces(tiles, move.Coords);
                if (move.Type != Shared.MoveType.Normal) continue;
                var occupationType = MovementManager.CalculateSpaceOccupation(board, move.Coords, team);
                if (occupationType == Shared.TileOccupiedBy.FriendlyPiece) markForExclusion.Add(move);
            }

            foreach (var move in markForExclusion)
                Moves.Remove(move);
        }
    }
}
