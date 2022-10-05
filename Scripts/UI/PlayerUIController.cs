using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField]
    private World world;

    [SerializeField]
    private Canvas escMenu;

    [SerializeField]
    private PlayerController playerController;

    public void ToggleEscMenu(){
        if(escMenu.enabled){
            escMenu.enabled = false;
        }
        else{
            escMenu.enabled = true;
        }
    }

    public void Resume(){
        playerController.TogglePlayerUiControls(true);
        ToggleEscMenu();
    }

    public void MainMenu(){
        world.SaveAll();
        SceneManager.LoadScene("Main Menu");
    }
}
