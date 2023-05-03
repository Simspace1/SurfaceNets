using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadRegions : MonoBehaviour
{
    [SerializeField]
    private World world;

    public static int maxRegionRadius = 10;
    public static int fullResRegionRadius = 2;

    private List<RegionPos> regionList = new List<RegionPos>();

    private List<Region> generateList = new List<Region>();
    private List<Region> updateList = new List<Region>();
    private List<Region> renderList = new List<Region>();

    // Start is called before the first frame update
    void Start()
    {
        for(int radius = 0; radius <= maxRegionRadius; radius++){
            for(int x = 0; x <= radius; x++){
                for(int z = 0; z <=radius; z++){
                    if((Mathf.Abs(x) == radius || Mathf.Abs(z) == radius) && MyMath.Hypothenuse(x,z) <= maxRegionRadius){
                        regionList.Add(new RegionPos(x,0,z));
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Unload()){
            return;
        }

        Load();
    }

    private void Load(){
        
    }

    private bool Unload(){
        return false;
    }

    private void ListRemover<T>(List<T> mainList, List<T> removeList){
        foreach(T obj in removeList){
            mainList.Remove(obj);
        }
    }
}
