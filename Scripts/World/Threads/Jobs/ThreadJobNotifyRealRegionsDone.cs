using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobNotifyRealRegionsDone : ThreadJob
{
    public ThreadJobNotifyRealRegionsDone() : base(true)
    {
    }

    public override void PostProcess()
    {
        LoadRegions.NotifyRealRegionsDone();
    }

    public override void Process()
    {
       
    }
}
