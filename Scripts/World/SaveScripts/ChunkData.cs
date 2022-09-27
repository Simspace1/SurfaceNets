using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class ChunkData 
{
    public WorldPos pos;
    public Voxel [, ,] voxels;
    public MeshData meshData;

    public ChunkData(Chunk chunk){
        pos = chunk.GetPos();
        voxels = chunk.voxels;
        meshData = new MeshData(chunk.meshData);
    }

    public void Revert(Chunk chunk){
        chunk.SetPos(pos);
        chunk.voxels = voxels;
        chunk.meshData = meshData.Revert();
    }

}
