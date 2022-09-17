using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class ColumnData 
{
    public WorldPos pos;
    public List<ChunkData> chunks;

    public ColumnData(ChunkColumn chunkColumn){
        pos = chunkColumn.pos;

        chunks = new List<ChunkData>();
        foreach(Chunk chunk in chunkColumn.chunks){
            chunks.Add(new ChunkData(chunk));
        }
    }

    public void Revert(ChunkColumn col){
        col.pos = pos;
        col.chunks = new List<Chunk>();

        foreach(ChunkData data in chunks){
            col.world.CreateChunk(data,col);
        }

        col.created = true;
        col.loaded = true;
    }

    public void Revert1(ChunkColumn col){
        col.pos = pos;
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
                if(WorldPos.Equals(chunk.pos, data.pos)){
                    data.Revert(chunk);
                    break;
                }
            }
        }
    }


}
