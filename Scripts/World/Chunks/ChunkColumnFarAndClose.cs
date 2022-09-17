using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkColumnFarAndClose 
{
    public FarChunkCol farChunkCol;
    public ChunkColumn col;

    public ChunkColumnFarAndClose(FarChunkCol far, ChunkColumn col){
        this.farChunkCol = far;
        this.col = col;
    }
}
