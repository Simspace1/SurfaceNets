using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]

public class VoxelAir : Voxel
{
   private static uint BlockID = 0;

   public VoxelAir() : base(){
      this.air = true;
      this.id = BlockID;
   }

   public VoxelAir(float sDistF) : base(sDistF){
      this.air = true;
      // this.sDistF = sDistF;
      this.id = BlockID;
   }

//    public override Tile TexturePosition(){
//        Tile tile = new Tile();
//          tile.x = 0;
//          tile.y = 0;
//          return tile;
//    }
}
