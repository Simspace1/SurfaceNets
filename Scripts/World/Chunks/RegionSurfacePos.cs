using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionSurfacePos 
{
    public RegionPos regionPos {get; private set;}
    public bool surface {get; private set;}

    public RegionSurfacePos(RegionPos pos, bool surface){
        this.regionPos = pos;
        this.surface = surface;
    }

    public void SetSurface(bool surface){
        this.surface = surface;
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        //
        // See the full list of guidelines at
        //   http://go.microsoft.com/fwlink/?LinkID=85237
        // and also the guidance for operator== at
        //   http://go.microsoft.com/fwlink/?LinkId=85238
        //
        
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        RegionSurfacePos objPos = (RegionSurfacePos) obj;
        if(regionPos.Equals(objPos.regionPos) && surface == objPos.surface){
            return true;
        }
        else{
            return false;
        }
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        int hash = regionPos.GetHashCode();
        if(surface){
            hash *= 26;
        }
        return hash;
    }
}
