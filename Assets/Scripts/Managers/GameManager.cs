using System;
using System.Collections.Generic;
using System.Linq;
using ChessLogic;
using ChessPieces;
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
        public bool IsPlayerTurn { get; set; }

        // Game History
        public List<Turn> History { get; set; }
        public Turn LastTurn { get; set; }

        public Shared.GameStatus GameStatus { get; set; }

        // Player Info and team selection
        private Shared.TeamType playersTeam;
        private List<GameObject> playersPieces;
        private List<GameObject> botPieces;
        private Vector3 playingPosition;
        public List<GameObject> teamSelectors;


        //--------------------------------------- Methods ----------------------------------------------------------
        private void Awake()
        {
            History = new List<Turn>();
            LastTurn = null;

            movementManager.TileManager = tileManager;
            chessboard.MovementManager = movementManager;
            chessboard.TileManager = tileManager;
            movementManager.GameManager = this;
            botPlayer.MovementManager = movementManager;
            botPlayer.TileManager = tileManager;

            if (mockTeamSelection)
            {
                SelectTeam(mockTeamType, transform.position);
            }
        }
        
        private void StartGame()
        {
            chessboard.StartGame();
            movementManager.GenerateAllMoves();
            IsWhiteTurn = true;
            movementManager.EliminateInvalidMoves(IsWhiteTurn);
            movementManager.DisableOrEnablePickUpOnPieces(movementManager.BlackPieces);
            
            playersPieces = playersTeam == Shared.TeamType.White
                ? movementManager.WhitePieces
                : movementManager.BlackPieces;
            botPieces = playersTeam == Shared.TeamType.White
                ? movementManager.BlackPieces
                : movementManager.WhitePieces;
            IsPlayerTurn = playersTeam == Shared.TeamType.White;

            if (IsPlayerTurn) return;
            MakeBotTurn();
        }

        public void SelectTeam(Shared.TeamType selectedTeam, Vector3 selectorPosition)
        {
            playersTeam = selectedTeam;
            playingPosition = selectorPosition;

            StartGame();
            SetPlayer();

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

            movementManager.GenerateAllMoves();
            movementManager.EvaluateKingStatus();
            movementManager.EliminateInvalidMoves(IsWhiteTurn);

            GameStatus = EvaluateGameStatus();
            if(GameStatus == Shared.GameStatus.Continue) return;

            // Handle End Of Game
            endGameHandler.DisplayEndGameCanvas(GameStatus);
        }

        private Shared.GameStatus EvaluateGameStatus()
        {
            var currentKing = (King)(IsWhiteTurn ? movementManager.WhiteKing : movementManager.BlackKing);
            var currentTeam = IsWhiteTurn ? Shared.TeamType.White : Shared.TeamType.Black;

            switch (currentKing.isChecked)
            {
                case true when !movementManager.TeamHasMoves:
                    return currentTeam == playersTeam ? Shared.GameStatus.Defeat : Shared.GameStatus.Victory;
                case false when !movementManager.TeamHasMoves:
                    return Shared.GameStatus.Draw;
            }

            var drawConfigurations = new List<List<ChessPiece>>();
            drawConfigurations.Add(new List<ChessPiece> {new King()});
            drawConfigurations.Add(new List<ChessPiece> {new King(), new Bishop()});
            drawConfigurations.Add(new List<ChessPiece> {new King(), new Knight()});

            if (drawConfigurations.Contains(movementManager.BlackPieces
                    .Select(piece => piece.GetComponent<ChessPiece>()).ToList())
                &&
                drawConfigurations.Contains(movementManager.WhitePieces
                    .Select(piece => piece.GetComponent<ChessPiece>()).ToList()))
                return Shared.GameStatus.Draw;

            return Shared.GameStatus.Continue;
        }

        public void MakeBotTurn()
        {
            var botTurn =
                botPlayer.BotMakeMove(botPieces.Select(piece => piece.GetComponent<ChessPiece>()).ToList());
            AdvanceTurn(botTurn);
        }
        
        public void SwitchTurn()
        {
            IsWhiteTurn = !IsWhiteTurn;
            IsPlayerTurn = !IsPlayerTurn;
            movementManager.DisableOrEnablePickUpOnPieces(playersPieces);
        }

        private void DisableEnPassantTargetOnLastTurnPiece()
        {
            if (LastTurn != null && LastTurn.PiecesMovedInThisTurn.Pieces[^1].type == ChessPieceType.Pawn)
            {
                ((Pawn)LastTurn.PiecesMovedInThisTurn.Pieces[^1]).IsEnPassantTarget = false;
            }
        }

        private void SetPlayer()
        {
            var direction = Shared.TeamType.White.Equals(playersTeam) ? 1 : -1;
            xrOrigin.transform.position = new Vector3(playingPosition.x + (0.25f * direction), playingPosition.y - 2.5f,
                playingPosition.z);
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
