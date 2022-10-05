using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnGen
{
    
    private float[,] terrainHeight = new float[Chunk.chunkVoxels+3,Chunk.chunkVoxels+3];
    private float[,] stoneHeight = new float[Chunk.chunkVoxels+3,Chunk.chunkVoxels+3];
    public float [] minMax;
    public bool generated = false;

    //Vars for Actual Generation of heights
    // private float stoneBaseHeight = 0;
    // private float stoneBaseNoise = 0.005f;
    // private float stoneBaseNoiseHeight = 4;

    // private float MountainsBiomeFrequency = 0.000000000000000000000000000000000000000000000000000000005f;
    // private float MountainsBiomeSize = 5;
    // private float MountainsBiomeAmplitureMultiplier = 1;

    // private float stoneMountainFrequency = 0.008f;

    // private float dirtBaseHeight = 1;
    // private float dirtNoise = 0.04f;
    // private float dirtNoiseHeight = 3;




    // Local code to set voxels
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


    //Start Chunk Gen here
    public Chunk ChunkGenC2(Chunk chunk){
        for (int x = chunk.pos.xi ; x<chunk.pos.xi+Chunk.chunkVoxels; x++){
            for (int z = chunk.pos.zi ; z<chunk.pos.zi+Chunk.chunkVoxels; z++){
                chunk = ChunkColumnGen(chunk,x,z);
            }
        }
        return chunk;
    }

    //Int version of Chunk Column gen
    private Chunk ChunkColumnGen(Chunk chunk, int xi, int zi){

        // float stoneheight = stoneHeight[xi,zi];
        // // float MountainsBiome = GetNoise(x,0,z,MountainsBiomeFrequency,MountainsBiomeSize);
        // float dirtHeight = terrainHeight[xi,zi];

        for(int yi = chunk.pos.yi+Chunk.chunkVoxels; yi>=chunk.pos.yi; yi--){
            float sDistF = sDistFGen(chunk,xi,yi,zi);

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

    //Main function to generate a Signed distance field at location
    private float sDistFGen(Chunk chunk, int x, int yi, int z){
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

    //Function used for SDistGen
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

    public float[,] GetTerrainHeight(){
        return terrainHeight;
    }


//Commented For now might go in TerrainGen2 instead

    // public float[] MmTerrainHeight(WorldPos pos){
    //     // float stoneheight, MountainsBiome,dirtHeight;
    //     float min = 0;
    //     float max = 0;
    //     for (float x = pos.x-Chunk.voxelSize ; x<pos.x+Chunk.chunkSize+Chunk.voxelSize*2; x += Chunk.voxelSize){
    //         for (float z = pos.z-Chunk.voxelSize ; z<pos.z+Chunk.chunkSize+Chunk.voxelSize*2; z += Chunk.voxelSize){
    //             int xi = Mathf.FloorToInt((x-(pos.x-Chunk.voxelSize))/Chunk.voxelSize);
    //             int zi = Mathf.FloorToInt((z-(pos.z-Chunk.voxelSize))/Chunk.voxelSize);

    //             // stoneheight = stoneBaseHeight;
    //             // stoneheight += GetNoise(x,0,z,stoneBaseNoise,stoneBaseNoiseHeight);

    //             // MountainsBiome = GetNoise(x,0,z,MountainsBiomeFrequency,MountainsBiomeSize)*MountainsBiomeAmplitureMultiplier;
    //             // stoneheight += GetNoise(x,0,z,stoneMountainFrequency,MountainsBiome);

    //             // dirtHeight = stoneheight + dirtBaseHeight;
    //             // dirtHeight += GetNoise(x,100,z,dirtNoise,dirtNoiseHeight);

    //             var val = GenerateHeights(x,z);

    //             stoneHeight[xi,zi] = val[0];

    //             terrainHeight[xi,zi] = val[1];
    //             if (min == 0 && max == 0 && val[1] != 0){
    //                 min = val[1];
    //                 max = val[1];
    //             }
    //             else if(val[1] < min){
    //                 min = val[1];
    //             }
    //             else if(val[1] > max){
    //                 max = val[1];
    //             }

    //             // terrainHeight[xi,zi] = dirtHeight;
    //             // if (min == 0 && max == 0 && dirtHeight != 0){
    //             //     min = dirtHeight;
    //             //     max = dirtHeight;
    //             // }
    //             // else if(dirtHeight < min){
    //             //     min = dirtHeight;
    //             // }
    //             // else if(dirtHeight > max){
    //             //     max = dirtHeight;
    //             // }
    //         }
    //     }
    //     minMax = new float[2];
    //     minMax[0] = min;
    //     minMax[1] = max;
    //     generated = true;
    //     return minMax;
    // }

    // public float[] GenerateHeights(float x, float z){
    //     float stoneheight, dirtheight, MountainsBiome;
    //     stoneheight = 0;
    //     dirtheight = 0;

    //     stoneheight = stoneBaseHeight;
    //     stoneheight += GetNoise(x,0,z,stoneBaseNoise,stoneBaseNoiseHeight);


    //     MountainsBiome = GetNoise(x,0,z,MountainsBiomeFrequency,MountainsBiomeSize);
    //     MountainsBiome = GetNoise(x,0,z,0.05f,100);
    //     stoneheight += GetNoise(x,0,z,stoneMountainFrequency,MountainsBiome);


    //     dirtheight = stoneheight + dirtBaseHeight;
    //     dirtheight += GetNoise(x,100,z,dirtNoise,dirtNoiseHeight);


    //     float[] val = new float[2];
    //     val[0] = stoneheight;
    //     val[1] = dirtheight;
    //     return val;
    // }

}
