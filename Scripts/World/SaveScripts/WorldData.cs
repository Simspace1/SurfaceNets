using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldData 
{

    public List<WorldPos> columns = new List<WorldPos>();
    public string worldName;
    public ulong worldSeed;

    public WorldData(string name, ulong seed){
        worldName = name;
        worldSeed = seed;
    }

    public bool Contains(WorldPos pos){
        foreach(WorldPos pos1 in columns){
            if(WorldPos.Equals(pos,pos1)){
                return true;
            }
        }
        return false;
    }
}
