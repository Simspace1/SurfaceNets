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
        regionCol.CreateAllRegions();
        regionCol.QueueAllRegionUpdates();
    }

    public override void Process(){
        regionCol.Generate2();
    }
}
