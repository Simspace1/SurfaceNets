using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfPt 
{
    public float x,y,z;
    public float scale = Chunk2.voxelSize;

    public SurfPt(float x, float y, float z){
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public void Add(SurfPt surfPt){
        this.x += surfPt.x;
        this.y += surfPt.y;
        this.z += surfPt.z;
    }

    public void Divide(int x){
        this.x = this.x/x;
        this.y = this.y/x;
        this.z = this.z/x;
    }

    public bool InRange(float x, float y, float z){
        if (this.x < x || this.x >= x+Chunk.voxelSize || this.y < y || this.y >= y+Chunk.voxelSize || this.z < z || this.z >= z+Chunk.voxelSize){
            return false;
        }
        else{
            return true;
        }
    }
}
