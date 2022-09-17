using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EditTerrain 
{

    //Gets the nearest voxel from vector3
    public static WorldPos GetVoxelPos(Vector3 pos){
        float multiple = Chunk.voxelSize;
        WorldPos voxelPos = new WorldPos(
            Mathf.RoundToInt(pos.x/multiple)*multiple,
            Mathf.RoundToInt(pos.y/multiple)*multiple,
            Mathf.RoundToInt(pos.z/multiple)*multiple
        );
        return voxelPos;
    }

    //Gets the voxel where vector 3 is in
    public static WorldPos GetVoxelPosIn(Vector3 pos){
        float multiple = Chunk.voxelSize;
        WorldPos voxelPos = new WorldPos(
            Mathf.FloorToInt(pos.x/multiple)*multiple,
            Mathf.FloorToInt(pos.y/multiple)*multiple,
            Mathf.FloorToInt(pos.z/multiple)*multiple
        );
        return voxelPos;
    }

    //Gets the nearest voxel from raycast
    public static WorldPos GetVoxelPos(RaycastHit hit){
        Vector3 pos = new Vector3(hit.point.x, hit.point.y,hit.point.z);
        return GetVoxelPos(pos);
    }

    // //sets voxel at nearset location to raycast to voxel
    // public static bool SetVoxel(RaycastHit hit, Voxel voxel){
    //     Chunk chunk = hit.collider.GetComponent<Chunk>();
    //     if(chunk == null)
    //         return false;
        
    //     WorldPos pos = GetVoxelPos(hit);

    //     chunk.world.SetVoxel(pos.x,pos.y,pos.z,voxel);
    //     return true;
    // }


    //Gets voxel at nearest location to raycast
    public static Voxel GetVoxel(RaycastHit hit){
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if(chunk == null)
            return null;
        
        WorldPos pos = GetVoxelPos(hit);

        Voxel voxel = chunk.world.GetVoxel(pos.x,pos.y,pos.z);
        return voxel;
    }


    public static bool AddVoxelSphere1(RaycastHit hit, float radius,Voxel voxel, bool add){
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if(chunk == null)
            return false;

        Vector3 posHit = new Vector3(hit.point.x, hit.point.y,hit.point.z);

        WorldPos pos = null;
        Voxel curVoxel = null;
        float sDistf;
        if (add){
            for (float xi = posHit.x - (radius+Chunk.voxelSize*5); xi <posHit.x + (radius+Chunk.voxelSize*5); xi += Chunk.voxelSize){
                for(float yi = posHit.y - (radius+Chunk.voxelSize*5); yi <posHit.y + (radius+Chunk.voxelSize*5); yi += Chunk.voxelSize){
                    for(float zi = posHit.z - (radius+Chunk.voxelSize*5); zi <posHit.z + (radius+Chunk.voxelSize*5); zi += Chunk.voxelSize){
                        //Gets current voxel location and voxel
                        pos = GetVoxelPosIn(new Vector3(xi,yi,zi));
                        curVoxel = chunk.world.GetVoxel(pos.x, pos.y, pos.z);

                        //Calculates sDistf
                        sDistf = Distance(posHit,pos)-radius;

                        if(curVoxel.sDistF >= 0 ){
                            if(sDistf < 0){
                                voxel = new Voxel();
                                voxel.sDistF = sDistf;
                                chunk.world.SetVoxel(pos, voxel);
                            }
                                                           
                        }
                        else if(sDistf < curVoxel.sDistF){
                            chunk.world.SetVoxel(pos, curVoxel);
                        } 
                        // else{

                        // }
                        
                    }
                }
            }

        }
        else{
            for (float xi = posHit.x - (radius+Chunk.voxelSize); xi <posHit.x + (radius+Chunk.voxelSize); xi += Chunk.voxelSize){
                for(float yi = posHit.y - (radius+Chunk.voxelSize); yi <posHit.y + (radius+Chunk.voxelSize); yi += Chunk.voxelSize){
                    for(float zi = posHit.z - (radius+Chunk.voxelSize); zi <posHit.z + (radius+Chunk.voxelSize); zi += Chunk.voxelSize){
                        //Gets current voxel location and voxel
                        pos = GetVoxelPosIn(new Vector3(xi,yi,zi));
                        curVoxel = chunk.world.GetVoxel(pos.x, pos.y, pos.z);

                        //Calculates sDistf
                        sDistf = -(Distance(posHit,pos)-radius);

                        if(curVoxel.sDistF < 0 ){
                            if(sDistf >= 0){
                                voxel = new Voxel();
                                voxel.sDistF = sDistf;
                                chunk.world.SetVoxel(pos, voxel);
                            }
                                                    
                        }
                        else if(sDistf > curVoxel.sDistF){
                            chunk.world.SetVoxel(pos, curVoxel);
                        } 
                        // else{

                        // }
                        
                    }
                }
            }
        }
        return true;
    }

    public static bool AddVoxelSphere(RaycastHit hit, float radius,Voxel voxel, bool add){
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if(chunk == null)
            return false;

        Vector3 posHit = new Vector3(hit.point.x, hit.point.y,hit.point.z);

        WorldPos pos = null;
        Voxel curVoxel = null;
        float sDistf;
        WorldPos worldPosHit = GetVoxelPosIn(posHit);
        float delim = radius + Chunk.voxelSize*2;

        if(add){
            for(float x = worldPosHit.x - delim; x < worldPosHit.x + delim; x += Chunk.voxelSize){
                for(float y = worldPosHit.y - delim; y < worldPosHit.y + delim; y += Chunk.voxelSize){
                    for(float z = worldPosHit.z - delim; z < worldPosHit.z + delim; z += Chunk.voxelSize){
                        pos = new WorldPos(x,y,z);
                        curVoxel = chunk.world.GetVoxel(pos);
                        
                        sDistf = Distance(posHit,pos)-radius;
                        if(curVoxel.sDistF >= 0 && sDistf < 0){
                            voxel = new Voxel();
                            voxel.sDistF = sDistf;
                            chunk.world.SetVoxel(pos, voxel);
                        }
                        else if(curVoxel.sDistF > sDistf){
                            chunk.world.SetSDistF(pos, voxel, sDistf);
                        }
                        // else if(){
                        //     curVoxel.sDistF = sDistf;
                        // }
                    }
                }
            }
        }
        else{
            for(float x = worldPosHit.x - delim; x < worldPosHit.x + delim; x += Chunk.voxelSize){
                for(float y = worldPosHit.y - delim; y < worldPosHit.y + delim; y += Chunk.voxelSize){
                    for(float z = worldPosHit.z - delim; z < worldPosHit.z + delim; z += Chunk.voxelSize){
                        pos = new WorldPos(x,y,z);
                        curVoxel = chunk.world.GetVoxel(pos);
                        
                        sDistf = -(Distance(posHit,pos)-radius);
                        if(curVoxel.sDistF < 0 && sDistf >= 0){
                            voxel = new Voxel();
                            voxel.sDistF = sDistf;
                            chunk.world.SetVoxel(pos, voxel);
                        }
                        else if(curVoxel.sDistF < sDistf){
                            chunk.world.SetSDistF(pos, voxel, sDistf);
                        }
                    }
                }
            }

        }

        return true;
    }

    public static float Distance(Vector3 posHit, WorldPos pos){
        return Mathf.Sqrt(Mathf.Pow(pos.x-posHit.x,2) + Mathf.Pow(pos.y- posHit.y,2) + Mathf.Pow(pos.z-posHit.z,2));
    }


    public static void GetLocation(RaycastHit hit){
        WorldPos pos = GetVoxelPos(hit);
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        Voxel voxel = chunk.world.GetVoxelDb(pos.x,pos.y,pos.z);
    }
    
}
