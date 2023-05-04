using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobRegionCol : ThreadJob
{
    private RegionCol regionCol;
    private bool generated = false;

    public ThreadJobRegionCol(bool postProcess, RegionCol regionCol) : base(postProcess){
        this.regionCol = regionCol;
    }

    public override void PostProcess(){
        if(generated){
            regionCol.CreateAllChunks();
            regionCol.QueueAllChunkUpdates();
        }
        else{
            throw new System.NotImplementedException("Loading of vertical regions not yet implemented");
        }
    }

    public override void Process(){
        if(!regionCol.generated){
            regionCol.Generate();
            generated = true;
        }
        else{
            throw new System.NotImplementedException("Loading of vertical regions not yet implemented");
        }  
    }
}
