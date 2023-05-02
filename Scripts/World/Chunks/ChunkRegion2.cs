using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class ChunkRegion2
{
    public RegionPos regionPos {get; private set;}

    public RegionCol regionCol {get; private set;}

    private List<WorldPos> savedColumnsList = new List<WorldPos>();
    private Dictionary<WorldPos, Columns2> columns = new Dictionary<WorldPos, Columns2>(World.worldPosEqC);

    public bool generated {get; private set;} = false;
    public bool destroying {get; private set;} = false;
    public bool destroyed {get; private set;} = false;
    public bool loaded {get; private set;} = false;
    public bool GenTerrain {get; private set;} = false;

    public bool chunksCreated {get; private set;} = false;
    public bool chunksGenerated {get; private set;} = false;

    public ChunkRegion2(RegionPos pos, RegionCol regionCol){
        Debug.Assert(regionCol.regionPos.InColumn(pos), "Created a region "+ pos.ToString() +" in the Wrong RegionColumn " + regionCol.regionPos.ToColString());

        this.regionPos = pos;
        this.regionCol = regionCol;

        GenColumns();
    }

    private void GenColumns(){
        WorldPos pos = regionPos.ToWorldPos();
        int x = pos.xi;
        int y = pos.yi;
        int z = pos.zi;

        WorldPos temp = null;
        for(int i = x; i < x+RegionCol.regionVoxels; i+= Chunk2.chunkVoxels){
            for(int j = z; j < z+RegionCol.regionVoxels; j+= Chunk2.chunkVoxels){
                temp = new WorldPos(i,y,j);
                if(WasSavedColumn(temp)){
                    throw new NotImplementedException("Chunk Columns loading not implemented yet");
                }
                else{
                    Columns2 col = new Columns2(temp, this);
                    AddColumns(col);
                }
            }
        }

        Debug.Assert(columns.Count == RegionCol.regionChunks* RegionCol.regionChunks,"ChunkRegion "+ regionPos.ToString()+ "has generated " + columns.Count + " Columns instead of " +RegionCol.regionChunks* RegionCol.regionChunks);
        generated = true;
    
    }

    private bool WasSavedColumn(WorldPos pos){
        if(savedColumnsList.Count == 0){
            return false;
        }

        foreach(WorldPos sPos in savedColumnsList){
            if(sPos.Equals(pos)){
                return true;
            }
        }
        return false;
    }

    public Columns2 GetColumn(WorldPos pos){
        if(!regionPos.Equals(pos.GetRegion())){
            return null;
        }
        Columns2 col = null;
        columns.TryGetValue(pos, out col);
        return col;
    }

    private void AddColumns(Columns2 col){
        if(destroying || destroyed || !col.columnPos.GetRegion().Equals(regionPos))
            return;
        columns.Add(col.columnPos,col);
    }

    public void SetGenTerrain(bool val){
        GenTerrain = val;
    }

    //test code for chunk generation
    public void CreateAllChunks(){
        foreach(var colEntry in columns){
            colEntry.Value.CreateChunks();
        }
        chunksCreated = true;
    }

    //test code for chunk generation
    public void GenerateAllChunks(){
        // ThreadPool.QueueUserWorkItem(GenerateAllChunks,this);
        GenerateAllChunks(this);
    }

    private void GenerateAllChunks(object state){
        foreach(var colEntry in columns){
            colEntry.Value.GenerateChunks();
        }
        chunksGenerated = true;
    }

    //test code for chunk updates
    public void UpdateAllChunks(){
        // ThreadPool.QueueUserWorkItem(UpdateAllChunks,this);
        UpdateAllChunks(this);
    }

    private void UpdateAllChunks(object state){
        foreach(var colEntry in columns){
            colEntry.Value.UpdateChunks();
        }
    }

    public void RenderAllChunks(){
        foreach(var colEntry in columns){
            colEntry.Value.RenderChunks();
        }
    }
}
