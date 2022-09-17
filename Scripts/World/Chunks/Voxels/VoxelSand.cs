using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]

public class VoxelSand : Voxel
{
   public VoxelSand() : base(){

   }

    public VoxelSand(float sDistF) : base(){
       this.sDistF = sDistF;
   }

   public override Tile TexturePosition(){
       Tile tile = new Tile();
         tile.x = 0;
         tile.y = 0;
         return tile;
   }
}
