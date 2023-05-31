using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        // Properties
        public Chessboard chessboard;
        public MovementManager movementManager;
        public TileManager tileManager;
        
        private void Awake()
        {
            movementManager.TileManager = tileManager;
            chessboard.MovementManager = movementManager;
            chessboard.TileManager = tileManager;
            chessboard.StartGame();
        }
    }
}
