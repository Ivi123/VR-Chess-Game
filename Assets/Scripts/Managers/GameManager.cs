using System.Collections.Generic;
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
        public bool IsWhiteTurn { get; private set; }
        
        // Game History
        public List<Turn> History { get; set; }
        public Turn LastTurn { get; set; }
       
        
        // Player Info and team selection
        private Shared.TeamType playersTeam;
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
            movementManager.DisableOrEnablePickUpOnPieces(movementManager.BlackPieces);
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
        }

        public void SwitchTurn()
        {
            IsWhiteTurn = !IsWhiteTurn;
            movementManager.DisableOrEnablePickUpOnPieces(movementManager.WhitePieces);
            movementManager.DisableOrEnablePickUpOnPieces(movementManager.BlackPieces);
        }

        private void DisableEnPassantTargetOnLastTurnPiece()
        {
            if(LastTurn != null && LastTurn.PiecesMovedInThisTurn.Pieces[^1].type == ChessPieceType.Pawn)
            {
                ((Pawn)LastTurn.PiecesMovedInThisTurn.Pieces[^1]).IsEnPassantTarget = false;
            }
        }
        
        private void SetPlayer()
        {
            var direction = Shared.TeamType.White.Equals(playersTeam) ? 1 : -1;
            xrOrigin.transform.position = new Vector3(playingPosition.x + (0.25f * direction), playingPosition.y - 2.5f, playingPosition.z);
            xrOrigin.GetComponent<ActionBasedContinuousMoveProvider>().enabled = false;
        }
    }
}
