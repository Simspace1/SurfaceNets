using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class ColumnData 
{
    public WorldPos pos;
    public List<ChunkData> chunks;

    public ColumnData(ChunkColumn chunkColumn){
        pos = chunkColumn.columnPos;

        chunks = new List<ChunkData>();
        foreach(Chunk chunk in chunkColumn.chunks){
            chunks.Add(new ChunkData(chunk));
        }
    }

    public void Revert1(ChunkColumn col){
        col.columnPos = pos;
        col.loaded = true;
        col.chunks = new List<Chunk>();
    }

    public void ReCreateChunks(ChunkColumn col){
        foreach(ChunkData data in chunks){
            col.world.CreateChunkInst(data,col);
        }
    }

    public void Revert2(ChunkColumn col){
        foreach(ChunkData data in chunks){
            foreach(Chunk chunk in col.chunks){
                if(WorldPos.Equals(chunk.GetPos(), data.pos)){
                    data.Revert(chunk);
                    break;
                }
            }
        }
    }


}
