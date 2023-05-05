using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadJobChangeChunkColumnResolution : ThreadJob
{
    private Columns2 column;
    private bool fullRes;

    public ThreadJobChangeChunkColumnResolution(Columns2 column, bool fullRes) : base(true){
        this.column = column;
        this.fullRes = fullRes;
    }

    public override void PostProcess()
    {
        column.RenderChunks();
    }

    public override void Process()
    {
        column.ChangeResolution(fullRes);
    }
}
