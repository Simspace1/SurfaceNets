using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobChangeRegionResolution : ThreadJob
{
    private List<Region> regionsUp;
    private List<Region> regionsDown;

    public ThreadJobChangeRegionResolution(List<Region> regionsUp, List<Region> regionsDown) : base(false){
        this.regionsUp = regionsUp;
        this.regionsDown = regionsDown;
    }

    public override void PostProcess()
    {
        throw new System.NotImplementedException();
    }

    public override void Process()
    {
        if(regionsUp.Count > 0){
            foreach(Region regionUp in regionsUp){
                regionUp.QueueRes(true);
            }
        }
        
        if(regionsDown.Count > 0){
            foreach(Region regionDown in regionsDown){
                regionDown.QueueRes(false);
            }
        }        

        MyThreadPool.QueueJob(new ThreadJobNotifyResolutionChangeDone());
    }
}
