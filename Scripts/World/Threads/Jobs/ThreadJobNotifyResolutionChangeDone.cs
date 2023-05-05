using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobNotifyResolutionChangeDone : ThreadJob
{
    public ThreadJobNotifyResolutionChangeDone() : base(true){
    }

    public override void PostProcess()
    {
        LoadRegions.NotifiyChangeResolutionDone();
    }

    public override void Process()
    {
        
    }
}
