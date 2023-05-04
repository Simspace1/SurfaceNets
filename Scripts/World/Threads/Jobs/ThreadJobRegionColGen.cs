using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobRegionColGen : ThreadJob
{
    private RegionCol regionCol;

    public ThreadJobRegionColGen(RegionCol regionCol) : base(true){
        this.regionCol = regionCol;
    }

    public override void PostProcess(){
        regionCol.CreateAllChunks();
        regionCol.QueueAllChunkUpdates();
    }

    public override void Process(){
        regionCol.Generate();
    }
}
