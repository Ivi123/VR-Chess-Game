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
        public ChessPiece WhiteKing { get; private set; }
        public List<GameObject> BlackPieces { get; set; }
        public ChessPiece BlackKing{ get; private set; }
        public bool TeamHasPossibleMoves { get; private set; }
        public PromotionHandler promotionHandler;
        
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
                WhiteKing = king;
            }
            else
            {
                BlackKing = king;
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

            foreach (var move in chessPiece.Moves)
            {
                var isTileWhite = TileManager.IsTileWhite(move.Coords);
                var tileType = 
                    move.Type 
                        is Shared.MoveType.Attack 
                        or Shared.MoveType.EnPassant 
                        or Shared.MoveType.AttackPromotion 
                        ? isTileWhite 
                            ? Shared.TileType.AttackTileWhite : Shared.TileType.AttackTileBlack 
                        : isTileWhite 
                            ? Shared.TileType.AvailableWhite 
                            : Shared.TileType.AvailableBlack;
                TileManager.UpdateTileMaterial(move.Coords, tileType);
                
                var tile = TileManager.GetTile(move.Coords);
                tile.IsAvailableTile = move.Type is Shared.MoveType.Normal
                    or Shared.MoveType.Promotion or Shared.MoveType.LongCastle or Shared.MoveType.ShortCastle;
                tile.IsAttackTile = move.Type is Shared.MoveType.Attack or Shared.MoveType.EnPassant
                    or Shared.MoveType.AttackPromotion;
                tile.IsSpecialTile = move.Type is Shared.MoveType.EnPassant or Shared.MoveType.Promotion
                    or Shared.MoveType.AttackPromotion or Shared.MoveType.LongCastle or Shared.MoveType.ShortCastle;
            }

            var pickedPiece = ChessPieces[x, y];
            DisablePickUpOnOtherPieces(pickedPiece.gameObject, pickedPiece.team);
        }
    
        public void PieceWasDropped(int currentX, int currentY, Tile newTile)
        {
            var chessPiece = ChessPieces[currentX, currentY];

            // Re-enable XRGrabInteractable on the current team's pieces
            chessPiece.MyPlayer.EnablePieces();

            // Update the currently picked chess piece's tile material from Selected to Default
            TileManager.UpdateTileMaterial(new Vector2Int(currentX, currentY), Shared.TileType.Default);
            
            // Update the ChessPieces matrix with the new format after a chess piece was moved. The method returns
            // the turn that was just made with all the moved pieces and the changes in positions
            var turn = MakeMove(chessPiece, newTile, false);
            if (turn == null)
            {
                TileManager.UpdateTileMaterialAfterMove(chessPiece);
                return;
            }
            
            GameManager.AdvanceTurn(turn);
        }

        public Turn MakeMove(ChessPiece chessPiece, Tile newTile, bool isSimulation)
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

            if(!isSimulation) chessPiece.MyPlayer.HasMoved = true;
            if (newTile.IsSpecialTile)
            {
                var specialMoveType = chessPiece.Moves.First(move => move.Coords == newTile.Position).Type;
                switch (specialMoveType)
                {
                    case Shared.MoveType.EnPassant:
                        var direction = chessPiece.team == Shared.TeamType.White ? -1 : 1;
                        var enemyPiece = ChessPieces[newPosition.x + (direction * 1), newPosition.y];
                        turn.MoveType = Shared.MoveType.EnPassant;
                        movedPieces.AddNewPieceAndPosition(enemyPiece, MovedPieces.EliminationPosition);
                        EliminatePiece(enemyPiece, isSimulation);
                        break;
                    case Shared.MoveType.ShortCastle:
                        var sRookToBeCastledPosition = new Vector2Int(newTile.Position.x, newTile.Position.y - 1);
                        var sRookToBeCastled = ChessPieces[sRookToBeCastledPosition.x, sRookToBeCastledPosition.y];
                        var sRookToBeCastledNewPosition = new Vector2Int(newTile.Position.x, newTile.Position.y + 1);

                        movedPieces.AddNewPieceAndPosition(sRookToBeCastled, sRookToBeCastledNewPosition);
                        ChessPieces[sRookToBeCastledPosition.x, sRookToBeCastledPosition.y] = null;
                        ChessPieces[sRookToBeCastledNewPosition.x, sRookToBeCastledNewPosition.y] = sRookToBeCastled;
                        sRookToBeCastled.currentX = sRookToBeCastledNewPosition.x;
                        sRookToBeCastled.currentY = sRookToBeCastledNewPosition.y;
                        sRookToBeCastled.IsMoved = true;
                        turn.MoveType = Shared.MoveType.ShortCastle;

                        if (!isSimulation)
                            sRookToBeCastled.transform.position =
                                TileManager.GetTileCenter(sRookToBeCastled.currentX, sRookToBeCastled.currentY);
                        sRookToBeCastled.SavePosition();
                        break;
                    case Shared.MoveType.LongCastle:
                        var lRookToBeCastledPosition = new Vector2Int(newTile.Position.x, newTile.Position.y + 2);
                        var lRookToBeCastled = ChessPieces[lRookToBeCastledPosition.x, lRookToBeCastledPosition.y];
                        var lRookToBeCastledNewPosition = new Vector2Int(newTile.Position.x, newTile.Position.y - 1);

                        movedPieces.AddNewPieceAndPosition(lRookToBeCastled, lRookToBeCastledNewPosition);
                        ChessPieces[lRookToBeCastledPosition.x, lRookToBeCastledPosition.y] = null;
                        ChessPieces[lRookToBeCastledNewPosition.x, lRookToBeCastledNewPosition.y] = lRookToBeCastled;
                        lRookToBeCastled.currentX = lRookToBeCastledNewPosition.x;
                        lRookToBeCastled.currentY = lRookToBeCastledNewPosition.y;
                        lRookToBeCastled.IsMoved = true;
                        turn.MoveType = Shared.MoveType.LongCastle;

                        if (!isSimulation)
                            lRookToBeCastled.transform.position =
                                TileManager.GetTileCenter(lRookToBeCastled.currentX, lRookToBeCastled.currentY);
                        lRookToBeCastled.SavePosition();
                        break;
                    case Shared.MoveType.AttackPromotion:
                        var promotionEnemyPiece = ChessPieces[newPosition.x, newPosition.y];
                        movedPieces.AddNewPieceAndPosition(promotionEnemyPiece, MovedPieces.EliminationPosition);
                        EliminatePiece(promotionEnemyPiece, isSimulation);
                        turn.MoveType = Shared.MoveType.AttackPromotion;
                        
                        chessPiece.MyPlayer.HasMoved = false;
                        chessPiece.MyPlayer.DisablePieces();
                        promotionHandler.EnableDisablePromotionCanvas(true, chessPiece);
                        break;
                    case Shared.MoveType.Promotion:
                        chessPiece.MyPlayer.HasMoved = false;
                        
                        turn.MoveType = Shared.MoveType.Promotion;
                        chessPiece.MyPlayer.DisablePieces();
                        promotionHandler.EnableDisablePromotionCanvas(true, chessPiece);
                        break;
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

            if(!isSimulation) enemyPiece.MyPlayer.Pieces.Remove(enemyPiece);
         
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
            
            GameManager.History.Remove(turnToUndo);
            GameManager.LastTurn = GameManager.History.Count == 0 ? null : GameManager.History[^1];
            
            var movesToUndo = turnToUndo.PiecesMovedInThisTurn;
            UndoMove(movesToUndo, false);
            UndoMoveOnBoard(movesToUndo);
            
            GameManager.SwitchTurn();
            
            GenerateAllMoves();
            EvaluateKingStatus();
            EliminateInvalidMoves(GameManager.IsWhiteTurn);
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
                if (chessPieceToUndo.startingPosition == oldPosition &&
                    !GameManager.IsChessPieceInHistory(chessPieceToUndo)) chessPieceToUndo.IsMoved = false;
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
                Debug.Log(e.Message);
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
            foreach (var chessPiece in GameManager.AIPlayer.Pieces)
            {
                chessPiece.CalculateAvailablePositions();
            }

            // Calculate all the move positions for the Black Pieces and populate the Attacking Pieces/Status of the Tiles
            foreach (var chessPiece in GameManager.HumanPlayer.Pieces)
            {
                chessPiece.CalculateAvailablePositions();
            }
            
            TileManager.DetermineAttackStatus();
        }

        public void EliminateInvalidMoves(bool isCurrentTeamWhite)
        {
            TeamHasPossibleMoves = false;
            var friendlyPieces =
                isCurrentTeamWhite
                    ? WhitePieces.Select(go => go.GetComponent<ChessPiece>()).ToList()
                    : BlackPieces.Select(go => go.GetComponent<ChessPiece>()).ToList();
            
            var teamPlayer = friendlyPieces.First().MyPlayer;
            var protectedKing = isCurrentTeamWhite ? WhiteKing : BlackKing;
            var ignoredAttacks = isCurrentTeamWhite ? Shared.AttackedBy.White : Shared.AttackedBy.Black;
            var isKingChecked = ((King)protectedKing).isChecked;
            var kingTile = TileManager.GetTile(protectedKing.currentX, protectedKing.currentY);

            foreach (var fPiece in friendlyPieces)
            {
                var currentPieceTile = TileManager.GetTile(fPiece.currentX, fPiece.currentY);
                if (currentPieceTile.AttackedBy == Shared.AttackedBy.None && fPiece != protectedKing && !isKingChecked)
                {
                    if (fPiece.Moves.Count != 0)
                        TeamHasPossibleMoves = true;
                    continue;
                }
                if (currentPieceTile.AttackedBy == ignoredAttacks && fPiece != protectedKing && !isKingChecked)
                {
                    if (fPiece.Moves.Count != 0)
                        TeamHasPossibleMoves = true;
                    continue;
                }

                var piecesAttackingTheTile = new List<ChessPiece>();
                piecesAttackingTheTile.AddRange(isCurrentTeamWhite
                    ? currentPieceTile.BlackAttackingPieces
                    : currentPieceTile.WhiteAttackingPieces);
                
                // Remove special moves
                var movesToBeRemoved = new List<Move>();
                foreach (var move in fPiece.Moves)
                {
                    var moveToTile = TileManager.GetTile(move.Coords);

                    if (move.Type is Shared.MoveType.ShortCastle or Shared.MoveType.LongCastle)
                    {
                        var attackingEnemyTeam = fPiece.team == Shared.TeamType.White
                            ? Shared.AttackedBy.Black
                            : Shared.AttackedBy.White;
                        
                        if (isKingChecked)
                        {
                            movesToBeRemoved.Add(move);
                            continue;
                        }

                        switch (move.Type)
                        {
                            case Shared.MoveType.ShortCastle:
                                var shortCastleKingStart = fPiece.startingPosition;
                                var shortCastleRookEnd = new Vector2Int(move.Coords.x, move.Coords.y - 1);

                                for (var yChecks = shortCastleKingStart.y - 1; yChecks >= shortCastleRookEnd.y + 1; yChecks--)
                                {
                                    var currentCheckingPosition = new Vector2Int(shortCastleKingStart.x, yChecks);
                                    if (CalculateSpaceOccupation(currentCheckingPosition, fPiece.team) !=
                                        Shared.TileOccupiedBy.None)
                                    {
                                        movesToBeRemoved.Add(move);
                                        break;
                                    }

                                    var currentCheckingTile = TileManager.GetTile(currentCheckingPosition);
                                    if (currentCheckingTile.AttackedBy == attackingEnemyTeam ||
                                         currentCheckingTile.AttackedBy == Shared.AttackedBy.Both)
                                    {
                                        movesToBeRemoved.Add(move);
                                        break;
                                    }
                                }
                                
                                break;
                            case Shared.MoveType.LongCastle:
                                var longCastleKingStart = fPiece.startingPosition;
                                var longCastleKingEnd = move.Coords;
                                var longCastleRookEnd = new Vector2Int(move.Coords.x, move.Coords.y + 2);
                                for (var yChecks = longCastleKingStart.y + 1; yChecks <= longCastleRookEnd.y - 1; yChecks++)
                                {
                                    var currentCheckingPosition = new Vector2Int(longCastleKingStart.x, yChecks);
                                    if (CalculateSpaceOccupation(currentCheckingPosition, fPiece.team) !=
                                        Shared.TileOccupiedBy.None)
                                    {
                                        movesToBeRemoved.Add(move);
                                        break;
                                    }

                                    var currentCheckingTile = TileManager.GetTile(currentCheckingPosition);
                                    if (yChecks <= longCastleKingEnd.y &&
                                        (currentCheckingTile.AttackedBy == attackingEnemyTeam ||
                                         currentCheckingTile.AttackedBy == Shared.AttackedBy.Both))
                                    {
                                        movesToBeRemoved.Add(move);
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        var tile = TileManager.GetTile(move.Coords);
                        tile.IsAvailableTile =
                            move.Type is Shared.MoveType.Normal or Shared.MoveType.ShortCastle
                                or Shared.MoveType.LongCastle;
                        tile.IsAttackTile = move.Type is Shared.MoveType.Attack or Shared.MoveType.EnPassant
                            or Shared.MoveType.AttackPromotion;
                        tile.IsSpecialTile = move.Type is Shared.MoveType.EnPassant or Shared.MoveType.ShortCastle
                            or Shared.MoveType.LongCastle;
                        
                        var simulatedTurn = MakeMove(fPiece, TileManager.GetTile(move.Coords), true);
                    
                        if (isKingChecked)
                            piecesAttackingTheTile.AddRange(fPiece.team == Shared.TeamType.White
                                ? kingTile.BlackAttackingPieces
                                : kingTile.WhiteAttackingPieces);
                
                        if (fPiece == protectedKing || isKingChecked)
                            piecesAttackingTheTile.AddRange(fPiece.team == Shared.TeamType.White
                                ? moveToTile.BlackAttackingPieces
                                : moveToTile.WhiteAttackingPieces);
                    
                        var markMoveForExclusion = false;
                        foreach (var attackingPiece in piecesAttackingTheTile.ToHashSet())
                        {
                            var moves = attackingPiece.CalculateAvailablePositionsWithoutUpdating();
                            var attackMoves = moves
                                .FindAll(aMove => aMove.Type is Shared.MoveType.Attack or Shared.MoveType.EnPassant or Shared.MoveType.AttackPromotion)
                                .Select(aMove => aMove.Coords).ToList();

                            if (!attackMoves.Contains(new Vector2Int(protectedKing.currentX, protectedKing.currentY)))
                                continue;
                        
                            markMoveForExclusion = true;
                            break;
                        }

                        if (markMoveForExclusion) movesToBeRemoved.Add(move);
                    
                        UndoMove(simulatedTurn.PiecesMovedInThisTurn, true);
                        tile.IsSpecialTile = false;
                        tile.IsAttackTile = false;
                        tile.IsAvailableTile = false;
                    }
                }

                foreach (var removedMove in movesToBeRemoved)
                    fPiece.Moves.Remove(removedMove);

                if (fPiece.Moves.Count != 0)
                    TeamHasPossibleMoves = true;

                teamPlayer.AllMoves.AddRange(fPiece.Moves);
            }
        }
        
        public void EvaluateKingStatus()
        {
            var king = GameManager.IsWhiteTurn ? WhiteKing : BlackKing;
            var ignoredAttacks = GameManager.IsWhiteTurn ? Shared.AttackedBy.White : Shared.AttackedBy.Black;
            var kingCoords = new Vector2Int(king.currentX, king.currentY);
            var kingTile = TileManager.GetTile(kingCoords);

            if (kingTile.AttackedBy == ignoredAttacks || kingTile.AttackedBy == Shared.AttackedBy.None)
            {
                ((King)king).isChecked = false;
                return;
            }
            
            ((King)king).isChecked = true;
            /*TileManager.UpdateTileMaterial(kingCoords,
                TileManager.GetTile(kingCoords).IsWhiteTile
                    ? Shared.TileType.AttackTileWhite
                    : Shared.TileType.AttackTileBlack);*/
        }
    }
}
