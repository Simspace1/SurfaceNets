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
    private UnityEngine.UI.RawImage map;

    [SerializeField]
    private PlayerController playerController;

    private bool gen = false;

    public void ToggleEscMenu(){
        if(escMenu.enabled){
            escMenu.enabled = false;
        }
        else{
            escMenu.enabled = true;
            if(!gen){
                gen = true;
                map.texture = world.gen.GenTexture2D(world.gen.GenerateWorldHeight());
            }
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
