using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SingleplayerManager : MonoBehaviour
{
    [SerializeField]
    private Canvas singlePlayerCanvas;
    [SerializeField]
    private GameObject scrollContent;
    [SerializeField]
    private GameObject worldNamePrefab;
    [SerializeField]
    private TMP_Text startButton;
    [SerializeField]
    private TMP_Text worldNameIn;
    [SerializeField]
    private TMP_Text seedIn;
    private string seedInTemp;


    private bool updated = false;

    public string currentSelectWorld;


    public List<AbrWorldData> worldList;

    void Start(){
        seedInTemp = seedIn.text;
    }

    // Update is called once per frame
    void Update()
    {
        if(!updated && singlePlayerCanvas.enabled){
            PopulateScroll();
            updated = true;
        }
    }

    private void PopulateScroll(){
        string path = Application.persistentDataPath + "/saves/";
        string[] dir = Directory.GetDirectories(path);
        worldList = new List<AbrWorldData>();


        foreach(string loc in dir){
            AbrWorldData world = SaveManager.ReadWorld(loc);

            if(world.valid){
                worldList.Add(world);
            }
        }
        worldList.Sort();
        worldList.Reverse();

        foreach(AbrWorldData world in worldList){
            GameObject worldLabel = Instantiate(worldNamePrefab) as GameObject;
            worldLabel.SetActive(true);

            worldLabel.GetComponent<WorldSelect>().SetText(world.worldName);
            worldLabel.GetComponent<WorldSelect>().manager = this;

            worldLabel.transform.SetParent(scrollContent.transform, false);
        }
        
    }

    public void SetCurrentWorld(string name){
        currentSelectWorld = name;
        startButton.text = "Start: "+name;
    }

    public void PlayWorld(){
        StaticWorld.worldName = currentSelectWorld;
        foreach(AbrWorldData world in worldList){
            if(world.worldName == currentSelectWorld){
                StaticWorld.seed = world.worldSeed;
            }
        }

        SceneManager.LoadScene("Game");
    }


    public void CreateWorld(){
        if(worldNameIn.text != null && NotAlreadyCreated(worldNameIn.text)){
            if(seedIn.text != seedInTemp){
               string seed = "";
               foreach(char c in seedIn.text){
                    if(char.IsLetter(c)){
                        seed += (c - 0).ToString();
                    }
                    else if(char.IsDigit(c)){
                        seed += c.ToString();
                    }
               }

               StaticWorld.seed = ulong.Parse(seed);
            }
            else{
                System.DateTime time = System.DateTime.Now;
                int timeInt = time.Year + time.Month+ time.Hour + time.Minute + time.Second + time.Millisecond;
                Random.InitState(timeInt);
                StaticWorld.seed = (ulong) Mathf.FloorToInt(Random.Range(0, 2147483647));
            }
            
            StaticWorld.worldName = worldNameIn.text;
            SceneManager.LoadScene("Game");
        }
    }

    private bool NotAlreadyCreated(string name){
        foreach(AbrWorldData world in worldList){
            if(world.worldName == name){
                return false;
            }
        }
        return true;
    }
}

public class AbrWorldData : System.IComparable
{
    public string worldName;
    public ulong worldSeed;
    public System.DateTime date;

    public bool valid;

    public AbrWorldData(){
        valid = false;
    }

    public AbrWorldData(string name, ulong seed, System.DateTime time){
        worldName = name;
        worldSeed = seed;
        date = time;
        valid = true;
    }

    public int CompareTo(object obj){
        if(obj == null){
            return 1;
        }

        AbrWorldData otherWorld = obj as AbrWorldData;
        if(otherWorld != null){
            return this.date.CompareTo(otherWorld.date);
        }
        else{
            throw new System.ArgumentException("Ojbect is not a AbrWorldData");
        }
    }
}
