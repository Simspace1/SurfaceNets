using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveManager 
{
    public static void SaveWorld(World world){
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/saves/"+world.GetWorldName();
        if(!Directory.Exists(path)){
            Directory.CreateDirectory(path);
        }
        path += "/world.sav";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, world.GetWorldData());
        stream.Close();
    }

    public static bool LoadWorld(World world){
        string path = Application.persistentDataPath + "/saves/"+world.GetWorldName()+"/world.sav";
        if(File.Exists(path)){
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            WorldData wData = formatter.Deserialize(stream) as WorldData;
            world.SetWorldData(wData);
            StaticWorld.worldName = wData.worldName;
            StaticWorld.seed = wData.worldSeed;
            stream.Close();
            return true;
        }
        else{
            Debug.LogError("World File not found in "+ path);
            return false;
        }
        

    }

    public static AbrWorldData ReadWorld(string path){
        path += "/world.sav";
        if(File.Exists(path)){
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            WorldData data = formatter.Deserialize(stream) as WorldData;
            stream.Close();

            return new AbrWorldData(data.worldName,data.worldSeed,File.GetLastWriteTime(path));
        }
        else{
            return new AbrWorldData();
        }
    }

    public static void SaveChunkColumn(ChunkColumn col){
        
        if(!col.CheckModified()){
            return;
        }
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/saves/"+col.world.GetWorldName()+"/Chunks/Column_"+col.pos.x+"_"+col.pos.z+".sav";
        FileStream stream = new FileStream(path, FileMode.Create);

        ColumnData data = new ColumnData(col);
        formatter.Serialize(stream, data);
        stream.Close();

        WorldData wData = col.world.GetWorldData();

        if(!wData.columns.Contains(col.pos)){
            wData.columns.Add(col.pos);
        }
    }

    public static void LoadChunkColumn(ChunkColumn col){
        string path = Application.persistentDataPath + "/saves/"+col.world.GetWorldName()+"/Chunks/Column_"+col.pos.x+"_"+col.pos.z+".sav";
        if(File.Exists(path)){
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            ColumnData data = formatter.Deserialize(stream) as ColumnData;
            stream.Close();
            data.Revert(col);
        }
        else{
            Debug.LogError("ChunkColumn File not found in "+ path);
        }
    }

     public static ColumnData LoadChunkColumn2(ChunkColumn col){
        string path = col.path + "/saves/" + col.world.GetWorldName()+"/Chunks/Column_"+col.pos.x+"_"+col.pos.z+".sav";
        if(File.Exists(path)){
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            ColumnData data = formatter.Deserialize(stream) as ColumnData;
            stream.Close();
            return data;
        }
        else{
            Debug.LogError("ChunkColumn File not found in "+ path);
            return null;
        }
    }

//MUST FIX
    // public static void SaveAll(World world){
    //     foreach(var col in world.chunkColumns){
    //         if(col.Value.CheckModified()){
    //             SaveChunkColumn(col.Value);
    //         }  
    //     }
    //     SaveWorld(world);
    // }
}
