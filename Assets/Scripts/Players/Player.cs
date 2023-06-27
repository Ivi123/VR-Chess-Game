using System.Collections.Generic;
using ChessLogic;
using ChessPieces;
using UnityEngine.PlayerLoop;

namespace Players
{
    public abstract class Player
    {
        public Shared.TeamType Team { get; set; }
        public List<ChessPiece> Pieces { get; set; }
        public bool IsMyTurn { get; set; }
        public bool HasMoved { get; set; }
        
        protected Player()
        {
            Pieces = new List<ChessPiece>();
            IsMyTurn = false;
            HasMoved = false;
        }

        public void InitPieces()
        {
            foreach (var piece in Pieces)
                piece.MyPlayer = this;
        }

        public void EnablePieces()
        {
            foreach (var chessPiece in Pieces)
                chessPiece.EnablePickUpOnPiece();
        }

        public void DisablePieces()
        {
            foreach (var chessPiece in Pieces)
                chessPiece.DisablePickUpOnPiece();
        }
        
        public abstract void Update();
    }
}