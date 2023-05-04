using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobChunk : ThreadJob
{
    private Chunk2 chunk;
    
    public ThreadJobChunk(bool postProcess, Chunk2 chunk) : base(postProcess){
        this.chunk = chunk;
    }

    public override void PostProcess(){
        chunk.RenderMesh();
    }

    public override void Process(){
        if(chunk.GetChunkRes()){
            Chunk2.UpdateFull(chunk);
        }
        else{
            Chunk2.UpdateHalf(chunk);
        }
    }
}
