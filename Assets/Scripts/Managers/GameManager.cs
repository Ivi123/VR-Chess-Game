using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        // Properties
        public Chessboard chessboard;
        public MovementManager movementManager;
        public TileManager tileManager;
        public GameObject xrOrigin;
            
        public bool IsWhiteTurn { get; set; }
        public List<GameObject> teamSelectors;

        
        private Shared.TeamType playersTeam;
        private Vector3 playingPosition;
        
        private void Awake()
        {
            movementManager.TileManager = tileManager;
            chessboard.MovementManager = movementManager;
            chessboard.TileManager = tileManager;
            movementManager.GameManager = this;
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

        private void StartGame()
        {
            chessboard.StartGame();
            IsWhiteTurn = true;
            movementManager.DisableOrEnablePickUpOnPieces(movementManager.BlackPieces);
        }

        private void SetPlayer()
        {
            xrOrigin.transform.position = new Vector3(playingPosition.x, playingPosition.y - 1, playingPosition.z);
            xrOrigin.GetComponent<ActionBasedContinuousMoveProvider>().enabled = false;
        }
    }
}
