using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobUnloader : ThreadJob
{
    List<RegionCol> toDestroy;

    public ThreadJobUnloader(List<RegionCol> toDestroy) : base(true){
        this.toDestroy = toDestroy;
    }

    public override void PostProcess()
    {
        World.GetWorld().DestroyRegionColumns(toDestroy);
        LoadRegions.NotifiyUnloadingDone();
    }

    public override void Process()
    {       
        foreach(RegionCol regionCol in toDestroy){
            regionCol.Save();
        }
    }
}
