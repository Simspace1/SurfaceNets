using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimplexNoise;

[System.Serializable]
public class TerrainGen
{
    ulong seed;
    
    public bool generated = false;
    public float [] minMax;


    float stoneBaseHeight = 0;
    float stoneBaseNoise = 0.005f;
    float stoneBaseNoiseHeight = 4;

    float MountainsBiomeFrequency = 0.000000000000000000000000000000000000000000000000000000005f;
    float MountainsBiomeSize = 5;
    float MountainsBiomeAmplitureMultiplier = 1;

    float stoneMountainFrequency = 0.008f;

    float dirtBaseHeight = 1;
    float dirtNoise = 0.04f;
    float dirtNoiseHeight = 3;



    float[,] terrainHeight = new float[Chunk.chunkVoxels+3,Chunk.chunkVoxels+3];
    float[,] stoneHeight = new float[Chunk.chunkVoxels+3,Chunk.chunkVoxels+3];

    // public Chunk ChunkGen(Chunk chunk){
    //     TerrainHeight(chunk);
    //     for (float x = chunk.pos.x ; x<chunk.pos.x+Chunk.chunkSize; x += Chunk.voxelSize){
    //         for (float z = chunk.pos.z ; z<chunk.pos.z+Chunk.chunkSize; z += Chunk.voxelSize){
                
    //             chunk = ChunkColumnGen(chunk,x,z);
    //         }
    //     }
    //     return chunk;
    // }

    // public Chunk ChunkGenC(Chunk chunk){
    //     for (float x = chunk.pos.x ; x<chunk.pos.x+Chunk.chunkSize; x += Chunk.voxelSize){
    //         for (float z = chunk.pos.z ; z<chunk.pos.z+Chunk.chunkSize; z += Chunk.voxelSize){
    //             chunk = ChunkColumnGen(chunk,x,z);
    //         }
    //     }
    //     return chunk;
    // }


    //Int version of Chunk Gen
    public Chunk ChunkGenC2(Chunk chunk){
        for (int x = chunk.pos.xi ; x<chunk.pos.xi+Chunk.chunkVoxels; x++){
            for (int z = chunk.pos.zi ; z<chunk.pos.zi+Chunk.chunkVoxels; z++){
                chunk = ChunkColumnGen(chunk,x,z);
            }
        }
        return chunk;
    }



    // public Chunk ChunkColumnGen(Chunk chunk, float x, float z){
    //     // int xi = Mathf.FloorToInt((x-(chunk.pos.x-Chunk.voxelSize))/Chunk.voxelSize);
    //     // int zi = Mathf.FloorToInt((z-(chunk.pos.z-Chunk.voxelSize))/Chunk.voxelSize);

    //     // float stoneheight = stoneHeight[xi,zi];
    //     // // float MountainsBiome = GetNoise(x,0,z,MountainsBiomeFrequency,MountainsBiomeSize);
    //     // float dirtHeight = terrainHeight[xi,zi];

    //     for(float y = chunk.pos.y+Chunk.chunkSize; y>=chunk.pos.y; y -= Chunk.voxelSize){
    //         float sDistF = sDistFGen(chunk,x,y,z);

    //         if (sDistF >0){
    //             SetVoxel(x,y,z,new VoxelAir(sDistF),chunk);
    //         }
    //         else{
    //             SetVoxel(x,y,z,new Voxel(sDistF),chunk);
    //         }
    //     }
    //     return chunk;

    // }

    //Int version of Chunk Column gen
    private Chunk ChunkColumnGen(Chunk chunk, int xi, int zi){

        // float stoneheight = stoneHeight[xi,zi];
        // // float MountainsBiome = GetNoise(x,0,z,MountainsBiomeFrequency,MountainsBiomeSize);
        // float dirtHeight = terrainHeight[xi,zi];

        for(int yi = chunk.pos.yi+Chunk.chunkVoxels; yi>=chunk.pos.yi; yi--){
            float sDistF = sDistFGen3(chunk,xi,yi,zi);

            if (sDistF >0){
                SetVoxel(xi,yi,zi,new VoxelAir(sDistF),chunk,sDistF);
            }
            else{
                if(chunk.pos.y >= 8){
                     SetVoxel(xi,yi,zi,new VoxelGrass(sDistF),chunk,sDistF);
                }
                SetVoxel(xi,yi,zi,new Voxel(sDistF),chunk,sDistF);
            }
        }
        return chunk;

    }

    // public float sDistFGen(Chunk chunk, float x, float y, float z){
    //     float sDistF = 0;
    //     int xi = Mathf.FloorToInt((x-(chunk.pos.x-Chunk.voxelSize))/Chunk.voxelSize);
    //     int zi = Mathf.FloorToInt((z-(chunk.pos.z-Chunk.voxelSize))/Chunk.voxelSize);
    //     int yi = Mathf.FloorToInt((y-(chunk.pos.y))/Chunk.voxelSize);

    //     if(SameSignHeight(y,xi,zi)){
    //         sDistF = y-terrainHeight[xi,zi];
    //     }
    //     else{
    //         List<SurfPt> edgePts= new List<SurfPt>();
    //         // SurfPt surfPt0;

    //         if( y <= terrainHeight[xi,zi]+Chunk.voxelSize && y >= terrainHeight[xi,zi]-Chunk.voxelSize){
    //             edgePts.Add(new SurfPt(0,terrainHeight[xi,zi]-y,0));
    //         }
    //         if(!SameSignHeight(y,xi,zi,xi+1,zi)){
    //             edgePts.Add(Intercept(y,xi,zi,xi+1,zi));
    //         }
    //         // else{
    //         //     surfPt0 = Intercept(y,xi,zi,xi+1,zi);
    //         //     if(Mathf.Sqrt(Mathf.Pow(surfPt0.y,2)+Mathf.Pow(surfPt0.x,2)+Mathf.Pow(surfPt0.z,2)) < y-terrainHeight[xi,zi]){
    //         //         edgePts.Add(surfPt0);
    //         //     }
    //         // }
    //         if(!SameSignHeight(y,xi,zi,xi,zi+1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi,zi+1));
    //         }
    //         // else{
    //         //     surfPt0 = Intercept(y,xi,zi,xi,zi+1);
    //         //     if(Mathf.Sqrt(Mathf.Pow(surfPt0.y,2)+Mathf.Pow(surfPt0.x,2)+Mathf.Pow(surfPt0.z,2)) < y-terrainHeight[xi,zi]){
    //         //         edgePts.Add(surfPt0);
    //         //     }
    //         // }
    //         if(!SameSignHeight(y,xi,zi,xi-1,zi)){
    //             edgePts.Add(Intercept(y,xi,zi,xi-1,zi));
    //         }
    //         // else{
    //         //     surfPt0 = Intercept(y,xi,zi,xi-1,zi);
    //         //     if(Mathf.Sqrt(Mathf.Pow(surfPt0.y,2)+Mathf.Pow(surfPt0.x,2)+Mathf.Pow(surfPt0.z,2)) < y-terrainHeight[xi,zi]){
    //         //         edgePts.Add(surfPt0);
    //         //     }
    //         // }
    //         if(!SameSignHeight(y,xi,zi,xi,zi-1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi,zi-1));
    //         }
    //         // else{
    //         //     surfPt0 = Intercept(y,xi,zi,xi,zi-1);
    //         //     if(Mathf.Sqrt(Mathf.Pow(surfPt0.y,2)+Mathf.Pow(surfPt0.x,2)+Mathf.Pow(surfPt0.z,2)) < y-terrainHeight[xi,zi]){
    //         //         edgePts.Add(surfPt0);
    //         //     }
    //         // }

    //         int edgePtN = edgePts.Count;
    //         SurfPt surfPt = new SurfPt(0,0,0);
    //         for(int i = 0; i < edgePtN; i++){
    //             surfPt.Add(edgePts[0]);
    //             edgePts.RemoveAt(0);
    //         }
    //         surfPt.Divide(edgePtN);
    //         sDistF = Mathf.Sqrt(Mathf.Pow(surfPt.y,2)+Mathf.Pow(surfPt.x,2)+Mathf.Pow(surfPt.z,2));
    //         if (y < terrainHeight[xi,zi]){
    //             sDistF = -sDistF;
    //         }
    //     }
    //     return sDistF;
    // }
    
    //Int inputs version
    // public float sDistFGen(Chunk chunk, int x, int yi, int z){
    //     float sDistF = 0;
    //     int xi = x-(chunk.pos.xi-1);
    //     int zi = z-(chunk.pos.zi-1);
    //     // int yi = Mathf.FloorToInt((y-(chunk.pos.y))/Chunk.voxelSize);

    //     float y = yi*Chunk.voxelSize;

    //     if(SameSignHeight(y,xi,zi)){
    //         sDistF = y-terrainHeight[xi,zi];
    //     }
    //     else{
    //         List<SurfPt> edgePts= new List<SurfPt>();
    //         // SurfPt surfPt0;

    //         if( y <= terrainHeight[xi,zi]+Chunk.voxelSize && y >= terrainHeight[xi,zi]-Chunk.voxelSize){
    //             edgePts.Add(new SurfPt(0,terrainHeight[xi,zi]-y,0));
    //         }
    //         if(!SameSignHeight(y,xi,zi,xi+1,zi)){
    //             edgePts.Add(Intercept(y,xi,zi,xi+1,zi));
    //         }
    //         // else{
    //         //     surfPt0 = Intercept(y,xi,zi,xi+1,zi);
    //         //     if(Mathf.Sqrt(Mathf.Pow(surfPt0.y,2)+Mathf.Pow(surfPt0.x,2)+Mathf.Pow(surfPt0.z,2)) < y-terrainHeight[xi,zi]){
    //         //         edgePts.Add(surfPt0);
    //         //     }
    //         // }
    //         if(!SameSignHeight(y,xi,zi,xi,zi+1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi,zi+1));
    //         }
    //         // else{
    //         //     surfPt0 = Intercept(y,xi,zi,xi,zi+1);
    //         //     if(Mathf.Sqrt(Mathf.Pow(surfPt0.y,2)+Mathf.Pow(surfPt0.x,2)+Mathf.Pow(surfPt0.z,2)) < y-terrainHeight[xi,zi]){
    //         //         edgePts.Add(surfPt0);
    //         //     }
    //         // }
    //         if(!SameSignHeight(y,xi,zi,xi-1,zi)){
    //             edgePts.Add(Intercept(y,xi,zi,xi-1,zi));
    //         }
    //         // else{
    //         //     surfPt0 = Intercept(y,xi,zi,xi-1,zi);
    //         //     if(Mathf.Sqrt(Mathf.Pow(surfPt0.y,2)+Mathf.Pow(surfPt0.x,2)+Mathf.Pow(surfPt0.z,2)) < y-terrainHeight[xi,zi]){
    //         //         edgePts.Add(surfPt0);
    //         //     }
    //         // }
    //         if(!SameSignHeight(y,xi,zi,xi,zi-1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi,zi-1));
    //         }
    //         // else{
    //         //     surfPt0 = Intercept(y,xi,zi,xi,zi-1);
    //         //     if(Mathf.Sqrt(Mathf.Pow(surfPt0.y,2)+Mathf.Pow(surfPt0.x,2)+Mathf.Pow(surfPt0.z,2)) < y-terrainHeight[xi,zi]){
    //         //         edgePts.Add(surfPt0);
    //         //     }
    //         // }

    //         int edgePtN = edgePts.Count;
    //         SurfPt surfPt = new SurfPt(0,0,0);
    //         for(int i = 0; i < edgePtN; i++){
    //             surfPt.Add(edgePts[0]);
    //             edgePts.RemoveAt(0);
    //         }
    //         surfPt.Divide(edgePtN);
    //         sDistF = Mathf.Sqrt(Mathf.Pow(surfPt.y,2)+Mathf.Pow(surfPt.x,2)+Mathf.Pow(surfPt.z,2));
    //         if (y < terrainHeight[xi,zi]){
    //             sDistF = -sDistF;
    //         }
    //     }
    //     return sDistF;
    // }

    //Int inputs version
    // public float sDistFGen1(Chunk chunk, int x, int yi, int z){
    //     float sDistF = 0;
    //     int xi = x-(chunk.pos.xi-1);
    //     int zi = z-(chunk.pos.zi-1);
    //     // int yi = Mathf.FloorToInt((y-(chunk.pos.y))/Chunk.voxelSize);

    //     float y = yi*Chunk.voxelSize;

    //     if(SameSignHeight1(y,xi,zi)){
    //         sDistF = y-terrainHeight[xi,zi];
    //     }
    //     else{
    //         List<SurfPt> edgePts= new List<SurfPt>();
    //         // SurfPt surfPt0;

    //         if( y <= terrainHeight[xi,zi]+Chunk.voxelSize && y >= terrainHeight[xi,zi]-Chunk.voxelSize){
    //             edgePts.Add(new SurfPt(0,terrainHeight[xi,zi]-y,0));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi+1,zi)){
    //             edgePts.Add(Intercept(y,xi,zi,xi+1,zi));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi,zi+1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi,zi+1));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi-1,zi)){
    //             edgePts.Add(Intercept(y,xi,zi,xi-1,zi));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi,zi-1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi,zi-1));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi-1,zi-1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi-1,zi-1));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi+1,zi-1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi+1,zi-1));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi-1,zi+1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi-1,zi+1));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi+1,zi+1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi+1,zi+1));
    //         }

    //         float min = 2;
    //         float sDist;
    //         foreach(SurfPt pt in edgePts){
    //             sDist = Mathf.Sqrt(Mathf.Pow(pt.y,2)+Mathf.Pow(pt.x,2)+Mathf.Pow(pt.z,2));
    //             if(Mathf.Abs(sDist) < min){
    //                 min = sDist;
    //             }
    //         }

    //         // int edgePtN = edgePts.Count;
    //         // SurfPt surfPt = new SurfPt(0,0,0);
    //         // for(int i = 0; i < edgePtN; i++){
    //         //     surfPt.Add(edgePts[0]);
    //         //     edgePts.RemoveAt(0);
    //         // }
    //         // surfPt.Divide(edgePtN);
    //         // sDistF = Mathf.Sqrt(Mathf.Pow(surfPt.y,2)+Mathf.Pow(surfPt.x,2)+Mathf.Pow(surfPt.z,2));

    //         // if(sDistF > min){
    //         //     sDistF = min;
    //         // }
            
    //         sDistF = min;
    //         if (y < terrainHeight[xi,zi]){
    //             sDistF = -sDistF;
    //         }
    //     }
    //     return sDistF;
    // }

    //Int inputs version
    // public float sDistFGen2(Chunk chunk, int x, int yi, int z){
    //     float sDistF = 0;
    //     int xi = x-(chunk.pos.xi-1);
    //     int zi = z-(chunk.pos.zi-1);
    //     // int yi = Mathf.FloorToInt((y-(chunk.pos.y))/Chunk.voxelSize);

    //     float y = yi*Chunk.voxelSize;

    //     if(SameSignHeight1(y,xi,zi)){
    //         sDistF = y-terrainHeight[xi,zi];
    //     }
    //     else{
    //         List<SurfPt> edgePts= new List<SurfPt>();
    //         // SurfPt surfPt0;

    //         // if( y <= terrainHeight[xi,zi]+Chunk.voxelSize && y >= terrainHeight[xi,zi]-Chunk.voxelSize){
    //             edgePts.Add(new SurfPt(0,Mathf.Abs(y-terrainHeight[xi,zi]),0));
    //         // }
    //         if(!SameSignHeight1(y,xi,zi,xi+1,zi)){
    //             edgePts.Add(Intercept(y,xi,zi,xi+1,zi));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi,zi+1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi,zi+1));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi-1,zi)){
    //             edgePts.Add(Intercept(y,xi,zi,xi-1,zi));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi,zi-1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi,zi-1));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi-1,zi-1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi-1,zi-1));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi+1,zi-1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi+1,zi-1));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi-1,zi+1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi-1,zi+1));
    //         }
    //         if(!SameSignHeight1(y,xi,zi,xi+1,zi+1)){
    //             edgePts.Add(Intercept(y,xi,zi,xi+1,zi+1));
    //         }

    //         float min = 1000;
    //         float sDist;
    //         foreach(SurfPt pt in edgePts){
    //             sDist = Mathf.Sqrt(Mathf.Pow(pt.y,2)+Mathf.Pow(pt.x,2)+Mathf.Pow(pt.z,2));
    //             if(Mathf.Abs(sDist) < min){
    //                 min = sDist;
    //             }
    //         }

    //         // int edgePtN = edgePts.Count;
    //         // SurfPt surfPt = new SurfPt(0,0,0);
    //         // for(int i = 0; i < edgePtN; i++){
    //         //     surfPt.Add(edgePts[0]);
    //         //     edgePts.RemoveAt(0);
    //         // }
    //         // surfPt.Divide(edgePtN);
    //         // sDistF = Mathf.Sqrt(Mathf.Pow(surfPt.y,2)+Mathf.Pow(surfPt.x,2)+Mathf.Pow(surfPt.z,2));

    //         // if(sDistF > min){
    //         //     sDistF = min;
    //         // }
            
    //         sDistF = min;
    //         if (y < terrainHeight[xi,zi]){
    //             sDistF = -sDistF;
    //         }
    //     }
    //     return sDistF;
    // }

    

    private float sDistFGen3(Chunk chunk, int x, int yi, int z){
        float sDistF = 0;
        int xi = x-(chunk.pos.xi-1);
        int zi = z-(chunk.pos.zi-1);

        float y = yi*Chunk.voxelSize;        

        bool posit = y >=terrainHeight[xi,zi];
        float height = terrainHeight[xi,zi];
        float offset = Chunk.voxelSize/2;

        if(!HeightCheck(y,xi,zi,posit,height,offset)){
            sDistF = y - terrainHeight[xi,zi];
        }
        else{
            List<float> dists= new List<float>();
            float dely = Mathf.Abs(y - terrainHeight[xi,zi]);
            
            for(int xj = xi-1; xj<= xi+1; xj++){
                for(int zj = zi-1; zj<=zi+1; zj++){
                    if(xj == xi && zj == zi){
                        dists.Add(Mathf.Abs(y-terrainHeight[xi,zi]));
                        continue;
                    }

                    // if(SingleHeightCheck(y,xj,zj,posit,height,offset)){
                        dists.Add(Intercept2(dely, y,xi,zi,xj,zj));
                    // }
                }
            }

            float min = 1000;
            foreach(float dist in dists){
                if(Mathf.Abs(dist) < min){
                    min = dist;
                }
            }
 
            sDistF = min;
            if (!posit){
                sDistF = -sDistF;
            }
        }

        return sDistF;
    }

    // returns true if surounding voxels need to be calculated for sDist generation
    private bool HeightCheck(float y, int xi, int zi,bool posit, float height, float offset){
        bool val = false;

        for(int x = xi-1; x<= xi+1; x++){
            for(int z = zi-1; z<=zi+1; z++){
                if(x == xi && z == zi){
                    continue;
                }

                // checks if y is above or under terrain height
                if(posit){
                    if(terrainHeight[x,z]-offset >= height && y-terrainHeight[x,z] <= Chunk.sDistLimit){
                        val = true;
                    }
                }
                else{
                    if(terrainHeight[x,z]+offset <= height && terrainHeight[x,z]-y <= Chunk.sDistLimit){
                        val = true;
                    }
                }
            }

        }
        return val;
    }

    //checks a single position to verify if it needs to be calculated for sDist
    // bool SingleHeightCheck(float y, int x, int z, bool posit, float height, float offset){
    //     bool val;
        
    //     if(posit){
    //         val = terrainHeight[x,z]-offset >= height && y-terrainHeight[x,z] <= Chunk.sDistLimit;
    //     }
    //     else{
    //         val = terrainHeight[x,z]+offset <= height && terrainHeight[x,z]-y <= Chunk.sDistLimit;
    //     }

    //     return val;
    // }

    private float Intercept2(float delyi, float y, int xi, int zi, int x, int z){
        float d, theta, delx, dely;
        
        dely = Mathf.Abs(terrainHeight[x,z] - terrainHeight[xi,zi]);
        if(x == xi){
            delx = Mathf.Abs(zi-z)*Chunk.voxelSize;
        }
        else if(z == zi){
            delx = Mathf.Abs(xi-x)*Chunk.voxelSize;
        }
        else{
            delx = Mathf.Sqrt(Mathf.Pow((xi-x)*Chunk.voxelSize, 2) + Mathf.Pow((zi-z)*Chunk.voxelSize, 2));
        }

        theta = Mathf.Atan2(delx,dely);
        d = Mathf.Sin(theta) * delyi;

        // float temp = Dist(y,xi,zi,x,z);
        // if(d > temp){
        //     d = temp;
        // }
        return d;
    }

    // float Dist(float y,int xi, int zi, int x, int z){
    //     return Mathf.Sqrt(Mathf.Pow((xi-x)*Chunk.voxelSize, 2) + Mathf.Pow((zi-z)*Chunk.voxelSize, 2) + Mathf.Pow((y-terrainHeight[x,z])*Chunk.voxelSize, 2));
    // }

    // SurfPt Intercept(float y, int xi, int zi, int xii, int zii){
    //     float slope;
    //     float b;
    //     float xint,zint;
    //     if (zi == zii){
    //         slope = (terrainHeight[xii,zii]-terrainHeight[xi,zi])/((xii-xi)*Chunk.voxelSize);
    //         b = terrainHeight[xi,zi];
    //         xint = (y-b)/slope;
    //         zint = 0;
    //     }
    //     else if(xi == xii){
    //         slope = (terrainHeight[xii,zii]-terrainHeight[xi,zi])/((zii-zi)*Chunk.voxelSize);
    //         b = terrainHeight[xi,zi];
    //         zint = (y-b)/slope;
    //         xint = 0;
    //     }
    //     // else{
    //     //     xint = 0;
    //     //     zint = 0;
    //     // }
    //     else{
    //         float xzi, xzii, xzint, mag;
    //         xzi = Mathf.Sqrt(Mathf.Pow(xi*Chunk.voxelSize,2) + Mathf.Pow(zi*Chunk.voxelSize,2));
    //         xzii = Mathf.Sqrt(Mathf.Pow(xii*Chunk.voxelSize,2) + Mathf.Pow(zii*Chunk.voxelSize,2));
    //         slope = (terrainHeight[xii,zii]-terrainHeight[xi,zi])/(xzii-xzi);
    //         b = terrainHeight[xi,zi];
    //         xzint = (y-b)/slope;
    //         mag = Mathf.Sqrt(Mathf.Pow(xzint,2)/2);
    //         if(xii > xi){
    //             xint = mag;
    //         }
    //         else{
    //             xint = -mag;
    //         }
            
    //         if(zii > zi){
    //             zint = mag;
    //         }
    //         else{
    //             zint = -mag;
    //         }
    //     }
    //     // else{
    //     //     slope = (terrainHeight[xii,zii]-terrainHeight[xi,zi])/(Mathf.Sqrt(Mathf.Pow(zii-zi,2)+Mathf.Pow(xii-xi,2))*Chunk.voxelSize);
    //     //     b = terrainHeight[xi,zi];
    //     //     inter = (y-b)/slope;
    //     //     zint = zi
    //     //     xint = xi;
    //     // }
        
    //     return new SurfPt(xint,y,zint);
    // }

    //Temp Method To generate Far voxels for correct textures
    public Voxel[,] FarVoxelGen(FarChunkCol col){
        Voxel[,] voxels = new Voxel[Chunk.chunkVoxels+2, Chunk.chunkVoxels+2];
        for(int xi = 0; xi <= Chunk.chunkVoxels+1; xi++){
            for(int zi = 0; zi <= Chunk.chunkVoxels+1; zi++){
                voxels[xi,zi] = new Voxel();
            }
        }
        return voxels;
    }

    //Returns required parts of TerrainHeight
    public float[,] FarHeightConversion(){
        float[,] heights = new float[Chunk.chunkVoxels+2,Chunk.chunkVoxels+2];
        for(int xi = 1; xi <= Chunk.chunkVoxels+2; xi++){
            for(int zi = 1; zi <= Chunk.chunkVoxels+2; zi++){
                heights[xi-1,zi-1] = terrainHeight[xi,zi];
            }
        }

        return heights;
    }

    public float[,] Terrain(){
        return terrainHeight;
    }

    // bool SameSignHeight(float y,int xi, int zi){
    //     bool sameSign = (y < terrainHeight[xi,zi] && y < terrainHeight[xi+1,zi] && y < terrainHeight[xi,zi+1] && y < terrainHeight[xi-1,zi] && y < terrainHeight[xi,zi-1]) || (y >= terrainHeight[xi,zi] && y >= terrainHeight[xi+1,zi] && y >= terrainHeight[xi,zi+1] && y >= terrainHeight[xi-1,zi] && y >= terrainHeight[xi,zi-1]);
    //     return sameSign;
    // }

    // bool SameSignHeight1(float y,int xi, int zi){
    //     bool sameSign = (y < terrainHeight[xi,zi]+Chunk.voxelSize && y < terrainHeight[xi+1,zi]+Chunk.voxelSize && y < terrainHeight[xi,zi+1]+Chunk.voxelSize && y < terrainHeight[xi-1,zi]+Chunk.voxelSize && y < terrainHeight[xi,zi-1]+Chunk.voxelSize && y < terrainHeight[xi-1,zi-1]+Chunk.voxelSize && y < terrainHeight[xi-1,zi+1]+Chunk.voxelSize && y < terrainHeight[xi+1,zi-1]+Chunk.voxelSize && y < terrainHeight[xi+1,zi+1]+Chunk.voxelSize) || (y >= terrainHeight[xi,zi]-Chunk.voxelSize && y >= terrainHeight[xi+1,zi]-Chunk.voxelSize && y >= terrainHeight[xi,zi+1]-Chunk.voxelSize && y >= terrainHeight[xi-1,zi] && y >= terrainHeight[xi,zi-1]-Chunk.voxelSize && y >= terrainHeight[xi-1,zi-1]-Chunk.voxelSize && y >= terrainHeight[xi+1,zi-1]-Chunk.voxelSize && y >= terrainHeight[xi+1,zi+1]-Chunk.voxelSize && y >= terrainHeight[xi-1,zi+1]-Chunk.voxelSize);
    //     return sameSign;
    // }

    // bool SameSignHeight2(float y, int xi, int zi){
    //     bool sameSign = (y < terrainHeight[xi,zi] ) || (y  >= terrainHeight[xi,zi]);
    //     return sameSign;
    // }

    // bool SameSignHeight(float y,int xi, int zi, int xii,int zii){
    //     bool sameSign = (y < terrainHeight[xi,zi] && y < terrainHeight[xii,zii]) || (y >= terrainHeight[xi,zi] && y>= terrainHeight[xii,zii]);
    //     return sameSign;
    // }

    // bool SameSignHeight1(float y,int xi, int zi, int xii,int zii){
    //     bool sameSign = (y < terrainHeight[xi,zi]+Chunk.voxelSize && y < terrainHeight[xii,zii]+Chunk.voxelSize) || (y >= terrainHeight[xi,zi]-Chunk.voxelSize && y>= terrainHeight[xii,zii]-Chunk.voxelSize);
    //     return sameSign;
    // }

    // public void TerrainHeight(Chunk chunk){
    //     // float stoneheight, MountainsBiome,dirtHeight;
    //     for (float x = chunk.pos.x-Chunk.voxelSize ; x<chunk.pos.x+Chunk.chunkSize+Chunk.voxelSize; x += Chunk.voxelSize){
    //         for (float z = chunk.pos.z-Chunk.voxelSize ; z<chunk.pos.z+Chunk.chunkSize+Chunk.voxelSize; z += Chunk.voxelSize){
    //             int xi = Mathf.FloorToInt((x-(chunk.pos.x-Chunk.voxelSize))/Chunk.voxelSize);
    //             int zi = Mathf.FloorToInt((z-(chunk.pos.z-Chunk.voxelSize))/Chunk.voxelSize);

    //             // stoneheight = stoneBaseHeight;
    //             // stoneheight += GetNoise(x,0,z,stoneBaseNoise,stoneBaseNoiseHeight);

    //             // MountainsBiome = GetNoise(x,0,z,MountainsBiomeFrequency,MountainsBiomeSize);
    //             // stoneheight += GetNoise(x,0,z,stoneMountainFrequency,MountainsBiome);

    //             // stoneHeight[xi,zi] = stoneheight;

    //             // dirtHeight = stoneheight + dirtBaseHeight;
    //             // dirtHeight += GetNoise(x,100,z,dirtNoise,dirtNoiseHeight);

    //             var val = GenerateHeights(x,z);

    //             stoneHeight[xi,zi] = val[0];

    //             terrainHeight[xi,zi] = val[1];

    //             // terrainHeight[xi,zi] = dirtHeight;
    //         }
    //     }
    // }

    public float[] MmTerrainHeight(WorldPos pos){
        // float stoneheight, MountainsBiome,dirtHeight;
        float min = 0;
        float max = 0;
        for (float x = pos.x-Chunk.voxelSize ; x<pos.x+Chunk.chunkSize+Chunk.voxelSize*2; x += Chunk.voxelSize){
            for (float z = pos.z-Chunk.voxelSize ; z<pos.z+Chunk.chunkSize+Chunk.voxelSize*2; z += Chunk.voxelSize){
                int xi = Mathf.FloorToInt((x-(pos.x-Chunk.voxelSize))/Chunk.voxelSize);
                int zi = Mathf.FloorToInt((z-(pos.z-Chunk.voxelSize))/Chunk.voxelSize);

                // stoneheight = stoneBaseHeight;
                // stoneheight += GetNoise(x,0,z,stoneBaseNoise,stoneBaseNoiseHeight);

                // MountainsBiome = GetNoise(x,0,z,MountainsBiomeFrequency,MountainsBiomeSize)*MountainsBiomeAmplitureMultiplier;
                // stoneheight += GetNoise(x,0,z,stoneMountainFrequency,MountainsBiome);

                // dirtHeight = stoneheight + dirtBaseHeight;
                // dirtHeight += GetNoise(x,100,z,dirtNoise,dirtNoiseHeight);

                var val = GenerateHeights(x,z);

                stoneHeight[xi,zi] = val[0];

                terrainHeight[xi,zi] = val[1];
                if (min == 0 && max == 0 && val[1] != 0){
                    min = val[1];
                    max = val[1];
                }
                else if(val[1] < min){
                    min = val[1];
                }
                else if(val[1] > max){
                    max = val[1];
                }

                // terrainHeight[xi,zi] = dirtHeight;
                // if (min == 0 && max == 0 && dirtHeight != 0){
                //     min = dirtHeight;
                //     max = dirtHeight;
                // }
                // else if(dirtHeight < min){
                //     min = dirtHeight;
                // }
                // else if(dirtHeight > max){
                //     max = dirtHeight;
                // }
            }
        }
        minMax = new float[2];
        minMax[0] = min;
        minMax[1] = max;
        generated = true;
        return minMax;
    }

    private float[] GenerateHeights(float x, float z){
        float stoneheight, dirtheight, MountainsBiome;
        stoneheight = 0;
        dirtheight = 0;

        stoneheight = stoneBaseHeight;
        stoneheight += GetNoise(x,0,z,stoneBaseNoise,stoneBaseNoiseHeight);


        MountainsBiome = GetNoise(x,0,z,MountainsBiomeFrequency,MountainsBiomeSize);
        MountainsBiome = GetNoise(x,0,z,0.05f,100);
        stoneheight += GetNoise(x,0,z,stoneMountainFrequency,MountainsBiome);


        dirtheight = stoneheight + dirtBaseHeight;
        dirtheight += GetNoise(x,100,z,dirtNoise,dirtNoiseHeight);


        float[] val = new float[2];
        val[0] = stoneheight;
        val[1] = dirtheight;
        return val;
    }

    public static float GetNoise(float x, float y, float z, float scale, float max){
        return (Noise.Generate(x*scale,y*scale,z*scale)+1f)*(max/2f);
    }

    // public static void SetVoxel(float x, float y, float z, Voxel voxel, Chunk chunk, bool replaceBlocks = false){
    //     x -=chunk.pos.x;
    //     y -=chunk.pos.y;
    //     z -=chunk.pos.z;
    //     if(Chunk.InRange(x,y,z)){
    //         if(replaceBlocks || chunk.GetVoxel(x,y,z) == null){
    //             chunk.SetVoxel(x,y,z,voxel);
    //         }
    //     }
    // }

    // public static void SetVoxel(int x, int y, int z, Voxel voxel, Chunk chunk, bool replaceBlocks = false){
    //     x -=chunk.pos.xi;
    //     y -=chunk.pos.yi;
    //     z -=chunk.pos.zi;
    //     if(Chunk.InRange(x,y,z)){
    //         if(replaceBlocks || chunk.GetVoxel(x,y,z) == null){
    //             chunk.SetVoxel(x,y,z,voxel);
    //         }
    //     }
    // }


    private static void SetVoxel(int x, int y, int z, Voxel voxel, Chunk chunk, float sDistF, bool replaceBlocks = false){
        x -=chunk.pos.xi;
        y -=chunk.pos.yi;
        z -=chunk.pos.zi;
        if(Chunk.InRange(x,y,z)){
            if(replaceBlocks || chunk.GetVoxel(x,y,z) == null){
                chunk.SetVoxel(x,y,z,voxel);
                // chunk.sDists[x,y,z] = sDistF;
            }
        }
    }



}
