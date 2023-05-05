using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Columns2
{
    public WorldPos columnPos {get; private set;}

    public Region region {get; private set;}

    private Dictionary<WorldPos,Chunk2> chunks = new Dictionary<WorldPos, Chunk2>(World.worldPosEqC);

    public bool destroying {get; private set;} = false;
    public bool destroyed {get; private set;} = false;

    public Columns2(WorldPos pos, Region region){
        Debug.Assert(region.regionPos.Equals(pos.GetRegion()), "Created a Column at "+ pos.ToIntString() +" in the Wrong Region " + region.regionPos.ToString());

        this.columnPos = pos;
        this.region = region;
    }

    // Must be performed in main thread
    public void CreateChunks(){
        int y = columnPos.yi;
        World world = World.GetWorld();

        int min = Mathf.FloorToInt((region.regionCol.minMax[0])/Chunk2.chunkSize)*Chunk2.chunkVoxels;
        int max = Mathf.CeilToInt((region.regionCol.minMax[1])/Chunk2.chunkSize)*Chunk2.chunkVoxels;

        if(min < y){
            min = y;
        }
        if(max > y + RegionCol.regionVoxels){
            max = y+ RegionCol.regionVoxels;
        }

        WorldPos pos = null;
        for(int i = min; i < max; i+=Chunk2.chunkVoxels){
            pos = new WorldPos(columnPos.xi,i,columnPos.zi);
            Chunk2 chunk = world.CreateChunk(pos, region.regionObject);
            chunk.SetPos(pos);
            chunk.SetColumn(this);
            AddChunk(chunk);
        }
    }

    public void GenerateChunks(){
        ColumnGen gen = region.regionCol.GetColumnGen(columnPos);
        Debug.Assert(gen != null, "Column gen at " + columnPos.ToString() + " is unexpectedly null");

        foreach(var chunkEntry in chunks){
            gen.ChunkGenC2(chunkEntry.Value);
        }
    }

    public void UpdateChunksFull(){
        foreach(var chunkEntry in chunks){
            Chunk2.UpdateFull(chunkEntry.Value);
        }
    }

    public void UpdateChunksHalf(){
        foreach(var chunkEntry in chunks){
            Chunk2.UpdateHalf(chunkEntry.Value);
        }
    }

    public void RenderChunks(){
        foreach(var chunkEntry in chunks){
            chunkEntry.Value.RenderMesh();
        }
    }

    public void AddChunk(Chunk2 chunk){
        if(destroying || destroyed || !InColChunk(chunk.chunkPos))
            return;
        chunks.Add(chunk.chunkPos,chunk);
    }


    //Checks if Chunk Position is in column
    public bool InColChunk(WorldPos pos){
        if(columnPos.xi == pos.xi && columnPos.zi == pos.zi && columnPos.yi <= pos.yi && pos.yi < columnPos.yi + RegionCol.regionVoxels)
            return true;
        return false;
    }

    public bool GetColumnRes(){
        return region.fullRes;
    }

    public void Destroy(){
        destroyed = true;
        foreach(var chunkEntry in chunks){
            chunkEntry.Value.Destroy();
        }
    }



}
