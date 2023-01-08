using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
[System.Serializable]

public class Voxel 
{
    const float tileSizex = 1f;
    const float tileSizey = 0.25f;
    private static uint BlockID = 1;

    [SerializeField]
    protected uint id = 1;
    public float sDistF = 1;
    [System.NonSerialized]
    public bool air = false;
    [System.NonSerialized]
    public bool resource = false;

    public enum Direction{north, east, south, west, up, down}
    public enum Axis{x,y,z}

    public Voxel(){
        this.id = BlockID;
    }

    public Voxel(float sDistF){
        this.id = BlockID;
        if(sDistF < Chunk.sDistLimit && sDistF > -Chunk.sDistLimit ){
            this.sDistF = sDistF;
        }
        else if(sDistF < 0){
            this.sDistF = -Chunk.sDistLimit;
        }
        else{
            this.sDistF = Chunk.sDistLimit;
        }
        
    }

    public struct VoxelData{
        public float sDistF;

        public VoxelData(float sDistF){
            this.sDistF = sDistF;
        }
    }

    public SurfPt FindSurfacePoint(Chunk chunk,float x, float y, float z){        
        List<SurfPt> edgePts = new List<SurfPt>();

        

        //Fetch each of the 8 voxels that limits this cube
        Voxel voxel000 = this;
        Voxel voxel100 = chunk.GetVoxel(x+Chunk.voxelSize,y,z);
        Voxel voxel010 = chunk.GetVoxel(x,y+Chunk.voxelSize,z);
        Voxel voxel001 = chunk.GetVoxel(x,y,z+Chunk.voxelSize);

        Voxel voxel110 = chunk.GetVoxel(x+Chunk.voxelSize,y+Chunk.voxelSize,z);
        Voxel voxel101 = chunk.GetVoxel(x+Chunk.voxelSize,y,z+Chunk.voxelSize);
        Voxel voxel011 = chunk.GetVoxel(x,y+Chunk.voxelSize,z+Chunk.voxelSize);
        Voxel voxel111 = chunk.GetVoxel(x+Chunk.voxelSize,y+Chunk.voxelSize,z+Chunk.voxelSize);


        // Calculates each of the edges intercept points
        SurfPt temp;
        if(!SameSignsDistF(voxel000,voxel100)){
            temp = SurfaceEdgeIntercept(voxel000,voxel100,x,y,z,Axis.x);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel000,voxel010)){
            temp = SurfaceEdgeIntercept(voxel000,voxel010,x,y,z,Axis.y);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel000,voxel001)){
            temp = SurfaceEdgeIntercept(voxel000,voxel001,x,y,z,Axis.z);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel100,voxel110)){
            temp = SurfaceEdgeIntercept(voxel100,voxel110,x+Chunk.voxelSize,y,z,Axis.y);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel100,voxel101)){
            temp = SurfaceEdgeIntercept(voxel100,voxel101,x+Chunk.voxelSize,y,z,Axis.z);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel010,voxel110)){
            temp = SurfaceEdgeIntercept(voxel010,voxel110,x,y+Chunk.voxelSize,z,Axis.x);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel010,voxel011)){
            temp = SurfaceEdgeIntercept(voxel010,voxel011,x,y+Chunk.voxelSize,z,Axis.z);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel001,voxel101)){
            temp = SurfaceEdgeIntercept(voxel001,voxel101,x,y,z+Chunk.voxelSize,Axis.x);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel001,voxel011)){
            temp = SurfaceEdgeIntercept(voxel001,voxel011,x,y,z+Chunk.voxelSize,Axis.y);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel011,voxel111)){
            temp = SurfaceEdgeIntercept(voxel011,voxel111,x,y+Chunk.voxelSize,z+Chunk.voxelSize,Axis.x);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel110,voxel111)){
            temp = SurfaceEdgeIntercept(voxel110,voxel111,x+Chunk.voxelSize,y+Chunk.voxelSize,z,Axis.z);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel101,voxel111)){
            temp = SurfaceEdgeIntercept(voxel101,voxel111,x+Chunk.voxelSize,y,z+Chunk.voxelSize,Axis.y);
            if (temp != null)
                edgePts.Add(temp);
        }
        

        //Variable to count average
        int edgePtN = edgePts.Count;
        //if there is no surface point returns null
        if(edgePtN == 0){
            return null;
        }

        //calculates surface point
        SurfPt surfPt = new SurfPt(0,0,0);
        for(int i = 0; i < edgePtN; i++){
            surfPt.Add(edgePts[0]);
            edgePts.RemoveAt(0);
        }
        surfPt.Divide(edgePtN);
        return surfPt;
    }

    //int version
    public SurfPt FindSurfacePoint(Chunk chunk,int xi, int yi, int zi){       
        if(Mathf.Abs(sDistF) >= Chunk.sDistLimit){
            return null;
        }

        List<SurfPt> edgePts = new List<SurfPt>();

        //Fetch each of the 8 voxels that limits this cube
        Voxel voxel000 = this;
        Voxel voxel100 = chunk.GetVoxel(xi+1,yi,zi);
        Voxel voxel010 = chunk.GetVoxel(xi,yi+1,zi);
        Voxel voxel001 = chunk.GetVoxel(xi,yi,zi+1);

        Voxel voxel110 = chunk.GetVoxel(xi+1,yi+1,zi);
        Voxel voxel101 = chunk.GetVoxel(xi+1,yi,zi+1);
        Voxel voxel011 = chunk.GetVoxel(xi,yi+1,zi+1);
        Voxel voxel111 = chunk.GetVoxel(xi+1,yi+1,zi+1);

        // Stopwatch stopWatch = new Stopwatch();
        // stopWatch.Start();

        // Calculates each of the edges intercept points
        SurfPt temp;
        if(!SameSignsDistF(voxel000,voxel100)){
            temp = SurfaceEdgeIntercept(voxel000,voxel100,xi,yi,zi,Axis.x);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel000,voxel010)){
            temp = SurfaceEdgeIntercept(voxel000,voxel010,xi,yi,zi,Axis.y);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel000,voxel001)){
            temp = SurfaceEdgeIntercept(voxel000,voxel001,xi,yi,zi,Axis.z);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel100,voxel110)){
            temp = SurfaceEdgeIntercept(voxel100,voxel110,xi+1,yi,zi,Axis.y);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel100,voxel101)){
            temp = SurfaceEdgeIntercept(voxel100,voxel101,xi+1,yi,zi,Axis.z);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel010,voxel110)){
            temp = SurfaceEdgeIntercept(voxel010,voxel110,xi,yi+1,zi,Axis.x);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel010,voxel011)){
            temp = SurfaceEdgeIntercept(voxel010,voxel011,xi,yi+1,zi,Axis.z);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel001,voxel101)){
            temp = SurfaceEdgeIntercept(voxel001,voxel101,xi,yi,zi+1,Axis.x);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel001,voxel011)){
            temp = SurfaceEdgeIntercept(voxel001,voxel011,xi,yi,zi+1,Axis.y);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel011,voxel111)){
            temp = SurfaceEdgeIntercept(voxel011,voxel111,xi,yi+1,zi+1,Axis.x);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel110,voxel111)){
            temp = SurfaceEdgeIntercept(voxel110,voxel111,xi+1,yi+1,zi,Axis.z);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel101,voxel111)){
            temp = SurfaceEdgeIntercept(voxel101,voxel111,xi+1,yi,zi+1,Axis.y);
            if (temp != null)
                edgePts.Add(temp);
        }


        
        
        // stopWatch.Stop();
        // float temp2 = stopWatch.ElapsedTicks;

        //Variable to count average
        int edgePtN = edgePts.Count;
        //if there is no surface point returns null
        if(edgePtN == 0){
            return null;
        }

        //calculates surface point
        SurfPt surfPt = new SurfPt(0,0,0);
        for(int i = 0; i < edgePtN; i++){
            surfPt.Add(edgePts[0]);
            edgePts.RemoveAt(0);
        }
        surfPt.Divide(edgePtN);
        return surfPt;
    }

    //int version
    public SurfPt FindSurfacePoint(Chunk2 chunk,int xi, int yi, int zi, int scale = 1){       
        if(Mathf.Abs(sDistF) >= Chunk2.sDistLimit){
            return null;
        }

        List<SurfPt> edgePts = new List<SurfPt>();

        //Fetch each of the 8 voxels that limits this cube
        Voxel voxel000 = this;
        Voxel voxel100 = chunk.GetVoxel(xi+scale,yi,zi);
        Voxel voxel010 = chunk.GetVoxel(xi,yi+scale,zi);
        Voxel voxel001 = chunk.GetVoxel(xi,yi,zi+scale);

        Voxel voxel110 = chunk.GetVoxel(xi+scale,yi+scale,zi);
        Voxel voxel101 = chunk.GetVoxel(xi+scale,yi,zi+scale);
        Voxel voxel011 = chunk.GetVoxel(xi,yi+scale,zi+scale);
        Voxel voxel111 = chunk.GetVoxel(xi+scale,yi+scale,zi+scale);

        // Stopwatch stopWatch = new Stopwatch();
        // stopWatch.Start();

        // Calculates each of the edges intercept points
        SurfPt temp;
        if(!SameSignsDistF(voxel000,voxel100)){
            temp = SurfaceEdgeIntercept(voxel000,voxel100,xi,yi,zi,Axis.x);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel000,voxel010)){
            temp = SurfaceEdgeIntercept(voxel000,voxel010,xi,yi,zi,Axis.y);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel000,voxel001)){
            temp = SurfaceEdgeIntercept(voxel000,voxel001,xi,yi,zi,Axis.z);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel100,voxel110)){
            temp = SurfaceEdgeIntercept(voxel100,voxel110,xi+1,yi,zi,Axis.y);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel100,voxel101)){
            temp = SurfaceEdgeIntercept(voxel100,voxel101,xi+1,yi,zi,Axis.z);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel010,voxel110)){
            temp = SurfaceEdgeIntercept(voxel010,voxel110,xi,yi+1,zi,Axis.x);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel010,voxel011)){
            temp = SurfaceEdgeIntercept(voxel010,voxel011,xi,yi+1,zi,Axis.z);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel001,voxel101)){
            temp = SurfaceEdgeIntercept(voxel001,voxel101,xi,yi,zi+1,Axis.x);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel001,voxel011)){
            temp = SurfaceEdgeIntercept(voxel001,voxel011,xi,yi,zi+1,Axis.y);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel011,voxel111)){
            temp = SurfaceEdgeIntercept(voxel011,voxel111,xi,yi+1,zi+1,Axis.x);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel110,voxel111)){
            temp = SurfaceEdgeIntercept(voxel110,voxel111,xi+1,yi+1,zi,Axis.z);
            if (temp != null)
                edgePts.Add(temp);
        }
        if(!SameSignsDistF(voxel101,voxel111)){
            temp = SurfaceEdgeIntercept(voxel101,voxel111,xi+1,yi,zi+1,Axis.y);
            if (temp != null)
                edgePts.Add(temp);
        }


        
        
        // stopWatch.Stop();
        // float temp2 = stopWatch.ElapsedTicks;

        //Variable to count average
        int edgePtN = edgePts.Count;
        //if there is no surface point returns null
        if(edgePtN == 0){
            return null;
        }

        //calculates surface point
        SurfPt surfPt = new SurfPt(0,0,0);
        for(int i = 0; i < edgePtN; i++){
            surfPt.Add(edgePts[0]);
            edgePts.RemoveAt(0);
        }
        surfPt.Divide(edgePtN);
        return surfPt;
    }

    // public SurfPt FindSurfacePoint(Chunkv2 chunk,int xi, int yi, int zi){        
    //     List<SurfPt> edgePts = new List<SurfPt>();

    //     //Fetch each of the 8 voxels that limits this cube
    //     Voxel voxel000 = this;
    //     Voxel voxel100 = chunk.GetVoxel(xi+1,yi,zi);
    //     Voxel voxel010 = chunk.GetVoxel(xi,yi+1,zi);
    //     Voxel voxel001 = chunk.GetVoxel(xi,yi,zi+1);

    //     Voxel voxel110 = chunk.GetVoxel(xi+1,yi+1,zi);
    //     Voxel voxel101 = chunk.GetVoxel(xi+1,yi,zi+1);
    //     Voxel voxel011 = chunk.GetVoxel(xi,yi+1,zi+1);
    //     Voxel voxel111 = chunk.GetVoxel(xi+1,yi+1,zi+1);

    //     // Calculates each of the edges intercept points
    //     SurfPt temp;
    //     if(!SameSignsDistF(voxel000,voxel100)){
    //         temp = SurfaceEdgeIntercept(voxel000,voxel100,xi,yi,zi,Axis.x);
    //         if (temp != null)
    //             edgePts.Add(temp);
    //     }
    //     if(!SameSignsDistF(voxel000,voxel010)){
    //         temp = SurfaceEdgeIntercept(voxel000,voxel010,xi,yi,zi,Axis.y);
    //         if (temp != null)
    //             edgePts.Add(temp);
    //     }
    //     if(!SameSignsDistF(voxel000,voxel001)){
    //         temp = SurfaceEdgeIntercept(voxel000,voxel001,xi,yi,zi,Axis.z);
    //         if (temp != null)
    //             edgePts.Add(temp);
    //     }
    //     if(!SameSignsDistF(voxel100,voxel110)){
    //         temp = SurfaceEdgeIntercept(voxel100,voxel110,xi+1,yi,zi,Axis.y);
    //         if (temp != null)
    //             edgePts.Add(temp);
    //     }
    //     if(!SameSignsDistF(voxel100,voxel101)){
    //         temp = SurfaceEdgeIntercept(voxel100,voxel101,xi+1,yi,zi,Axis.z);
    //         if (temp != null)
    //             edgePts.Add(temp);
    //     }
    //     if(!SameSignsDistF(voxel010,voxel110)){
    //         temp = SurfaceEdgeIntercept(voxel010,voxel110,xi,yi+1,zi,Axis.x);
    //         if (temp != null)
    //             edgePts.Add(temp);
    //     }
    //     if(!SameSignsDistF(voxel010,voxel011)){
    //         temp = SurfaceEdgeIntercept(voxel010,voxel011,xi,yi+1,zi,Axis.z);
    //         if (temp != null)
    //             edgePts.Add(temp);
    //     }
    //     if(!SameSignsDistF(voxel001,voxel101)){
    //         temp = SurfaceEdgeIntercept(voxel001,voxel101,xi,yi,zi+1,Axis.x);
    //         if (temp != null)
    //             edgePts.Add(temp);
    //     }
    //     if(!SameSignsDistF(voxel001,voxel011)){
    //         temp = SurfaceEdgeIntercept(voxel001,voxel011,xi,yi,zi+1,Axis.y);
    //         if (temp != null)
    //             edgePts.Add(temp);
    //     }
    //     if(!SameSignsDistF(voxel011,voxel111)){
    //         temp = SurfaceEdgeIntercept(voxel011,voxel111,xi,yi+1,zi+1,Axis.x);
    //         if (temp != null)
    //             edgePts.Add(temp);
    //     }
    //     if(!SameSignsDistF(voxel110,voxel111)){
    //         temp = SurfaceEdgeIntercept(voxel110,voxel111,xi+1,yi+1,zi,Axis.z);
    //         if (temp != null)
    //             edgePts.Add(temp);
    //     }
    //     if(!SameSignsDistF(voxel101,voxel111)){
    //         temp = SurfaceEdgeIntercept(voxel101,voxel111,xi+1,yi,zi+1,Axis.y);
    //         if (temp != null)
    //             edgePts.Add(temp);
    //     }


        
        

    //     //Variable to count average
    //     int edgePtN = edgePts.Count;
    //     //if there is no surface point returns null
    //     if(edgePtN == 0){
    //         return null;
    //     }

    //     //calculates surface point
    //     SurfPt surfPt = new SurfPt(0,0,0);
    //     for(int i = 0; i < edgePtN; i++){
    //         surfPt.Add(edgePts[0]);
    //         edgePts.RemoveAt(0);
    //     }
    //     surfPt.Divide(edgePtN);
    //     return surfPt;
    // }

    SurfPt SurfaceEdgeIntercept(Voxel v1, Voxel v2, float x, float y, float z, Axis axis){
        switch(axis){
            case Axis.x:
                return new SurfPt((1-v1.sDistF/(v1.sDistF-v2.sDistF))*x+(v1.sDistF/(v1.sDistF-v2.sDistF))*(x+Chunk.voxelSize),y,z);
            case Axis.y:
                return new SurfPt(x,(1-v1.sDistF/(v1.sDistF-v2.sDistF))*y+(v1.sDistF/(v1.sDistF-v2.sDistF))*(y+Chunk.voxelSize),z);
            case Axis.z:
                return new SurfPt(x,y,(1-v1.sDistF/(v1.sDistF-v2.sDistF))*z+(v1.sDistF/(v1.sDistF-v2.sDistF))*(z+Chunk.voxelSize));
        }
        return null;
    }

    SurfPt SurfaceEdgeIntercept(Voxel v1, Voxel v2, int xi, int yi, int zi, Axis axis){
        switch(axis){
            case Axis.x:
                return new SurfPt((1-v1.sDistF/(v1.sDistF-v2.sDistF))*(xi*Chunk.voxelSize)+(v1.sDistF/(v1.sDistF-v2.sDistF))*((xi+1 )*Chunk.voxelSize),yi*Chunk.voxelSize,zi*Chunk.voxelSize);
            case Axis.y:
                return new SurfPt(xi*Chunk.voxelSize,(1-v1.sDistF/(v1.sDistF-v2.sDistF))*(yi*Chunk.voxelSize)+(v1.sDistF/(v1.sDistF-v2.sDistF))*((yi+1)*Chunk.voxelSize),zi*Chunk.voxelSize);
            case Axis.z:
                return new SurfPt(xi*Chunk.voxelSize,yi*Chunk.voxelSize,(1-v1.sDistF/(v1.sDistF-v2.sDistF))*(zi*Chunk.voxelSize)+(v1.sDistF/(v1.sDistF-v2.sDistF))*((zi+1)*Chunk.voxelSize));
        }
        return null;
    }

    public Vector3 SurfGrad(Chunk chunk, float x, float y, float z){
        float sDistF000 = sDistF;
        float sDistF001 = chunk.GetVoxel(x,y,z+Chunk.voxelSize).sDistF; 
        float sDistF010 = chunk.GetVoxel(x,y+Chunk.voxelSize,z).sDistF;
        float sDistF100 = chunk.GetVoxel(x+Chunk.voxelSize,y,z).sDistF;
        float sDistF110 = chunk.GetVoxel(x+Chunk.voxelSize,y+Chunk.voxelSize,z).sDistF;
        float sDistF011 = chunk.GetVoxel(x,y+Chunk.voxelSize,z+Chunk.voxelSize).sDistF;
        float sDistF101 = chunk.GetVoxel(x+Chunk.voxelSize,y,z+Chunk.voxelSize).sDistF;
        float sDistF111 = chunk.GetVoxel(x+Chunk.voxelSize,y+Chunk.voxelSize,z+Chunk.voxelSize).sDistF;
        float normX = (sDistF100-sDistF000) + (sDistF101-sDistF001) + (sDistF110-sDistF010) + (sDistF111-sDistF011);
        float normY = (sDistF010-sDistF000) + (sDistF011-sDistF001) + (sDistF110-sDistF100) + (sDistF111-sDistF101);
        float normZ = (sDistF001-sDistF000) + (sDistF101-sDistF100) + (sDistF011-sDistF010) + (sDistF111-sDistF110);
        return new Vector3(normX,normY,normZ);
    }

    //int version of SurfGrad
    public Vector3 SurfGrad(Chunk chunk, int x, int y, int z){
        float sDistF000 = sDistF;
        float sDistF001 = chunk.GetVoxel(x,y,z+1).sDistF; 
        float sDistF010 = chunk.GetVoxel(x,y+1,z).sDistF;
        float sDistF100 = chunk.GetVoxel(x+1,y,z).sDistF;
        float sDistF110 = chunk.GetVoxel(x+1,y+1,z).sDistF;
        float sDistF011 = chunk.GetVoxel(x,y+1,z+1).sDistF;
        float sDistF101 = chunk.GetVoxel(x+1,y,z+1).sDistF;
        float sDistF111 = chunk.GetVoxel(x+1,y+1,z+1).sDistF;
        float normX = (sDistF100-sDistF000) + (sDistF101-sDistF001) + (sDistF110-sDistF010) + (sDistF111-sDistF011);
        float normY = (sDistF010-sDistF000) + (sDistF011-sDistF001) + (sDistF110-sDistF100) + (sDistF111-sDistF101);
        float normZ = (sDistF001-sDistF000) + (sDistF101-sDistF100) + (sDistF011-sDistF010) + (sDistF111-sDistF110);
        return new Vector3(normX,normY,normZ);
    }

    public static bool SameSignsDistF(Voxel v1, Voxel v2){
        
        return ((v1.sDistF >= 0 && v2.sDistF >= 0)  || (v1.sDistF < 0 && v2.sDistF < 0));
        
    }

    public struct Tile {public int x; public int y;}

    public virtual Tile TexturePosition(){
        Tile tile = new Tile();
        tile.x = 0;
        tile.y = 2;
        return tile;
    }

    public virtual Vector2[] FaceUVs(){
        Vector2[] UVs = new Vector2[4];
        Tile tilePos = TexturePosition();
        var tileSizey = 0.249f;
    //  UVs[0] = new Vector2(tileSizex*tilePos.x+tileSizex,tileSizey*(tilePos.y+0.01f));
    //  UVs[1] = new Vector2(tileSizex*tilePos.x+tileSizex,tileSizey*(tilePos.y+0.01f)+tileSizey);
    //  UVs[2] = new Vector2(tileSizex*tilePos.x,tileSizey*(tilePos.y+0.01f)+tileSizey);
    //  UVs[3] = new Vector2(tileSizex*tilePos.x,tileSizey*(tilePos.y+0.01f));


        UVs[0] = new Vector2(tileSizex*tilePos.x,tileSizey*(tilePos.y+0.01f));
        UVs[1] = new Vector2(tileSizex*tilePos.x,tileSizey*(tilePos.y+0.01f));
        UVs[2] = new Vector2(tileSizex*tilePos.x,tileSizey*(tilePos.y+0.01f));
        UVs[3] = new Vector2(tileSizex*tilePos.x,tileSizey*(tilePos.y+0.01f));
        return UVs;
    }

    public uint GetID(){
        return id;
    }

    public static bool Equals(Voxel v1, Voxel v2){
        if(v1.GetID() == v2.GetID() && v1.sDistF == v2.sDistF){
            return true;
        }
        else{
            return false;
        }
    }

}
