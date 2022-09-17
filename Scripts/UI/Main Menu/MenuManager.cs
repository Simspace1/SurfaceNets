using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Canvas mainMenu;
    [SerializeField]
    private Canvas optionsMenu;
    [SerializeField]
    private Canvas singlePlayerMenu;
    [SerializeField]
    private Canvas multiPlayerMenu;
    [SerializeField]
    private Canvas createWorldMenu;
    private Canvas currMenu;
    
    void Start(){
        mainMenu.enabled = true;
        optionsMenu.enabled = false;
        singlePlayerMenu.enabled = false;
        multiPlayerMenu.enabled = false;
        createWorldMenu.enabled = false;

        currMenu = mainMenu;
    }

    public void SwitchOptions(){
        currMenu.enabled = false;
        optionsMenu.enabled = true;

        currMenu = optionsMenu;
    }

    public void SwitchSinglePlayer(){
        currMenu.enabled = false;
        singlePlayerMenu.enabled = true;

        currMenu = singlePlayerMenu;
    }

    public void SwitchMultiPlayer(){
        currMenu.enabled = false;
        multiPlayerMenu.enabled = true;

        currMenu = multiPlayerMenu;
    }

    public void SwitchMainMenu(){
        currMenu.enabled = false;
        mainMenu.enabled = true;

        currMenu = mainMenu;
    }

    public void SwitchCreateWorld(){
        currMenu.enabled = false;
        createWorldMenu.enabled = true;

        currMenu = createWorldMenu;
    }

    public void QuitGame(){
        Application.Quit();
    }

    
}
