using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobChangeRealRegions : ThreadJob
{
    private List<Region> loadReal;
    private List<Region> unloadReal;

    public ThreadJobChangeRealRegions(List<Region> loadReal, List<Region> unloadReal) : base(false){
        this.loadReal = loadReal;
        this.unloadReal = unloadReal;
    }

    public override void PostProcess()
    {
        throw new System.NotImplementedException();
    }

    public override void Process()
    {
        if(loadReal.Count > 0){
            foreach(Region regionUp in loadReal){
                regionUp.QueueLoadReal(true);
            }
        }
        
        if(unloadReal.Count > 0){
            foreach(Region regionDown in unloadReal){
                regionDown.QueueLoadReal(false);
            }
        }

        MyThreadPool.QueueJob(new ThreadJobNotifyRealRegionsDone());
    }
}
