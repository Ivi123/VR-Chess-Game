using System;
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
        public HumanPlayer HumanPlayer { get; private set; }
        public AIPlayer AIPlayer { get; private set; }
        public Player CurrentPlayer { get; set; }
        
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
            botPlayer.MovementManager = movementManager;
            botPlayer.TileManager = tileManager;
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
                
                movementManager.GenerateAllMoves();
                movementManager.EvaluateKingStatus();
                movementManager.EliminateInvalidMoves(IsWhiteTurn);
                GameStatus = EvaluateGameStatus();
            }
            
            if (GameStatus is Shared.GameStatus.Victory or Shared.GameStatus.Defeat or Shared.GameStatus.Draw)
            {
                CurrentPlayer = null;
                endGameHandler.DisplayEndGameCanvas(GameStatus);
                return;
            }

            if (CurrentPlayer != AIPlayer) return;
            MakeBotTurn();
            CurrentPlayer.HasMoved = true;
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
            
            movementManager.GenerateAllMoves();
            movementManager.EliminateInvalidMoves(IsWhiteTurn);
            
            AIPlayer.DisablePieces();
            CurrentPlayer.IsMyTurn = true;
            
            HumanPlayer.InitPieces();
            AIPlayer.InitPieces();

            if (!AIPlayer.IsMyTurn) return;
            MakeBotTurn();
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

        private Shared.GameStatus EvaluateGameStatus()
        {
            var currentKing = (King)(IsWhiteTurn ? movementManager.WhiteKing : movementManager.BlackKing);
            var currentTeam = IsWhiteTurn ? Shared.TeamType.White : Shared.TeamType.Black;

            switch (currentKing.isChecked)
            {
                case true when !movementManager.TeamHasPossibleMoves:
                    return currentTeam == HumanPlayer.Team ? Shared.GameStatus.Defeat : Shared.GameStatus.Victory;
                case false when !movementManager.TeamHasPossibleMoves:
                    return Shared.GameStatus.Draw;
            }

            var drawConfigurations = new List<List<ChessPiece>>();
            drawConfigurations.Add(new List<ChessPiece> {new King()});
            drawConfigurations.Add(new List<ChessPiece> {new King(), new Bishop()});
            drawConfigurations.Add(new List<ChessPiece> {new King(), new Knight()});

            if (drawConfigurations.Contains(AIPlayer.Pieces)
                && drawConfigurations.Contains(HumanPlayer.Pieces))
                return Shared.GameStatus.Draw;

            return Shared.GameStatus.Continue;
        }

        private void MakeBotTurn()
        {
            var botTurn =
                botPlayer.BotMakeMove(AIPlayer.Pieces);
            AdvanceTurn(botTurn);
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
    }
}
