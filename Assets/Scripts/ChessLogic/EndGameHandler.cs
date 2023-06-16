using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChessLogic
{
    public class EndGameHandler : MonoBehaviour
    {
        public GameObject endGameUI;
        public GameObject endGameInteractor;
        public Text gameStatusText; 
        public void DisplayEndGameCanvas(Shared.GameStatus gameStatus)
        {
            endGameUI.SetActive(true);
            endGameInteractor.SetActive(true);

            switch (gameStatus)
            {
                case Shared.GameStatus.Defeat:
                    gameStatusText.text = "YOU LOST :(";
                    break;
                case Shared.GameStatus.Draw:
                    gameStatusText.text = "DRAW";
                    break;
                case Shared.GameStatus.Victory:
                    gameStatusText.text = "YOU WON!!";
                    break;
            }
                
        } 

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }

}
