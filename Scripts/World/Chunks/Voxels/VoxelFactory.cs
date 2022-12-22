using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelFactory
{
    
    public static Voxel Create(string input){
        switch(input){
            case "air":
                return new VoxelAir();
            case "stone":
                return new Voxel();
            case "grass":
                return new VoxelGrass();


            default:
                return new Voxel();
        }
    }

    public static Voxel Create(int id, float sDist){
        switch(id){
            case 0:
                return new VoxelAir(sDist);
            case 1:
                return new Voxel(sDist);
            case 3:
                return new VoxelGrass(sDist);
            

            default:
                return new Voxel();
        }
    }

    public static Voxel Create(Voxel voxel){
        float sDist = voxel.sDistF;
        switch(voxel.GetID()){
            case 0:
                return new VoxelAir(sDist);
            case 1:
                return new Voxel(sDist);
            case 3:
                return new VoxelGrass(sDist);
            

            default:
                return new Voxel();
        }
    }
}
