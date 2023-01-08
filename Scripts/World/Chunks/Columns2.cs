using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Columns2
{
    public WorldPos columnPos {get; private set;}

    public ChunkRegion2 region {get; private set;}

    private Dictionary<WorldPos,Chunk2> chunks = new Dictionary<WorldPos, Chunk2>(World.worldPosEqC);

    public bool destroying {get; private set;} = false;
    public bool destroyed {get; private set;} = false;

    public Columns2(WorldPos pos, ChunkRegion2 region){
        Debug.Assert(region.regionPos.Equals(pos.GetRegion()), "Created a Column at "+ pos.ToIntString() +" in the Wrong Region " + region.regionPos.ToString());

        this.columnPos = pos;
        this.region = region;
    }

    // Must be performed in main thread
    public void CreateChunks(){
        int y = columnPos.yi;

        WorldPos pos = null;
        for(int i = y; i < y+RegionCol.regionVoxels; i+=Chunk2.chunkVoxels){
            pos = new WorldPos(columnPos.xi,i,columnPos.zi);
            Chunk2 chunk = World.GetWorld().CreateChunk(pos);
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

    public void UpdateChunks(){
        foreach(var chunkEntry in chunks){
            chunkEntry.Value.UpdateThird(chunkEntry.Value);
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

    



}
