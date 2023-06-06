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
            var turn = MakeMove(chessPiece, newTile, false);
            if(turn != null) GameManager.AdvanceTurn(turn);

            // Change back the Available/Attack/Special Tiles material back to the default value 
            TileManager.UpdateTileMaterialAfterMove(chessPiece);
            if(turn == null) return;
            GenerateAllMoves();
            EvaluateKingStatus();
            EliminateInvalidMoves(GameManager.IsWhiteTurn);
        }

        private Turn MakeMove(ChessPiece chessPiece, Tile newTile, bool isSimulation)
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
                EliminatePiece(enemyPiece, isSimulation);
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
                        EliminatePiece(enemyPiece, isSimulation);
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
            ChessPieces[newPosition.x, newPosition.y] = chessPiece;
            ChessPieces[currentPosition.x, currentPosition.y] = null;
            chessPiece.currentX = newPosition.x;
            chessPiece.currentY = newPosition.y;

            if (chessPiece is Pawn pawn && Mathf.Abs(newPosition.x - currentPosition.x) == 2)
            {
                pawn.IsEnPassantTarget = true;
            }

            if(!isSimulation) chessPiece.transform.position = TileManager.GetTileCenter(newPosition.x, newPosition.y);
            chessPiece.SavePosition();
            
            chessPiece.IsMoved = true;

            return turn;
        }

        private void EliminatePiece(ChessPiece enemyPiece, bool isSimulation)
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
            enemyPiece.currentX = -1;
            enemyPiece.currentY = -1;
            if(!isSimulation) EliminatePieceFromBoard(enemyPiece, eliminationPosition);
        }

        private void EliminatePieceFromBoard(ChessPiece enemyPiece, Vector3 eliminationPosition)
        {
            enemyPiece.transform.position = eliminationPosition;
            enemyPiece.GetComponent<XRGrabInteractable>().enabled = false;
        }

        public void UndoLastMove()
        {
            var turnToUndo = GameManager.LastTurn;
            if(turnToUndo == null) return;
            var movesToUndo = turnToUndo.PiecesMovedInThisTurn;

            UndoMove(movesToUndo, false);
            UndoMoveOnBoard(movesToUndo);
            
            GameManager.SwitchTurn();
            
            GenerateAllMoves();
            EvaluateKingStatus();
            EliminateInvalidMoves(GameManager.IsWhiteTurn);
            
            GameManager.History.Remove(turnToUndo);
            GameManager.LastTurn = GameManager.History.Count == 0 ? null : GameManager.History[^1];
        }

        private void UndoMove(MovedPieces movesToUndo, bool isSimulation)
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
                DetermineEnPassantStatus(chessPieceToUndo, isSimulation);
            }
        }

        private void DetermineEnPassantStatus(ChessPiece chessPieceToUndo, bool isSimulation)
        {
            if (chessPieceToUndo is Pawn pawn)
            {
                if (Mathf.Abs(pawn.startingPosition.x - pawn.currentX) == 2 && !GameManager.IsChessPieceInHistory(chessPieceToUndo))
                {
                    pawn.IsEnPassantTarget = true;
                } else if (pawn.startingPosition ==
                           new Vector2Int(chessPieceToUndo.currentX, chessPieceToUndo.currentY))
                {
                    pawn.IsEnPassantTarget = false;
                }
            }
            
            if(isSimulation) return;
            
            MovedPieces piecesMovedTwoTurnsAgo;
            try
            {
                piecesMovedTwoTurnsAgo = GameManager.History[^2].PiecesMovedInThisTurn;
            }
            catch (Exception e)
            {
                return;
            }

            foreach (var piece in piecesMovedTwoTurnsAgo.Pieces)
            {
                if (piece is Pawn pawnTwoTurnsAgo &&
                    Mathf.Abs(pawnTwoTurnsAgo.currentX - pawnTwoTurnsAgo.startingPosition.x) == 2)
                {
                    pawnTwoTurnsAgo.IsEnPassantTarget = true;
                }
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
                    chessPiece.Moves.AttackMoves.Select(attackMove => TileManager.GetTile(attackMove)).ToList();
                attackedTiles.AddRange(chessPiece.Moves.AvailableMoves
                    .Select(avMove => TileManager.GetTile(avMove)).ToList());
                attackedTiles.AddRange(chessPiece.Moves.SpecialMoves
                    .Where(sm => sm.MoveType == Shared.MoveType.EnPassant)
                    .Select(spMove => TileManager.GetTile(spMove.Coords)));
                
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
                
                if (chessPiece is Pawn pawn)
                {
                    pawn.Moves.AttackMoves = 
                        pawn.Moves.AttackMoves.Where(atMove => 
                                CalculateSpaceOccupation(atMove, pawn.team) == Shared.TileOccupiedBy.EnemyPiece)
                            .ToList();
                }
            }

            // Calculate all the move positions for the Black Pieces and populate the Attacking Pieces/Status of the Tiles
            foreach (var chessPiece in BlackPieces.Select(pieceGameObject => pieceGameObject.GetComponent<ChessPiece>()))
            {
                chessPiece.GetComponent<ChessPiece>().CalculateAvailablePositions();

                var attackedTiles = 
                    chessPiece.Moves.AttackMoves.Select(attackMove => TileManager.GetTile(attackMove)).ToList();
                attackedTiles.AddRange(chessPiece.Moves.AvailableMoves
                    .Select(avMove => TileManager.GetTile(avMove)).ToList());
                attackedTiles.AddRange(chessPiece.Moves.SpecialMoves
                    .Where(sm => sm.MoveType == Shared.MoveType.EnPassant)
                    .Select(spMove => TileManager.GetTile(spMove.Coords)));
                
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

                if (chessPiece is Pawn pawn)
                {
                    pawn.Moves.AttackMoves = 
                        pawn.Moves.AttackMoves.Where(atMove => 
                            CalculateSpaceOccupation(atMove, pawn.team) == Shared.TileOccupiedBy.EnemyPiece)
                            .ToList();
                }
                
            }
        }

        private void EliminateInvalidMoves(bool isCurrentTeamWhite)
        {
            var friendlyPieces =
                isCurrentTeamWhite
                    ? WhitePieces.Select(go => go.GetComponent<ChessPiece>())
                    : BlackPieces.Select(go => go.GetComponent<ChessPiece>());

            var protectedKing = isCurrentTeamWhite ? whiteKing : blackKing;
            var ignoredAttacks = isCurrentTeamWhite ? Shared.AttackedBy.White : Shared.AttackedBy.Black;

            foreach (var fPiece in friendlyPieces)
            {
                var currentPieceTile = ((King)protectedKing).isChecked
                    ? TileManager.GetTile(new Vector2Int(protectedKing.currentX, protectedKing.currentY))
                    : TileManager.GetTile(fPiece.currentX, fPiece.currentY);
                if (currentPieceTile.AttackedBy == Shared.AttackedBy.None && fPiece != protectedKing) continue;
                if (currentPieceTile.AttackedBy == ignoredAttacks && fPiece != protectedKing) continue;
                
                var piecesAttackingTheTile = isCurrentTeamWhite
                    ? currentPieceTile.BlackAttackingPieces
                    : currentPieceTile.WhiteAttackingPieces;

                var attackMovesToBeRemoved = new List<Vector2Int>();
                foreach (var move in fPiece.Moves.AttackMoves)
                {
                    // Simulate Move
                    var moveToTile = TileManager.GetTile(move);
                    moveToTile.IsAttackTile = true;
                    var simulatedTurn = MakeMove(fPiece, TileManager.GetTile(move), true);
                    
                    if (fPiece == protectedKing)
                        piecesAttackingTheTile = isCurrentTeamWhite
                            ? moveToTile.BlackAttackingPieces
                            : moveToTile.WhiteAttackingPieces;
                    
                    // Calculate protected king check status
                    var markMoveForExclusion = false;
                    foreach (var attackingPiece in piecesAttackingTheTile)
                    {
                        var moves = attackingPiece.CalculateAvailablePositionsWithoutUpdating();
                        var attackMoves = moves.AttackMoves;
                        var attackSpecialMoves =
                            moves.SpecialMoves
                                .FindAll(sMove => sMove.MoveType == Shared.MoveType.EnPassant)
                                .Select(sMove => sMove.Coords);

                        if (!attackMoves.Contains(new Vector2Int(protectedKing.currentX, protectedKing.currentY)) &&
                            !attackSpecialMoves.Contains(new Vector2Int(protectedKing.currentX,
                                protectedKing.currentY)))
                            continue;
                        
                        markMoveForExclusion = true;
                        break;
                    }

                    if (markMoveForExclusion) attackMovesToBeRemoved.Add(move);

                        // Undo simulated Move
                    UndoMove(simulatedTurn.PiecesMovedInThisTurn, true);
                    moveToTile.IsAttackTile = false;
                }
                foreach (var removedMove in attackMovesToBeRemoved)
                    fPiece.Moves.AttackMoves.Remove(removedMove);
                

                var availableMovesToBeRemoved = new List<Vector2Int>();
                foreach (var move in fPiece.Moves.AvailableMoves)
                {
                    var moveToTile = TileManager.GetTile(move); 
                    moveToTile.IsAvailableTile = true;
                    var simulatedTurn = MakeMove(fPiece, TileManager.GetTile(move), true);

                    if (fPiece == protectedKing)
                        piecesAttackingTheTile = isCurrentTeamWhite
                            ? moveToTile.BlackAttackingPieces
                            : moveToTile.WhiteAttackingPieces;

                    var markMoveForExclusion = false;
                    foreach (var attackingPiece in piecesAttackingTheTile)
                    {
                        var moves = attackingPiece.CalculateAvailablePositionsWithoutUpdating();
                        var attackMoves = moves.AttackMoves;
                        var attackSpecialMoves =
                            moves.SpecialMoves
                                .FindAll(sMove => sMove.MoveType == Shared.MoveType.EnPassant)
                                .Select(sMove => sMove.Coords);

                        if (!attackMoves.Contains(new Vector2Int(protectedKing.currentX, protectedKing.currentY)) &&
                            !attackSpecialMoves.Contains(new Vector2Int(protectedKing.currentX, protectedKing.currentY)))
                            continue;
                        
                        markMoveForExclusion = true;
                        break;
                    }

                    if (markMoveForExclusion) availableMovesToBeRemoved.Add(move);
                    
                    UndoMove(simulatedTurn.PiecesMovedInThisTurn, true);
                    moveToTile.IsAvailableTile = false;
                }

                foreach (var removedMove in availableMovesToBeRemoved)
                    fPiece.Moves.AvailableMoves.Remove(removedMove);

                var specialMovesToBeRemoved = new List<SpecialMove>();
                foreach (var move in fPiece.Moves.SpecialMoves)
                {
                    var moveToTile = TileManager.GetTile(move.Coords); 
                    moveToTile.IsSpecialTile = true;
                    moveToTile.IsAttackTile = move.MoveType == Shared.MoveType.EnPassant;
                    var simulatedTurn = MakeMove(fPiece, TileManager.GetTile(move.Coords), true);
                    
                    if (fPiece == protectedKing)
                        piecesAttackingTheTile = isCurrentTeamWhite
                            ? moveToTile.BlackAttackingPieces
                            : moveToTile.WhiteAttackingPieces;
                    
                    var markMoveForExclusion = false;
                    foreach (var attackingPiece in piecesAttackingTheTile)
                    {
                        var moves = attackingPiece.CalculateAvailablePositionsWithoutUpdating();
                        var attackMoves = moves.AttackMoves;
                        var attackSpecialMoves =
                            moves.SpecialMoves
                                .FindAll(sMove => sMove.MoveType == Shared.MoveType.EnPassant)
                                .Select(sMove => sMove.Coords);

                        if (!attackMoves.Contains(new Vector2Int(protectedKing.currentX, protectedKing.currentY)) &&
                            !attackSpecialMoves.Contains(new Vector2Int(protectedKing.currentX, protectedKing.currentY)))
                            continue;
                        
                        markMoveForExclusion = true;
                        break;
                    }

                    if (markMoveForExclusion) specialMovesToBeRemoved.Add(move);
                    
                    UndoMove(simulatedTurn.PiecesMovedInThisTurn, true);
                    TileManager.GetTile(move.Coords).IsSpecialTile = false;
                    TileManager.GetTile(move.Coords).IsAttackTile = false;
                }

                foreach (var removedMove in specialMovesToBeRemoved)
                    fPiece.Moves.SpecialMoves.Remove(removedMove);
            }
        }

        private void EvaluateKingStatus()
        {
            var king = GameManager.IsWhiteTurn ? whiteKing : blackKing;
            var ignoredAttacks = GameManager.IsWhiteTurn ? Shared.AttackedBy.White : Shared.AttackedBy.Black;
            var kingCoords = new Vector2Int(king.currentX, king.currentY);
            var kingTile = TileManager.GetTile(kingCoords);

            if (kingTile.AttackedBy == ignoredAttacks || kingTile.AttackedBy == Shared.AttackedBy.None)
            {
                ((King)king).isChecked = false;
                return;
            }
            
            ((King)king).isChecked = true;
        }
    }
}
