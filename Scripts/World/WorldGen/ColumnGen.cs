using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnGen
{
    
    private float[,] terrainHeight;
    private float[,] stoneHeight;
    public float [] minMax;
    public bool generated = false;
   
   public ColumnGen(){}

    public ColumnGen(float[] minMax, float[,] terrainHeight, float[,] stoneHeight){
        this.minMax = minMax;
        this.terrainHeight = terrainHeight;
        this.stoneHeight = stoneHeight;
        generated = true;
    }

    // Local code to set voxels
    private static void SetVoxel(int x, int y, int z, Voxel voxel, Chunk chunk, float sDistF, bool replaceBlocks = false){
        WorldPos chunkPos = chunk.GetPos();
        x -=chunkPos.xi;
        y -=chunkPos.yi;
        z -=chunkPos.zi;
        if(Chunk.InRange(x,y,z)){
            if(replaceBlocks || chunk.GetVoxel(x,y,z) == null){
                chunk.SetVoxel(x,y,z,voxel);
                // chunk.sDists[x,y,z] = sDistF;
            }
        }
    }

    // Local code to set voxels Chunk2 version
    private static void SetVoxel(int x, int y, int z, Voxel voxel, Chunk2 chunk, float sDistF, bool replaceBlocks = false){
        WorldPos chunkPos = chunk.chunkPos;
        x -=chunkPos.xi;
        y -=chunkPos.yi;
        z -=chunkPos.zi;
        if(Chunk2.InRange(x,y,z)){
            if(replaceBlocks || chunk.GetVoxel(x,y,z) == null){
                chunk.SetVoxel(x,y,z,voxel);
                // chunk.sDists[x,y,z] = sDistF;
            }
        }
    }


    //Start Chunk Gen here
    public Chunk ChunkGenC2(Chunk chunk){
        WorldPos chunkPos = chunk.GetPos();
        for (int x = chunkPos.xi ; x<chunkPos.xi+Chunk.chunkVoxels; x++){
            for (int z = chunkPos.zi ; z<chunkPos.zi+Chunk.chunkVoxels; z++){
                chunk = ChunkColumnGen(chunk,x,z);
            }
        }
        return chunk;
    }

    //Start Chunk Gen here Chunk2 version
    public Chunk2 ChunkGenC2(Chunk2 chunk){
        WorldPos chunkPos = chunk.chunkPos;
        for (int x = chunkPos.xi ; x<chunkPos.xi+Chunk.chunkVoxels+2; x++){
            for (int z = chunkPos.zi ; z<chunkPos.zi+Chunk.chunkVoxels+2; z++){
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

        WorldPos chunkPos = chunk.GetPos();

        for(int yi = chunkPos.yi+Chunk.chunkVoxels; yi>=chunkPos.yi; yi--){
            float sDistF = sDistFGen(chunk,xi,yi,zi);

            if (sDistF >0){
                SetVoxel(xi,yi,zi,new VoxelAir(sDistF),chunk,sDistF);
            }
            else{
                if(chunkPos.y >= 8){
                     SetVoxel(xi,yi,zi,new VoxelGrass(sDistF),chunk,sDistF);
                }
                SetVoxel(xi,yi,zi,new Voxel(sDistF),chunk,sDistF);
            }
        }
        return chunk;

    }

    //Int version of Chunk Column gen Chunk2 version
    private Chunk2 ChunkColumnGen(Chunk2 chunk, int xi, int zi){

        // float stoneheight = stoneHeight[xi,zi];
        // // float MountainsBiome = GetNoise(x,0,z,MountainsBiomeFrequency,MountainsBiomeSize);
        // float dirtHeight = terrainHeight[xi,zi];

        WorldPos chunkPos = chunk.chunkPos;

        for(int yi = chunkPos.yi+Chunk.chunkVoxels+1; yi>=chunkPos.yi; yi--){
            float sDistF = sDistFGen(chunk,xi,yi,zi);

            if (sDistF >0){
                SetVoxel(xi,yi,zi,new VoxelAir(sDistF),chunk,sDistF);
            }
            else{
                if(chunkPos.y >= 8){
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
        int xi = x-(chunk.GetPos().xi-1);
        int zi = z-(chunk.GetPos().zi-1);

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

    //Main function to generate a Signed distance field at location Chunk2 version
    private float sDistFGen(Chunk2 chunk, int x, int yi, int z){
        float sDistF = 0;
        int xi = x-(chunk.chunkPos.xi-1);
        int zi = z-(chunk.chunkPos.zi-1);

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

}
