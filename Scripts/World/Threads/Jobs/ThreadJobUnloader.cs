using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobUnloader : ThreadJob
{
    private RegionPos playerPos;
    private int loadDistance;
    List<RegionCol> toDestroy;

    public ThreadJobUnloader(RegionPos playerPos, int loadDistance) : base(true){
        this.playerPos = playerPos;
        this.loadDistance = loadDistance;
    }

    public override void PostProcess()
    {
        World.GetWorld().DestroyRegionColumns(toDestroy);
        LoadRegions.NotifiyUnloadingDone();
    }

    public override void Process()
    {
        toDestroy = World.GetWorld().CheckDestroyRegionColumns(playerPos, loadDistance);
        
        foreach(RegionCol regionCol in toDestroy){
            regionCol.Save();
        }
    }
}
