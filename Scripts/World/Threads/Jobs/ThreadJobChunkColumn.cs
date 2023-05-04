using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobChunkColumn : ThreadJob
{
    Columns2 column;

    public ThreadJobChunkColumn(Columns2 column) : base(true){
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
