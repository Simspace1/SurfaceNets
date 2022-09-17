using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class WorldSelect : MonoBehaviour
{
    
    public SingleplayerManager manager;

    [SerializeField]
    private TMP_Text text;

    public string WorldName;

    public void SetText(string textin){
        text.text = textin;
        WorldName = textin;
    }

    public void OnClick(){
        manager.SetCurrentWorld(WorldName);
    }
}
