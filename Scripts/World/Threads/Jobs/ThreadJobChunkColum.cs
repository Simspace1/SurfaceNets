using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobChunkColum : ThreadJob
{
    Columns2 column;

    public ThreadJobChunkColum(bool postProcess, Columns2 column) : base(postProcess){
        this.column = column;
    }

    public override void PostProcess(){
        column.RenderChunks();
    }

    public override void Process(){
        column.GenerateChunks();
        if(column.GetColumnRes()){
            column.UpdateChunksFull();
        }
        else{
            column.UpdateChunksHalf();
        }
    }
}
