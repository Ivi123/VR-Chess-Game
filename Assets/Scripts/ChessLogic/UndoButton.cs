using Managers;
using UnityEngine;

namespace ChessLogic
{
    public class UndoButton : MonoBehaviour
    {
        public MovementManager movementManager;

        public void UndoMove()
        {
            movementManager.UndoLastMove();
        }
    }
}
