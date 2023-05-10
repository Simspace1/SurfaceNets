using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]

public class VoxelGrass : Voxel
{
   private static uint BlockID = 3;
   public VoxelGrass() : base(){
      this.id = BlockID;
   }

   public VoxelGrass(float sDistF) : base(sDistF){
      // this.sDistF = sDistF;
      this.id = BlockID;
   }

   public override Tile TexturePosition(){
       Tile tile = new Tile();
         tile.x = 0;
         tile.y = 3;
         return tile;
   }
}
