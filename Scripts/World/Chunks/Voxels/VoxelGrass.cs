using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]

public class VoxelGrass : Voxel
{
   public VoxelGrass() : base(){

   }

   public VoxelGrass(float sDistF) : base(){
      this.sDistF = sDistF;
   }

   public override Tile TexturePosition(){
       Tile tile = new Tile();
         tile.x = 0;
         tile.y = 3;
         return tile;
   }
}
