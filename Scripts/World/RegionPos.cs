using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionPos
{
    public int x {get; private set;}
    public int y {get; private set;}
    public int z {get; private set;}

    public RegionPos(int x, int y, int z){
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public WorldPos ToWorldPos(){
        return new WorldPos(x*RegionCol.regionVoxels,y*RegionCol.regionVoxels,z*RegionCol.regionVoxels);
    }

    public static bool Equals(RegionPos w1,RegionPos w2){
        // if(GetHashCode(w1) == GetHashCode(w2))
        if(w1.x == w2.x && w1.y == w2.y && w1.z == w2.z)
            return true;
        return false;
    }

    public bool Equals(RegionPos w2){
        // if(GetHashCode(w1) == GetHashCode(w2))
        if(this.x == w2.x && this.y == w2.y && this.z == w2.z)
            return true;
        return false;
    }

    public RegionPos GetColumn(){
        this.y = 0;
        return this;
    }

}

public class RegionPosEqualityComparer : IEqualityComparer<RegionPos>
{
    public bool Equals(RegionPos w1,RegionPos w2){
        // if(GetHashCode(w1) == GetHashCode(w2))
        if(w1.x == w2.x && w1.y == w2.y && w1.z == w2.z)
            return true;
        return false;
    }

    public int GetHashCode(RegionPos w1){
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

public class RegionPosColumnEqualityComparer : IEqualityComparer<RegionPos>
{
    public bool Equals(RegionPos w1,RegionPos w2){
        // if(GetHashCode(w1) == GetHashCode(w2))
        if(w1.x == w2.x && w1.z == w2.z)
            return true;
        return false;
    }

    public int GetHashCode(RegionPos w1){
        unchecked{
            int hash = 47;
            hash = hash * 227+w1.x.GetHashCode();
            hash = hash * 227+w1.z.GetHashCode();

            // hash = w1.x.GetHashCode() ^ w1.y.GetHashCode() ^ w1.z.GetHashCode();
            return hash;
        }
    }
}