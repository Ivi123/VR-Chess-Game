namespace Players
{
    public class HumanPlayer : Player
    {
        public override void Update()
        {
            if (!IsMyTurn || !HasMoved) return;
            
            DisablePieces();
            IsMyTurn = false;
            HasMoved = false;
        }
    }
}