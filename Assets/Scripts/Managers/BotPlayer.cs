using System.Collections.Generic;
using System.Linq;
using ChessLogic;
using ChessPieces;
using UnityEngine;

namespace Managers
{
    public class BotPlayer : MonoBehaviour
    {
        //Possible Info?
        public MovementManager MovementManager { get; set; }
        public TileManager TileManager { get; set; }

        public Turn BotMakeMove(List<ChessPiece> botPieces)
        {
            var botPiecesThatCanMove = botPieces.Where(piece =>
                    piece.GetAllPossibleMoves().Count != 0 || piece.Moves.SpecialMoves.Count != 0)
                .ToList();
            
            var pieceToMoveIndex = Random.Range(0, botPiecesThatCanMove.Count);
            var pieceToMove = botPiecesThatCanMove[pieceToMoveIndex];

            var moveToMakeIndex = Random.Range(0,
                pieceToMove.GetAllPossibleMoves().Count + pieceToMove.Moves.SpecialMoves.Count);

            var moves = new List<Vector2Int>(pieceToMove.GetAllPossibleMoves());
            moves.AddRange(pieceToMove.Moves.SpecialMoves.Select(sp => sp.Coords));

            var moveToMakeCoords = moves[moveToMakeIndex];
            var moveTile = TileManager.GetTile(moveToMakeCoords);
            
            if (pieceToMove.Moves.AvailableMoves.Contains(moveToMakeCoords))
                moveTile.IsAvailableTile = true;
            
            if (pieceToMove.Moves.AttackMoves.Contains(moveToMakeCoords))
                moveTile.IsAttackTile = true;
            
            if(pieceToMove.Moves.SpecialMoves.Select(sp => sp.Coords).ToList().Contains(moveToMakeCoords))
            {
                moveTile.IsSpecialTile = true;
                switch (pieceToMove.Moves.FindSpecialMoveTypeFromCoords(moveToMakeCoords))
                {
                    case Shared.MoveType.EnPassant:
                        moveTile.IsAttackTile = true;
                        break;
                    case Shared.MoveType.LongCastle:
                    case Shared.MoveType.ShortCastle:
                        moveTile.IsAvailableTile = true;
                        break;
                }
            }

            var botTurn = MovementManager.MakeMove(pieceToMove, moveTile, false);

            moveTile.IsAvailableTile = false;
            moveTile.IsAttackTile = false;
            moveTile.IsSpecialTile = false;

            return botTurn;
        }
    }
}
