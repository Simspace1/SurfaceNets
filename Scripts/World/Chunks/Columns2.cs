using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Columns2
{
    public WorldPos columnPos {get; private set;}

    public ChunkRegion2 region {get; private set;}



    public Columns2(WorldPos pos, ChunkRegion2 region){
        Debug.Assert(region.regionPos.Equals(pos.GetRegion()), "Created a Column at "+ pos.ToIntString() +" in the Wrong Region " + region.regionPos.ToString());

        this.columnPos = pos;
        this.region = region;
    }
}
