using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour
{

    private string path;

    private Settings settings;

    public TMP_Dropdown resDropdown; 
    public Toggle fullScreen;
    public Toggle borderless;
    private List <TMP_Dropdown.OptionData> resOptions;
    
    // Start is called before the first frame update
    void Start()
    {
        path = Application.persistentDataPath + "/settings.txt";
        IniResDropdown();

        //Read settings if exists else create and apply default
        if(ReadSettings()){
            ApplySettings();
            SetUiSettings();
        }
        else{
            settings = new Settings(1920,1080,true,false,60);

            int i = resOptions.Count-1;
            string str = resOptions[i].text;
            string[] words = str.Split(' ');
            
            SetWidth(int.Parse(words[0]));
            SetHeight(int.Parse(words[2]));
            SetRefreshRate(int.Parse(GetNumsString(words[4])));

            WriteSettings();
            ApplySettings();
            SetUiSettings();
        }
    }

    //Initialise the resDropdown with all compatible resolutions
    private void IniResDropdown(){
        resDropdown.ClearOptions();
        OptionDataIComparer comp = new OptionDataIComparer();
        resOptions = new List<TMP_Dropdown.OptionData>();
        foreach(Resolution res in Screen.resolutions){
            resOptions.Add(new TMP_Dropdown.OptionData(res.ToString()));
        }
        resOptions.Sort(comp);
        resDropdown.AddOptions(resOptions);
    }

    //Reads settings from file if exists
    private bool ReadSettings(){
        if(File.Exists(path)){
            settings = new Settings();
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(settings.GetType());
            FileStream stream = new FileStream(path, FileMode.Open);
            
            settings = x.Deserialize(stream) as Settings;

            stream.Close();
            return true;

        }
        else{
            return false;
        }
    }

    //Saves settings to file
    public void WriteSettings(){
        System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(settings.GetType());
        FileStream stream = new FileStream(path, FileMode.Create);
        
        x.Serialize(stream, settings);

        stream.Close();
    }

    public void SetWidth(int width){
        settings.width = width;
    }

    public void SetHeight(int height){
        settings.height = height;
    }

    public void SetRefreshRate(int rate){
        settings.refreshRate = rate;
    }

    public void SetFullscreen(bool fullscreen){
        settings.fullscreen = fullscreen;
    }

    public void SetBorderless(bool borderless){
        settings.borderlessWindowed = borderless;
    }

    public void SetSound(int sound){
        if(sound > 100){
            settings.sound = 100;
        }
        else if(sound < 0){
            settings.sound = 0;
        }
        else{
            settings.sound = sound;
        }
    }

    //Apply settings
    public void ApplySettings(){
        FullScreenMode fullScreenMode = new FullScreenMode();
        if(settings.borderlessWindowed){
            fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else if(settings.fullscreen){
            fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else{
            fullScreenMode = FullScreenMode.Windowed;
        }
        Screen.SetResolution(settings.width, settings.height,fullScreenMode, settings.refreshRate);

        

        //ADD SOUND SETTINGS
    }
    
    //Sets ui settings from settings object
    public void SetUiSettings(){
        int i = ResOptionIndexFind(settings.width+" x "+settings.height+ " @ "+ settings.refreshRate+"Hz");
        if(i != -1){
            resDropdown.value = i;
        }

        if(settings.borderlessWindowed){
            borderless.isOn = true;
            fullScreen.isOn = false;
        }
        else if(settings.fullscreen){
            borderless.isOn = false;
            fullScreen.isOn = true;
        }
        else{
            borderless.isOn = false;
            fullScreen.isOn = false;
        }
        
    }

    private int ResOptionIndexFind(string str){
        for(int i = 0; i<resOptions.Count; i++){
            if(resOptions[i].text == str){
                return i;
            }
        }
        return -1;
    }

    //Reads Settings in the ui and apply them to the settigns object
    public void ReadUiSettings(){
        int i = resDropdown.value;
        string str = resOptions[i].text;
        string[] words = str.Split(' ');
        
        SetWidth(int.Parse(words[0]));
        SetHeight(int.Parse(words[2]));
        SetRefreshRate(int.Parse(GetNumsString(words[4])));

        if(borderless.isOn){
            SetBorderless(true);
            SetFullscreen(false);
        }
        else if(fullScreen.isOn){
            SetBorderless(false);
            SetFullscreen(true);
        }
        else{
            SetBorderless(false);
            SetFullscreen(false);
        }
    }

    private string GetNumsString(string str){
        string val = "";
        foreach(char c in str){
            if(char.IsDigit(c)){
                val += c;
            }
        }
        return val;
    }


    //Toggles Fullscreen toggle interactable depending on state of Borderless toggle
    public void BorderlessToggle(){
        if(borderless.isOn){
            fullScreen.interactable = false;
        }
        else{
            fullScreen.interactable = true;
        }
    }


}

//Comparer for TMP_Dropdown.OptionData
public class OptionDataIComparer : IComparer<TMP_Dropdown.OptionData>
{

    public int Compare(TMP_Dropdown.OptionData val1, TMP_Dropdown.OptionData val2){
        string x,y;
        x = val1.text.Substring(0,4);
        y = val2.text.Substring(0,4);
        int X,Y;
        X = int.Parse(x);
        Y = int.Parse(y);

        return ((new CaseInsensitiveComparer()).Compare(X, Y));
    }
}
