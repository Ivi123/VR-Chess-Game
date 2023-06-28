using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChessLogic;
using ChessPieces;
using Players;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        // Mock Start Game
        public bool mockTeamSelection;
        public Shared.TeamType mockTeamType;

        // Properties
        public Chessboard chessboard;
        public MovementManager movementManager;
        public TileManager tileManager;
        public GameObject xrOrigin;
        public BotPlayer botPlayer;
        public EndGameHandler endGameHandler;
        public bool IsWhiteTurn { get; private set; }

        // Game History
        public List<Turn> History { get; set; }
        public Turn LastTurn { get; set; }
        public Shared.GameStatus GameStatus { get; set; }

        // Player Info and team selection
        public List<GameObject> teamSelectors;
        
        // Refactor
        private HumanPlayer HumanPlayer { get; set; }
        private AIPlayer AIPlayer { get; set; }
        private Player CurrentPlayer { get; set; }
        
        //--------------------------------------- Methods ----------------------------------------------------------
        private void Awake()
        {
            HumanPlayer = new HumanPlayer();
            AIPlayer = new AIPlayer();
            CurrentPlayer = null;
            
            History = new List<Turn>();
            LastTurn = null;

            movementManager.TileManager = tileManager;
            chessboard.MovementManager = movementManager;
            chessboard.TileManager = tileManager;
            movementManager.GameManager = this;
            GameStatus = Shared.GameStatus.NotStarted;

            if (mockTeamSelection)
            {
                SelectTeam(mockTeamType, transform.position);
            }
        }

        private void Update()
        {
            if(CurrentPlayer == null) return;
            
            if (CurrentPlayer.HasMoved)
            {
                CurrentPlayer.Update();
                CurrentPlayer = CurrentPlayer == AIPlayer ? HumanPlayer : AIPlayer;
                CurrentPlayer.IsMyTurn = true;
                if (CurrentPlayer == HumanPlayer) HumanPlayer.EnablePieces();
                
                movementManager.GenerateAllMoves(movementManager.ChessPieces, tileManager.Tiles, HumanPlayer.Pieces, AIPlayer.Pieces);
                movementManager.EvaluateKingStatus(tileManager.Tiles,
                    (King)CurrentPlayer.Pieces.First(p => p.type == ChessPieceType.King));
                movementManager.EliminateInvalidMoves(movementManager.ChessPieces, tileManager.Tiles,
                    CurrentPlayer.Team);
                GameStatus = EvaluateGameStatus(CurrentPlayer.Team);

                if (CurrentPlayer == AIPlayer)
                    StartCoroutine(AITurn());
            }

            if (GameStatus is not (Shared.GameStatus.Victory or Shared.GameStatus.Defeat or Shared.GameStatus.Draw))
                return;
            CurrentPlayer = null;
            endGameHandler.DisplayEndGameCanvas(GameStatus);
        }

        private void StartGame()
        {
            chessboard.StartGame();
            IsWhiteTurn = true;
            
            GameStatus = Shared.GameStatus.Continue;

            if (HumanPlayer.Team == Shared.TeamType.White)
            {
                HumanPlayer.Pieces = movementManager.WhitePieces.Select(piece => piece.GetComponent<ChessPiece>())
                    .ToList();
                AIPlayer.Pieces = movementManager.BlackPieces.Select(piece => piece.GetComponent<ChessPiece>())
                    .ToList();
                CurrentPlayer = HumanPlayer;
                HumanPlayer.EnablePieces();
            }
            else
            {
                HumanPlayer.Pieces = movementManager.BlackPieces.Select(piece => piece.GetComponent<ChessPiece>())
                    .ToList();
                AIPlayer.Pieces = movementManager.WhitePieces.Select(piece => piece.GetComponent<ChessPiece>())
                    .ToList();
                CurrentPlayer = AIPlayer;
                HumanPlayer.DisablePieces();
            }
            
            movementManager.GenerateAllMoves(movementManager.ChessPieces, tileManager.Tiles, HumanPlayer.Pieces, AIPlayer.Pieces);
            movementManager.EliminateInvalidMoves(movementManager.ChessPieces, tileManager.Tiles, CurrentPlayer.Team);
            
            AIPlayer.DisablePieces();
            CurrentPlayer.IsMyTurn = true;
            
            HumanPlayer.InitPieces();
            AIPlayer.InitPieces();

            if (!AIPlayer.IsMyTurn) return;
            StartCoroutine(AITurn());
            CurrentPlayer.HasMoved = true;
        }

        public void SelectTeam(Shared.TeamType selectedTeam, Vector3 selectorPosition)
        {
            HumanPlayer.Team = selectedTeam;
            AIPlayer.Team = HumanPlayer.Team == Shared.TeamType.White ? Shared.TeamType.Black : Shared.TeamType.White;
            
            StartGame();
            SetPlayer(selectorPosition);

            foreach (var teamSelector in teamSelectors)
            {
                Destroy(teamSelector);
            }
        }

        public void AdvanceTurn(Turn currentTurn)
        {
            DisableEnPassantTargetOnLastTurnPiece();
            History.Add(currentTurn);
            LastTurn = currentTurn;
            SwitchTurn();
            
            tileManager.UpdateTileMaterialAfterMove(LastTurn.PiecesMovedInThisTurn.Pieces[^1]);
        }

        private Shared.GameStatus EvaluateGameStatus(Shared.TeamType evaluatedTeam)
        {
            var currentKing = (King)(evaluatedTeam == Shared.TeamType.White ? movementManager.WhiteKing : movementManager.BlackKing);

            switch (currentKing.isChecked)
            {
                case true when !movementManager.TeamHasPossibleMoves:
                    return evaluatedTeam == HumanPlayer.Team ? Shared.GameStatus.Defeat : Shared.GameStatus.Victory;
                case false when !movementManager.TeamHasPossibleMoves:
                    return Shared.GameStatus.Draw;
            }

            var drawConfigurations = new List<List<ChessPiece>>();
            /*drawConfigurations.Add(new List<ChessPiece> {new King()});
            drawConfigurations.Add(new List<ChessPiece> {new King(), new Bishop()});
            drawConfigurations.Add(new List<ChessPiece> {new King(), new Knight()});*/

            if (drawConfigurations.Contains(AIPlayer.Pieces)
                && drawConfigurations.Contains(HumanPlayer.Pieces))
                return Shared.GameStatus.Draw;

            return Shared.GameStatus.Continue;
        }

        private IEnumerator AITurn() 
        {
            const int depthSearch = 3;
            // Calculate best move for bot
            ChessPiece chessPieceToMove = null;
            var moveToMake = new Move();
            var bestScore = int.MinValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            var moveDict = CopyAllMoves(AIPlayer.Pieces);
            foreach (var piece in AIPlayer.Pieces)
            {
                var copyPieceMoves = moveDict[piece];
                foreach (var move in copyPieceMoves)
                {
                    yield return null;
                    var simulatedTurn = movementManager.MakeMove(movementManager.ChessPieces, piece, tileManager.GetTile( move.Coords), true);

                    // We make a deep copy of the board to not re-calculate the moves for each simulation
                    // Each MiniMax instance will have its own board to play around with
                    var score = MiniMax(chessboard.DeepCopyBoard(movementManager.ChessPieces),
                        chessboard.DeepCopyTiles(), depthSearch - 1, HumanPlayer, false, alpha, beta);
                    
                    
                    movementManager.UndoMove(movementManager.ChessPieces, simulatedTurn.PiecesMovedInThisTurn, true);
                    
                    if (score <= bestScore) continue;
                    
                    chessPieceToMove = piece;
                    moveToMake = move;
                    bestScore = score;
                    
                    alpha = Math.Max(alpha, bestScore);
                    
                    if (alpha >= beta)
                        break;
                }
            }

            var turn = movementManager.MakeMove(movementManager.ChessPieces, chessPieceToMove,
                tileManager.Tiles[moveToMake.Coords.x, moveToMake.Coords.y], false);
            AIPlayer.HasMoved = true;
            AdvanceTurn(turn);
        }

        private static Dictionary<ChessPiece, List<Move>> CopyAllMoves(List<ChessPiece> pieces)
        {
            return pieces.ToDictionary(playerPiece => playerPiece, playerPiece => Move.DeepCopy(playerPiece.Moves));
        }

        private int MiniMax(ChessPiece[,] board, Tile[,] tiles, int depth, Player player, bool maximizing, int alpha, int beta)
        {
            if (depth == 0 || EvaluateGameStatus(player.Team) is Shared.GameStatus.Defeat or Shared.GameStatus.Victory
                    or Shared.GameStatus.Draw)
            {
                var score = EvaluateBoardScore(board, player);
                CleanUpSearchBoard(board, tiles);
                return score;
            }
            
            // Correctly break the copied board into white and black chessPieces
            
            var chessBoard = movementManager.GetChessPieceListFromArray(board);
            var whitePieces = chessBoard.FindAll(cp => cp.team == Shared.TeamType.White).ToList();
            var blackPieces = chessBoard.FindAll(cp => cp.team == Shared.TeamType.Black).ToList();
            RegenerateMovesForPlayer(board, tiles, whitePieces, blackPieces, player);

            // Calculate each piece moves
            var playerPieces = player.Team == Shared.TeamType.White ? whitePieces : blackPieces;
            var moveDict = CopyAllMoves(playerPieces);
            
            int bestEval;
            if (maximizing)
            {
                bestEval = int.MinValue;
                foreach (var piece in playerPieces)
                {
                    var copyPieceMoves = moveDict[piece];
                    foreach (var move in copyPieceMoves)
                    {
                        // Make Move
                        var simulatedTurn =
                            movementManager.MakeMove(board, piece, tiles[move.Coords.x, move.Coords.y], true);

                        var score = MiniMax(chessboard.DeepCopyBoard(board), chessboard.DeepCopyTiles(), depth - 1,
                            HumanPlayer, false, alpha, beta);

                        movementManager.UndoMove(board, simulatedTurn.PiecesMovedInThisTurn, true);
                        
                        bestEval = Math.Max(bestEval, score);
                        alpha = Math.Max(alpha, bestEval);

                        if (alpha >= beta)
                            break;
                    }
                }
            }
            else
            {
                bestEval = int.MaxValue;
                foreach (var piece in playerPieces)
                {
                    var copyPieceMoves = moveDict[piece];
                    foreach (var move in copyPieceMoves)
                    {
                        // Simulate move
                        var simulatedTurn =
                            movementManager.MakeMove(board, piece, tiles[move.Coords.x, move.Coords.y], true);
                        
                        // Calculate score
                        var score = MiniMax(chessboard.DeepCopyBoard(board), chessboard.DeepCopyTiles(), depth - 1,
                            AIPlayer, true, alpha, beta);
                        
                        // Undo simulated movement
                        movementManager.UndoMove(board, simulatedTurn.PiecesMovedInThisTurn, true);

                        bestEval = Math.Min(bestEval, score);
                        beta = Math.Min(beta, bestEval);

                        if (alpha >= beta)
                            break;
                    }
                }
                
            }

            CleanUpSearchBoard(board, tiles);
            return bestEval;
        }

        private int EvaluateBoardScore(ChessPiece[,] board, Player evaluatedPlayer)
        {
            var score = 0;
            foreach (var piece in board)
            {
                if (piece == null) continue;

                var scoreSign = piece.team == evaluatedPlayer.Team ? 1 : -1;
                score += piece.pieceScore * scoreSign;
            }
            
            return score;
        }
        
        public void SwitchTurn()
        {
            IsWhiteTurn = !IsWhiteTurn;
        }

        private void DisableEnPassantTargetOnLastTurnPiece()
        {
            if (LastTurn != null && LastTurn.PiecesMovedInThisTurn.Pieces[^1].type == ChessPieceType.Pawn)
            {
                ((Pawn)LastTurn.PiecesMovedInThisTurn.Pieces[^1]).IsEnPassantTarget = false;
            }
        }

        private void SetPlayer(Vector3 position)
        {
            var direction = Shared.TeamType.White == HumanPlayer.Team ? 1 : -1;
            xrOrigin.transform.position = new Vector3(position.x + (0.25f * direction), position.y - 2.5f,
                position.z);
            xrOrigin.GetComponent<ActionBasedContinuousMoveProvider>().enabled = false;
        }

        public bool IsChessPieceInHistory(ChessPiece chessPiece)
        {
            foreach (var turn in History)
            {
                var pieces = turn.PiecesMovedInThisTurn.Pieces;
                foreach (var piece in pieces)
                {
                    if (piece == chessPiece) return true;
                }
            }

            return false;
        }

        private void RegenerateMovesForPlayer(ChessPiece[,] board, Tile[,] tiles, List<ChessPiece> whitePieces, List<ChessPiece> blackPieces, Player player)
        {
            var king = (King)(player.Team == Shared.TeamType.White
                ? whitePieces.First(cp => cp.type == ChessPieceType.King)
                : blackPieces.First(cp => cp.type == ChessPieceType.King));
            
            movementManager.GenerateAllMoves(board, tiles, whitePieces, blackPieces);
            movementManager.EvaluateKingStatus(tiles, king);
            movementManager.EliminateInvalidMoves(board, tiles, player.Team);
        }

        private static void CleanUpSearchBoard(ChessPiece[,] board, Tile[,] tiles)
        {
            DestroyBoard(board);
            DestroyTileSet(tiles);
        }
        private static void DestroyBoard(ChessPiece[,] boardToDestroy)
        {
            foreach (var chessPiece in boardToDestroy)
            {
                if(chessPiece == null) continue;
                
                Destroy(chessPiece.gameObject);
            }
        }
        private static void DestroyTileSet(Tile[,] tilesToDestroy)
        {
            foreach (var tile in tilesToDestroy)
                Destroy(tile.gameObject);
        }
    }
}
