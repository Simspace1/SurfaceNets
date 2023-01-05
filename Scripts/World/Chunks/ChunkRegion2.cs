using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkRegion2
{
    public RegionPos regionPos {get; private set;}

    public RegionCol regionCol {get; private set;}

    private Dictionary<WorldPos, Columns2> columns = new Dictionary<WorldPos, Columns2>(World.worldPosEqC);

    public bool generated {get; private set;} = false;
    public bool destroying {get; private set;} = false;
    public bool destroyed {get; private set;} = false;
    public bool loaded {get; private set;} = false;

    public ChunkRegion2(RegionPos pos, RegionCol regionCol){
        Debug.Assert(regionCol.regionPos.InColumn(pos), "Created a region "+ pos.ToString() +" in the Wrong RegionColumn " + regionCol.regionPos.ToColString());

        this.regionPos = pos;
        this.regionCol = regionCol;

        GenColumns();
    }

    private void GenColumns(){
        if(!loaded){
            WorldPos pos = regionPos.ToWorldPos();
            int x = pos.xi;
            int y = pos.yi;
            int z = pos.zi;

            WorldPos temp = null;
            for(int i = x; i < x+RegionCol.regionVoxels; i+= Chunk.chunkVoxels){
                for(int j = z; j < z+RegionCol.regionVoxels; j+= Chunk.chunkVoxels){
                    temp = new WorldPos(i,y,j);
                    Columns2 col = new Columns2(temp, this);
                    AddColumns(col);
                }
            }

            Debug.Assert(columns.Count == RegionCol.regionChunks* RegionCol.regionChunks,"ChunkRegion "+ regionPos.ToString()+ "has generated " + columns.Count + " Columns instead of " +RegionCol.regionChunks* RegionCol.regionChunks);
            generated = true;
        }
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

   

   


   
   

   

}
