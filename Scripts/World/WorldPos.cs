using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class WorldPos 
{
    public float x {get; private set;}
    public float y {get; private set;}
    public float z {get; private set;}
    public int xi {get => Xi; private set => Xi = value;}
    public int yi {get => Yi; private set => Yi = value;}
    public int zi {get => Zi; private set => Zi = value;}

    [SerializeField]
    private int Xi;
    [SerializeField]
    private int Yi;
    [SerializeField]
    private int Zi;


    public WorldPos(float x, float y, float z){
        this.x = x;
        this.y = y;
        this.z = z;

        this.xi = Mathf.FloorToInt(x/Chunk.voxelSize);
        this.yi = Mathf.FloorToInt(y/Chunk.voxelSize);
        this.zi = Mathf.FloorToInt(z/Chunk.voxelSize);
    }

    public WorldPos(int xi, int yi, int zi){
        this.xi = xi;
        this.yi = yi;
        this.zi = zi;

        this.x = xi*Chunk.voxelSize;
        this.y = yi*Chunk.voxelSize;
        this.z = zi*Chunk.voxelSize;
    }

    public void SetPos(float x, float y, float z){
        this.x = x;
        this.y = y;
        this.z = z;

        this.xi = Mathf.FloorToInt(x/Chunk.voxelSize);
        this.yi = Mathf.FloorToInt(y/Chunk.voxelSize);
        this.zi = Mathf.FloorToInt(z/Chunk.voxelSize);
    }

    public void SetPos(int xi, int yi, int zi){
        this.xi = xi;
        this.yi = yi;
        this.zi = zi;

        this.x = xi*Chunk.voxelSize;
        this.y = yi*Chunk.voxelSize;
        this.z = zi*Chunk.voxelSize;
    }

    // public WorldPos NearestVoxel(){
    //     float x = Mathf.Round(this.x/Chunk.voxelSize)*Chunk.voxelSize;
    //     float y = Mathf.Round(this.y/Chunk.voxelSize)*Chunk.voxelSize;
    //     float z = Mathf.Round(this.z/Chunk.voxelSize)*Chunk.voxelSize;
    //     return new WorldPos(x,y,z);
    // }

    public static bool Equals(WorldPos w1,WorldPos w2){
        // if(GetHashCode(w1) == GetHashCode(w2))
        if(w1.x == w2.x && w1.y == w2.y && w1.z == w2.z)
            return true;
        return false;
    }

    public bool Equals(WorldPos w2){
        // if(GetHashCode(w1) == GetHashCode(w2))
        if(this.x == w2.x && this.y == w2.y && this.z == w2.z)
            return true;
        return false;
    }

    public override int GetHashCode(){
        unchecked{
            int hash = 47;
            hash = hash * 227+this.x.GetHashCode();
            hash = hash * 227+this.y.GetHashCode();
            hash = hash * 227+this.z.GetHashCode();
            return hash;
        }
    }

    public WorldPos NextX(){
        return new WorldPos(xi+1,yi,zi);
    }

    public WorldPos NextY(){
        return new WorldPos(xi,yi+1,zi);
    }  

    public WorldPos NextZ(){
        return new WorldPos(xi,yi,zi+1);
    }

    public WorldPos NextXY(){
        return new WorldPos(xi+1,yi+1,zi);
    }

    public WorldPos NextXZ(){
        return new WorldPos(xi+1,yi,zi+1);
    }

    public WorldPos NextYZ(){
        return new WorldPos(xi,yi+1,zi+1);
    } 

    public WorldPos NextXYZ(){
        return new WorldPos(xi+1,yi+1,zi+1);
    }

    public static WorldPos Add(WorldPos pos1, WorldPos pos2){
        return new WorldPos(pos1.xi+pos2.xi, pos1.yi+pos2.yi, pos1.zi+pos2.zi);
    }

    public static WorldPos Sub(WorldPos pos1, WorldPos pos2){
        return new WorldPos(pos1.xi-pos2.xi, pos1.yi-pos2.yi, pos1.zi-pos2.zi);
    }

    public static float Distance(WorldPos pos1, WorldPos pos2){
        return Mathf.Sqrt(Mathf.Pow(pos1.x-pos2.x,2) + Mathf.Pow(pos1.y-pos2.y,2) + Mathf.Pow(pos1.z-pos2.z,2));
    }

    public void FixfloatPos(){
        this.x = xi*Chunk.voxelSize;
        this.y = yi*Chunk.voxelSize;
        this.z = zi*Chunk.voxelSize;
    }

    public void FixIntPos(){
        this.xi = Mathf.FloorToInt(x/Chunk.voxelSize);
        this.yi = Mathf.FloorToInt(y/Chunk.voxelSize);
        this.zi = Mathf.FloorToInt(z/Chunk.voxelSize);
    }

    public RegionPos GetRegion(){
        return new RegionPos((xi+RegionCol.regionVoxels/2)/RegionCol.regionVoxels,(yi+RegionCol.regionVoxels/2)/RegionCol.regionVoxels,(zi+RegionCol.regionVoxels/2)/RegionCol.regionVoxels);
    }

    new public string ToString(){
        return "pos:"+x+"_"+y+"_"+z;
    }

    public string ToIntString(){
        return "posI:"+xi+"_"+yi+"_"+zi;
    }

    public WorldPos ToColumn(){
        return new WorldPos(xi,0,zi);
    }
    
}

public class WorldPosEqualityComparer : IEqualityComparer<WorldPos>
{
    public bool Equals(WorldPos w1,WorldPos w2){
        // if(GetHashCode(w1) == GetHashCode(w2))
        if(w1.x == w2.x && w1.y == w2.y && w1.z == w2.z)
            return true;
        return false;
    }

    public int GetHashCode(WorldPos w1){
        unchecked{
            int hash = 47;
            hash = hash * 227+w1.x.GetHashCode();
            hash = hash * 227+w1.y.GetHashCode();
            hash = hash * 227+w1.z.GetHashCode();

            // hash = w1.x.GetHashCode() ^ w1.y.GetHashCode() ^ w1.z.GetHashCode();
            return hash;
        }
    }
}
