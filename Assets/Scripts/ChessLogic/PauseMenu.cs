using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace ChessLogic
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject wristUI;
        public GameObject wristInteractor;
        
        public bool activeWristUI = true;
        void Start()
        {
            DisplayWristUI();
        }
        

        public void PauseButtonPressed(InputAction.CallbackContext context)
        {
            
            if (context.performed)
                DisplayWristUI();
        }
        
        public void DisplayWristUI()
        {
            if (activeWristUI)
            {
                wristUI.SetActive(false);
                wristInteractor.SetActive(false);
                activeWristUI = false;
                Time.timeScale = 1;
            } else {
                wristInteractor.SetActive(true);
                wristUI.SetActive(true);
                activeWristUI = true;
                Time.timeScale = 0;
            }
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}