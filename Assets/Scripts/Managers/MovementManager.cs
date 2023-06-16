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
        
        // Elimination Related Properties
        public LinkedList<Vector3> FreeWhiteEliminationPosition { get; set; }
        public LinkedList<Vector3> UsedWhiteEliminationPosition { get; set; }
        public LinkedList<Vector3> FreeBlackEliminationPosition { get; set; }
        public LinkedList<Vector3> UsedBlackEliminationPosition { get; set; }

        //Refactor
        public bool TeamHasMoved { get; set; }
        
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
                var tile = TileManager.GetTile(move.Coords);
                tile.IsSpecialTile = true;
                tile.IsAttackTile = move.MoveType == Shared.MoveType.EnPassant;
                tile.IsAvailableTile = move.MoveType is Shared.MoveType.ShortCastle or Shared.MoveType.LongCastle
                    or Shared.MoveType.Promotion;

                TileManager.UpdateTileMaterial(move.Coords,
                    TileManager.IsTileWhite(move.Coords)
                        ? tile.IsAttackTile 
                            ? Shared.TileType.AttackTileWhite 
                            : Shared.TileType.AvailableWhite
                        : tile.IsAttackTile
                            ? Shared.TileType.AttackTileBlack
                            : Shared.TileType.AvailableBlack);
            }

            var pickedPiece = ChessPieces[x, y];
            DisablePickUpOnOtherPieces(pickedPiece.gameObject, pickedPiece.team);
        }
    
        public void PieceWasDropped(int currentX, int currentY, Tile newTile)
        {
            var chessPiece = ChessPieces[currentX, currentY];

            // Re-enable XRGrabInteractable on the current team's pieces
            chessPiece.MyPlayer.DisablePieces();

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
            
            chessPiece.MyPlayer.HasMoved = true;
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
                    //case Shared.MoveType.Promotion:
                    //    throw new NotImplementedException();
                    //    break;
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
                    ? WhitePieces.Select(go => go.GetComponent<ChessPiece>())
                    : BlackPieces.Select(go => go.GetComponent<ChessPiece>());

            var protectedKing = isCurrentTeamWhite ? WhiteKing : BlackKing;
            var ignoredAttacks = isCurrentTeamWhite ? Shared.AttackedBy.White : Shared.AttackedBy.Black;
            var isKingChecked = ((King)protectedKing).isChecked;

            foreach (var fPiece in friendlyPieces.ToList())
            {
                var currentPieceTile = TileManager.GetTile(fPiece.currentX, fPiece.currentY);
                if (currentPieceTile.AttackedBy == Shared.AttackedBy.None && fPiece != protectedKing && !isKingChecked)
                {
                    if (fPiece.Moves.AttackMoves.Count != 0 || fPiece.Moves.AvailableMoves.Count != 0 ||
                        fPiece.Moves.SpecialMoves.Count != 0)
                        TeamHasPossibleMoves = true;
                    continue;
                }
                if (currentPieceTile.AttackedBy == ignoredAttacks && fPiece != protectedKing && !isKingChecked)
                {
                    if (fPiece.Moves.AttackMoves.Count != 0 || fPiece.Moves.AvailableMoves.Count != 0 ||
                        fPiece.Moves.SpecialMoves.Count != 0)
                        TeamHasPossibleMoves = true;
                    continue;
                }
                
                var piecesAttackingTheTile = isCurrentTeamWhite
                    ? currentPieceTile.BlackAttackingPieces
                    : currentPieceTile.WhiteAttackingPieces;

                // Remove invalid attack moves
                var attackMovesToBeRemoved = MovesToBeRemoved(fPiece, piecesAttackingTheTile, protectedKing,
                    fPiece.Moves.AttackMoves, true, false);
                foreach (var removedMove in attackMovesToBeRemoved)
                    fPiece.Moves.AttackMoves.Remove(removedMove);

                // Remove invalid available moves
                var availableMovesToBeRemoved = MovesToBeRemoved(fPiece, piecesAttackingTheTile, protectedKing,
                    fPiece.Moves.AvailableMoves, false, true);
                foreach (var removedMove in availableMovesToBeRemoved)
                    fPiece.Moves.AvailableMoves.Remove(removedMove);

                // Remove special moves
                var specialMovesToBeRemoved = new List<SpecialMove>();
                foreach (var move in fPiece.Moves.SpecialMoves.ToList())
                {
                    var moveToTile = TileManager.GetTile(move.Coords);

                    if (move.MoveType is Shared.MoveType.ShortCastle or Shared.MoveType.LongCastle)
                    {
                        var attackingEnemyTeam = fPiece.team == Shared.TeamType.White
                            ? Shared.AttackedBy.Black
                            : Shared.AttackedBy.White;
                        
                        if (isKingChecked)
                        {
                            specialMovesToBeRemoved.Add(move);
                            continue;
                        }

                        switch (move.MoveType)
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
                                        specialMovesToBeRemoved.Add(move);
                                        break;
                                    }

                                    var currentCheckingTile = TileManager.GetTile(currentCheckingPosition);
                                    if (currentCheckingTile.AttackedBy == attackingEnemyTeam ||
                                         currentCheckingTile.AttackedBy == Shared.AttackedBy.Both)
                                    {
                                        specialMovesToBeRemoved.Add(move);
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
                                        specialMovesToBeRemoved.Add(move);
                                        break;
                                    }

                                    var currentCheckingTile = TileManager.GetTile(currentCheckingPosition);
                                    if (yChecks <= longCastleKingEnd.y &&
                                        (currentCheckingTile.AttackedBy == attackingEnemyTeam ||
                                         currentCheckingTile.AttackedBy == Shared.AttackedBy.Both))
                                    {
                                        specialMovesToBeRemoved.Add(move);
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        var simulatedTurn = MakeMove(fPiece, TileManager.GetTile(move.Coords), true);
                    
                        if (fPiece == protectedKing)
                            piecesAttackingTheTile = isCurrentTeamWhite
                                ? moveToTile.BlackAttackingPieces
                                : moveToTile.WhiteAttackingPieces;
                    
                        var markMoveForExclusion = false;
                        foreach (var attackingPiece in piecesAttackingTheTile.ToList())
                        {
                            var moves = attackingPiece.CalculateAvailablePositionsWithoutUpdating();
                            var attackMoves = moves.AttackMoves;
                            var attackSpecialMoves =
                                moves.SpecialMoves
                                    .FindAll(sMove => sMove.MoveType == Shared.MoveType.EnPassant)
                                    .Select(sMove => sMove.Coords)
                                    .ToList();

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
                }

                foreach (var removedMove in specialMovesToBeRemoved)
                    fPiece.Moves.SpecialMoves.Remove(removedMove);

                if (fPiece.Moves.AttackMoves.Count != 0 || fPiece.Moves.AvailableMoves.Count != 0 ||
                    fPiece.Moves.SpecialMoves.Count != 0)
                    TeamHasPossibleMoves = true;
            }
        }

        private List<Vector2Int> MovesToBeRemoved(ChessPiece fPiece, List<ChessPiece> piecesAttackingTheTile,
            ChessPiece protectedKing, List<Vector2Int> fPieceMoves, bool areAttackMoves, bool areAvailableMoves)
        {
            var movesToBeRemoved = new List<Vector2Int>();
            var kingIsChecked = ((King)protectedKing).isChecked;
            var kingTile = TileManager.GetTile(protectedKing.currentX, protectedKing.currentY);

            foreach (var move in fPieceMoves)
            {
                var moveToTile = TileManager.GetTile(move);
                moveToTile.IsAttackTile = areAttackMoves;
                moveToTile.IsAvailableTile = areAvailableMoves;
                
                var piecesAttackingVitalTiles = new List<ChessPiece>(piecesAttackingTheTile);

                if (kingIsChecked)
                    piecesAttackingVitalTiles.AddRange(fPiece.team == Shared.TeamType.White
                        ? kingTile.BlackAttackingPieces
                        : kingTile.WhiteAttackingPieces);
                
                if (fPiece == protectedKing || kingIsChecked)
                    piecesAttackingVitalTiles.AddRange(fPiece.team == Shared.TeamType.White
                        ? moveToTile.BlackAttackingPieces
                        : moveToTile.WhiteAttackingPieces);
                
                // Simulate Move
                var simulatedTurn = MakeMove(fPiece, TileManager.GetTile(move), true);

                // Calculate protected king check status
                var markMoveForExclusion = false;
                foreach (var attackingPiece in piecesAttackingVitalTiles.ToList())
                {
                    var moves = attackingPiece.CalculateAvailablePositionsWithoutUpdating();
                    var attackMoves = moves.AttackMoves;
                    var attackSpecialMoves =
                        moves.SpecialMoves
                            .FindAll(sMove => sMove.MoveType == Shared.MoveType.EnPassant)
                            .Select(sMove => sMove.Coords)
                            .ToList();

                    if (!attackMoves.Contains(new Vector2Int(protectedKing.currentX, protectedKing.currentY)) &&
                        !attackSpecialMoves.Contains(new Vector2Int(protectedKing.currentX,
                            protectedKing.currentY)))
                        continue;

                    markMoveForExclusion = true;
                    break;
                }

                if (markMoveForExclusion) movesToBeRemoved.Add(move);
                UndoMove(simulatedTurn.PiecesMovedInThisTurn, true);
                moveToTile.IsAttackTile = false;
                moveToTile.IsAvailableTile = false;
            }

            return movesToBeRemoved;
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
            TileManager.UpdateTileMaterial(kingCoords,
                TileManager.GetTile(kingCoords).IsWhiteTile
                    ? Shared.TileType.AttackTileWhite
                    : Shared.TileType.AttackTileBlack);
        }
    }
}
