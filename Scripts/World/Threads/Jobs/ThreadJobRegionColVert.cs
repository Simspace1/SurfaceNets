using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobRegionColVert : ThreadJob
{
    private RegionCol regionCol;

    public ThreadJobRegionColVert(RegionCol regionCol) : base(true){
        this.regionCol = regionCol;
    }

    public override void PostProcess()
    {
        throw new System.NotImplementedException();
    }

    public override void Process()
    {
        throw new System.NotImplementedException();
    }
}
