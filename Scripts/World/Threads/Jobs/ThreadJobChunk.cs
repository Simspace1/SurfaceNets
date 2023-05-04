using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobChunk : ThreadJob
{
    private Chunk2 chunk;
    
    public ThreadJobChunk(Chunk2 chunk) : base(true){
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
