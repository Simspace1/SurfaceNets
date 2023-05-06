using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobRegionCheapMesh : ThreadJob
{
    Region region;
    public ThreadJobRegionCheapMesh(Region region) : base(true){
        this.region = region;
    }

    public override void PostProcess()
    {
        region.RenderCheapMesh();
    }

    public override void Process()
    {
        region.GenerateCheapMesh();
    }
}
