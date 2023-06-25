using System;
using ChessLogic;
using ChessPieces;
using Managers;

namespace Players
{
    public class AIPlayer : Player
    {
        public GameManager gameManager;
        public MovementManager movementManager;

        public override void Update()
        {
            if (!IsMyTurn || !HasMoved) return;
            
            IsMyTurn = false;
            HasMoved = false;
        }

        /*
        private int Minimax(char[,] board, int depth, Player currentPlayer, bool maximizingPlayer)
        {
            if (depth == 0 
                || gameManager.GameStatus is Shared.GameStatus.Defeat or Shared.GameStatus.Draw or Shared.GameStatus.Victory) {
                return EvaluateBoard(gameManager.chessboard);
            }

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                //i want to have here the possible moves already generated
                movementManager.GenerateAllMoves();
                movementManager.EvaluateKingStatus();
                movementManager.EliminateInvalidMoves(GameManager.IsWhiteTurn);
                foreach (Move move in gameManage.)
                {
                    char[,] newBoard = AIMakesMove();
                    int eval = Minimax(newBoard, depth - 1, GetOpponent(currentPlayer), false);
                    maxEval = Math.Max(maxEval, eval);
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (ChessMove move in GenerateMoves(board, currentPlayer))
                {
                    char[,] newBoard = MakeMove(board, move);
                    int eval = Minimax(newBoard, depth - 1, GetOpponent(currentPlayer), true);
                    minEval = Math.Min(minEval, eval);
                }
                return minEval;
            }
        }
        
        private int EvaluateBoard(Chessboard chessboard)
        {
            int score = 0;
            //int size = che.GetLength(0);

            /*for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    char piece = board[i, j];
                   
                    score += chessboard.get;
                    
                }
            }#1#
            //how can i add scores?
            //for player, the scores have to be - and for bot's have to be +

            return score;
        }
        
        public void AIMakesMove()
        {
            //rules for moves
        }*/
    }
}