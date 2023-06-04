using System;
using System.Collections.Generic;
using System.Linq;
using ChessLogic;
using ChessPieces;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Managers
{
    public class MovementManager : MonoBehaviour
    {
        // Managers
        public TileManager TileManager { get; set; }
        public GameManager GameManager { get; set; }
        
        //Movement and piece tracking properties
        public ChessPiece[,] ChessPieces { get; set; }
        public List<GameObject> WhitePieces { get; set; }
        private ChessPiece whiteKing;
        public List<GameObject> BlackPieces { get; set; }
        private ChessPiece blackKing;
        
        // Elimination Related Properties
        public LinkedList<Vector3> FreeWhiteEliminationPosition { get; set; }
        public LinkedList<Vector3> UsedWhiteEliminationPosition { get; set; }
        public LinkedList<Vector3> FreeBlackEliminationPosition { get; set; }
        public LinkedList<Vector3> UsedBlackEliminationPosition { get; set; }

        //----------------------------------------------- Methods ------------------------------------------------------

        public void SetKing(ChessPiece king)
        {
            if (king.team == Shared.TeamType.White)
            {
                whiteKing = king;
            }
            else
            {
                blackKing = king;
            }
        }
        
        public ChessPiece GetChessPiece(Vector2Int position)
        {
            return ChessPieces[position.x, position.y];
        }
        
        public void PieceWasPickedUp(int x, int y)
        {
            var pickedPieceCoord = new Vector2Int(x, y);
            var chessPiece = GetChessPiece(pickedPieceCoord);
            TileManager.UpdateTileMaterial(pickedPieceCoord, Shared.TileType.Selected);
            
            foreach (var move in chessPiece.Moves.AvailableMoves)
            {
                var moveCoord = new Vector2Int(move.x, move.y);
                TileManager.UpdateTileMaterial(moveCoord, 
                    TileManager.IsTileWhite(moveCoord) 
                        ? Shared.TileType.AvailableWhite 
                        : Shared.TileType.AvailableBlack);
                TileManager.Tiles[move.x, move.y].GetComponent<Tile>().IsAvailableTile = true;
            }

            foreach (var move in chessPiece.Moves.AttackMoves)
            {
                var moveCoord = new Vector2Int(move.x, move.y);
                TileManager.UpdateTileMaterial(moveCoord, 
                    TileManager.IsTileWhite(moveCoord) 
                        ? Shared.TileType.AttackTileWhite 
                        : Shared.TileType.AttackTileBlack);
                TileManager.Tiles[move.x, move.y].GetComponent<Tile>().IsAttackTile = true;
            }

            foreach (var move in chessPiece.Moves.SpecialMoves)
            {
                TileManager.UpdateTileMaterial(move.Coords, 
                    TileManager.IsTileWhite(move.Coords) 
                        ? Shared.TileType.AttackTileWhite 
                        : Shared.TileType.AttackTileBlack);
                
                TileManager.Tiles[move.Coords.x, move.Coords.y].GetComponent<Tile>().IsSpecialTile = true;
                TileManager.Tiles[move.Coords.x, move.Coords.y].GetComponent<Tile>().IsAttackTile = 
                    move.MoveType == Shared.MoveType.EnPassant;
            }

            var pickedPiece = ChessPieces[x, y];
            DisablePickUpOnOtherPieces(pickedPiece.gameObject, pickedPiece.team);
        }
    
        public void PieceWasDropped(int currentX, int currentY, Tile newTile)
        {
            var chessPiece = ChessPieces[currentX, currentY];

            // Re-enable XRGrabInteractable on the current team's pieces
            EnablePickUpOnPieces(chessPiece.team);

            // Update the currently picked chess piece's tile material from Selected to Default
            TileManager.UpdateTileMaterial(new Vector2Int(currentX, currentY), Shared.TileType.Default); 
            
            // Update the ChessPieces matrix with the new format after a chess piece was moved. The method returns
            // the turn that was just made with all the moved pieces and the changes in positions
            var turn = MakeMove(chessPiece, newTile);
            if(turn != null) GameManager.AdvanceTurn(turn);

            // Change back the Available/Attack/Special Tiles material back to the default value 
            TileManager.UpdateTileMaterialAfterMove(chessPiece);
            if(turn == null) return;
            GenerateAllMoves();
            EliminateInvalidMoves(GameManager.IsWhiteTurn);
        }

        private Turn MakeMove(ChessPiece chessPiece, Tile newTile)
        {
            if (newTile == null) return null;
            
            var movedPieces = new MovedPieces();
            var turn = 
                new Turn(movedPieces, Shared.MoveType.Normal, 
                    GameManager.IsWhiteTurn 
                        ? Shared.TeamType.White 
                        : Shared.TeamType.Black);
            
            var newPosition = newTile.Position;
            var currentPosition = new Vector2Int(chessPiece.currentX, chessPiece.currentY);

            if (newTile.IsAttackTile && !newTile.IsSpecialTile)
            {
                var enemyPiece = ChessPieces[newPosition.x, newPosition.y];
                movedPieces.AddNewPieceAndPosition(enemyPiece, MovedPieces.EliminationPosition);
                EliminatePiece(enemyPiece);
            }

            if (newTile.IsSpecialTile)
            {
                var specialMoveType = chessPiece.Moves.FindSpecialMoveTypeFromCoords(newTile.Position);
                switch (specialMoveType)
                {
                    case Shared.MoveType.EnPassant:
                        var direction = chessPiece.team == Shared.TeamType.White ? -1 : 1;
                        var enemyPiece = ChessPieces[newPosition.x + (direction * 1), newPosition.y];
                        turn.MoveType = Shared.MoveType.EnPassant;
                        movedPieces.AddNewPieceAndPosition(enemyPiece, MovedPieces.EliminationPosition);
                        EliminatePiece(enemyPiece);
                        break;
                    //case Shared.MoveType.Castling:
                    //    throw new NotImplementedException();
                    //    break;
                    //case Shared.MoveType.Promotion:
                    //    throw new NotImplementedException();
                    //    break;
                    //case Shared.MoveType.Normal:
                    //    break;
                    //default:
                    //    throw new ArgumentOutOfRangeException();
                }
            }
            movedPieces.AddNewPieceAndPosition(chessPiece, newTile.Position);
            chessPiece.transform.position = TileManager.GetTileCenter(newPosition.x, newPosition.y);
            ChessPieces[newPosition.x, newPosition.y] = chessPiece;
            ChessPieces[currentPosition.x, currentPosition.y] = null;
            chessPiece.currentX = newPosition.x;
            chessPiece.currentY = newPosition.y;

            if (chessPiece is Pawn pawn && Mathf.Abs(newPosition.x - currentPosition.x) == 2)
            {
                pawn.IsEnPassantTarget = true;
            }

            chessPiece.SavePosition();
            chessPiece.IsMoved = true;

            return turn;
        }

        public void UndoLastMove()
        {
            var turnToUndo = GameManager.LastTurn;
            if(turnToUndo == null) return;
            var movesToUndo = turnToUndo.PiecesMovedInThisTurn;

            UndoMove(movesToUndo);
            UndoMoveOnBoard(movesToUndo);
            
            GameManager.SwitchTurn();
            
            GenerateAllMoves();
            EliminateInvalidMoves(GameManager.IsWhiteTurn);
            
            GameManager.History.Remove(turnToUndo);
            GameManager.LastTurn = GameManager.History.Count == 0 ? null : GameManager.History[^1];
        }

        private void UndoMove(MovedPieces movesToUndo)
        {
            for (var i = movesToUndo.PositionChanges.Count - 1; i >= 0; i--)
            {
                var chessPieceToUndo = movesToUndo.Pieces[i];
                var (oldPosition, currentPosition) = movesToUndo.PositionChanges[i];

                if (currentPosition == MovedPieces.EliminationPosition)
                {
                    switch (chessPieceToUndo.team)
                    {
                        case Shared.TeamType.White:
                            WhitePieces.Add(chessPieceToUndo.gameObject);
                            FreeWhiteEliminationPosition.AddFirst(UsedWhiteEliminationPosition.First.Value);
                            UsedWhiteEliminationPosition.RemoveFirst();
                            break;
                        case Shared.TeamType.Black:
                            BlackPieces.Add(chessPieceToUndo.gameObject);
                            FreeBlackEliminationPosition.AddFirst(UsedBlackEliminationPosition.First.Value);
                            UsedBlackEliminationPosition.RemoveFirst();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    ChessPieces[currentPosition.x, currentPosition.y] = null;
                }

                ChessPieces[oldPosition.x, oldPosition.y] = chessPieceToUndo;
                chessPieceToUndo.currentX = oldPosition.x;
                chessPieceToUndo.currentY = oldPosition.y;
                if (chessPieceToUndo.startingPosition == oldPosition) chessPieceToUndo.IsMoved = false;
            }
        }

        private void UndoMoveOnBoard(MovedPieces movesToUndo)
        {
            for (var i = movesToUndo.PositionChanges.Count - 1; i >= 0; i--)
            {
                var chessPieceToUndo = movesToUndo.Pieces[i];
                var (oldPosition, currentPosition) = movesToUndo.PositionChanges[i];

                if (currentPosition == MovedPieces.EliminationPosition)
                    chessPieceToUndo.gameObject.GetComponent<XRGrabInteractable>().enabled = true;

                chessPieceToUndo.gameObject.transform.position =
                    TileManager.GetTileCenter(oldPosition.x, oldPosition.y);
                chessPieceToUndo.SavePosition();
            }
        }
        
        private void DisablePickUpOnOtherPieces(GameObject pickedPiece, Shared.TeamType team)
        {
            if (Shared.TeamType.White.Equals(team))
            {
                foreach (var piece in WhitePieces.Where(piece => !piece.Equals(pickedPiece)))
                {
                    piece.GetComponent<XRGrabInteractable>().enabled = false;
                }
            }
            else
            {
                foreach (var piece in BlackPieces.Where(piece => !piece.Equals(pickedPiece)))
                {
                    piece.GetComponent<XRGrabInteractable>().enabled = false;
                }
            }
            
        }

        public void DisableOrEnablePickUpOnPieces(List<GameObject> pieces)
        {
            foreach (var piece in pieces)
            {
                piece.GetComponent<XRGrabInteractable>().enabled 
                    = !piece.GetComponent<XRGrabInteractable>().enabled;
            }
        }
    
        private void EnablePickUpOnPieces(Shared.TeamType team)
        {
            var teamToBeEnabled = Shared.TeamType.White.Equals(team) ? WhitePieces : BlackPieces;
            foreach (var piece in teamToBeEnabled)
            { 
                piece.GetComponent<XRGrabInteractable>().enabled = true;
            }
        }

        private void EliminatePiece(ChessPiece enemyPiece)
        {
            Vector3 eliminationPosition;
            if(enemyPiece.team == Shared.TeamType.White)
            {
                WhitePieces.Remove(enemyPiece.gameObject);
                eliminationPosition = FreeWhiteEliminationPosition.First.Value;
                UsedWhiteEliminationPosition.AddFirst(eliminationPosition);
                FreeWhiteEliminationPosition.RemoveFirst();
            }
            else
            {
                BlackPieces.Remove(enemyPiece.gameObject);
                eliminationPosition = FreeBlackEliminationPosition.First.Value;
                UsedBlackEliminationPosition.AddFirst(eliminationPosition);
                FreeBlackEliminationPosition.RemoveFirst();
            }

            ChessPieces[enemyPiece.currentX, enemyPiece.currentY] = null;
            enemyPiece.transform.position = eliminationPosition;
            enemyPiece.GetComponent<XRGrabInteractable>().enabled = false;
        }
        
        public Shared.TileOccupiedBy CalculateSpaceOccupation(Vector2Int position, Shared.TeamType selectedPieceTeam)
        {
            ChessPiece chessPiece;
            try
            {
                chessPiece = ChessPieces[position.x, position.y];
            }
            catch
            {
                return Shared.TileOccupiedBy.EndOfTable;
            }

            if (chessPiece == null)
            {
                return Shared.TileOccupiedBy.None;
            }

            return selectedPieceTeam.Equals(chessPiece.team) ? Shared.TileOccupiedBy.FriendlyPiece : Shared.TileOccupiedBy.EnemyPiece;
        }

        public void GenerateAllMoves()
        {
            // Reset the Tiles Old AttackedBy status and the Attacking Pieces
            TileManager.ResetTileAttackedStatus();

            // Calculate all the move positions for the White Pieces and populate the Attacking Pieces/Status of the Tiles
            foreach (var chessPiece in WhitePieces.Select(pieceGameObject => pieceGameObject.GetComponent<ChessPiece>()))
            {
                chessPiece.CalculateAvailablePositions();
                
                var attackedTiles = 
                    chessPiece.Moves.AttackMoves.Select(attackMove => TileManager.GetTile(attackMove));
                foreach (var attackedTile in attackedTiles)
                {
                    attackedTile.AttackedBy = attackedTile.AttackedBy switch
                    {
                        Shared.AttackedBy.None => Shared.AttackedBy.White,
                        Shared.AttackedBy.Black => Shared.AttackedBy.Both,
                        _ => attackedTile.AttackedBy
                    };
                    attackedTile.WhiteAttackingPieces.Add(chessPiece);
                }
            }

            // Calculate all the move positions for the Black Pieces and populate the Attacking Pieces/Status of the Tiles
            foreach (var chessPiece in BlackPieces.Select(pieceGameObject => pieceGameObject.GetComponent<ChessPiece>()))
            {
                chessPiece.GetComponent<ChessPiece>().CalculateAvailablePositions();

                var attackedTiles = 
                    chessPiece.Moves.AttackMoves.Select(attackMove => TileManager.GetTile(attackMove));
                foreach (var attackedTile in attackedTiles)
                {
                    attackedTile.AttackedBy = attackedTile.AttackedBy switch
                    {
                        Shared.AttackedBy.None => Shared.AttackedBy.Black,
                        Shared.AttackedBy.White => Shared.AttackedBy.Both,
                        _ => attackedTile.AttackedBy
                    };
                    attackedTile.BlackAttackingPieces.Add(chessPiece);
                }
            }
        }

        private void EliminateInvalidMoves(bool isCurrentTeamWhite)
        {
            var enemyPieces =
                isCurrentTeamWhite
                    ? BlackPieces.Select(go => go.GetComponent<ChessPiece>())
                    : WhitePieces.Select(go => go.GetComponent<ChessPiece>());

            var friendlyPieces =
                isCurrentTeamWhite
                    ? WhitePieces.Select(go => go.GetComponent<ChessPiece>())
                    : BlackPieces.Select(go => go.GetComponent<ChessPiece>());
           
            var protectedKing = isCurrentTeamWhite ? whiteKing : blackKing;
            var ignoredAttacks = isCurrentTeamWhite ? Shared.AttackedBy.White : Shared.AttackedBy.Black;

            foreach (var fPiece in friendlyPieces)
            {
                var currentPieceTile = TileManager.GetTile(fPiece.currentX, fPiece.currentY);
                if (currentPieceTile.AttackedBy == Shared.AttackedBy.None) continue;
                if (currentPieceTile.AttackedBy == ignoredAttacks) continue;
                
                var allMoves = fPiece.GetAllPossibleMoves();
                foreach (var move in allMoves)
                {
                    
                }
            }
            
        }
    }
}
