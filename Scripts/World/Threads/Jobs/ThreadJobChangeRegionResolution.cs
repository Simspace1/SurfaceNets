using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobChangeRegionResolution : ThreadJob
{
    private List<Region> regions;

    public ThreadJobChangeRegionResolution(List<Region> regions) : base(true){
        this.regions = regions;
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
