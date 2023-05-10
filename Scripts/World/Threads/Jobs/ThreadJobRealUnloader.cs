using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobRealUnloader : ThreadJob
{
    Region region;
    public ThreadJobRealUnloader(Region region) : base(true){
        this.region = region;
    }

    public override void PostProcess()
    {
        region.UnloadReal();
    }

    public override void Process()
    {
        if(region.modified){
            throw new System.NotImplementedException("Saving is not implemented Yet");
            throw new System.NotImplementedException("Modifiable far surface is not implemented Yet");
        }
    }
}
