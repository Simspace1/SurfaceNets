using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]

public class VoxelSnow : Voxel
{
   public VoxelSnow() : base(){

   }

   public VoxelSnow(float sDistF) : base(){
      this.sDistF = sDistF;
   }

   public override Tile TexturePosition(){
       Tile tile = new Tile();
         tile.x = 0;
         tile.y = 1;
         return tile;
   }
}
