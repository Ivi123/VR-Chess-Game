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
            var botPiecesThatCanMove = 
                botPieces
                    .Where(piece => piece.Moves.Count != 0)
                    .ToList();
            
            var pieceToMoveIndex = Random.Range(0, botPiecesThatCanMove.Count);
            var pieceToMove = botPiecesThatCanMove[pieceToMoveIndex];

            var moveToMakeIndex = Random.Range(0, pieceToMove.Moves.Count );

            var moves = pieceToMove.Moves;

            var moveToMake = moves[moveToMakeIndex];
            var moveTile = TileManager.GetTile(moveToMake.Coords);

            moveTile.IsAvailableTile = moveToMake.Type == Shared.MoveType.Normal;
            moveTile.IsAttackTile = moveToMake.Type is Shared.MoveType.Attack or Shared.MoveType.EnPassant;
            moveTile.IsSpecialTile =
                moveToMake.Type is Shared.MoveType.EnPassant or Shared.MoveType.ShortCastle
                    or Shared.MoveType.LongCastle;

            var botTurn = MovementManager.MakeMove(pieceToMove, moveTile, false);

            moveTile.IsAvailableTile = false;
            moveTile.IsAttackTile = false;
            moveTile.IsSpecialTile = false;

            return botTurn;
        }
    }
}
