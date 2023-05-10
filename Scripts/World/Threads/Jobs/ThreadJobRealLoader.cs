using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobRealLoader : ThreadJob
{
    Region region;
    public ThreadJobRealLoader(Region region) : base(true){
        this.region = region;
    }

    public override void PostProcess()
    {
        region.CreateAllChunks();
        region.QueueAllChunkUpdates();
    }

    public override void Process()
    {
        region.GenColumns();
    }
}
