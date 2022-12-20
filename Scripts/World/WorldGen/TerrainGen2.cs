using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoiseTest;
using System.IO;

public class TerrainGen2
{
    private long seed;

    // private float scale = 1;
    private float persistance = 0.5f;
    // private int octaves = 5;
    private float lacunarity = 2f;
    // private float exponentiation = 3;
    private float height = 1;
    private OpenSimplexNoise Noise;


    private float stoneBaseHeight = 0;
    private float stoneBaseNoiseHeight = 50;

    // private float MountainsBiomeFrequency = 0.000000000000000000000000000000000000000000000000000000005f;
    // private float MountainsBiomeSize = 5;
    // private float MountainsBiomeAmplitureMultiplier = 1;

    // private float stoneMountainFrequency = 0.008f;

    private float dirtBaseHeight = 3;
    private float dirtNoiseHeight = 2;



    public TerrainGen2(long seed){
        this.seed = seed;
        Noise = new OpenSimplexNoise(seed);
    }

    
    private float ComputeFBM(WorldPos pos, double scale, int octaves = 1, double frequency = 1, float exponentiation = 3){
        return ComputeFBM(pos.x,pos.z,scale, octaves, frequency, exponentiation);
    }

    private float ComputeFBM(float x, float z , double scale, int octaves = 1, double frequency = 1, float exponentiation = 3){
        double xs = (x) / scale;
        double zs = (z) / scale;
        double G = 2.0 * (persistance);
        double amplitude = 1;
        double norm = 0;
        double total = 0;

        for (int i = 0; i <octaves ; i++){
            double noise;
            if(i % 2 == 1){
                noise = Get2DNoise(zs*frequency+20,xs*frequency+20)*0.5f+0.5f;
            }
            else{
                noise = Get2DNoise(xs*frequency,zs*frequency)*0.5f+0.5f;
            }
            total += noise * amplitude;
            norm += amplitude;
            amplitude *= G;
            frequency *= lacunarity;
        }

        total /= norm;
        return (float) System.Math.Pow(total, exponentiation)*height;
    }

    private double Get2DNoise(double x, double y){
        return Noise.Evaluate(x,y);
    }

    private double Get3DNoise(double x, double y, double z){
        return Noise.Evaluate(x,y,z);
    }

    public ColumnGen GenerateColumnGen(WorldPos pos){
        float[,] terrainHeight = new float[Chunk.chunkVoxels+3,Chunk.chunkVoxels+3];
        float[,] stoneHeight = new float[Chunk.chunkVoxels+3,Chunk.chunkVoxels+3];
        float[] minMax = new float[2];

        // Old Code from TerrainGen
        // float stoneheight, MountainsBiome,dirtHeight;
        float min = 0;
        float max = 0;
        for (float x = pos.x-Chunk.voxelSize ; x<pos.x+Chunk.chunkSize+Chunk.voxelSize*2; x += Chunk.voxelSize){
            for (float z = pos.z-Chunk.voxelSize ; z<pos.z+Chunk.chunkSize+Chunk.voxelSize*2; z += Chunk.voxelSize){
                int xi = Mathf.FloorToInt((x-(pos.x-Chunk.voxelSize))/Chunk.voxelSize);
                int zi = Mathf.FloorToInt((z-(pos.z-Chunk.voxelSize))/Chunk.voxelSize);

                var val = GenerateHeight(x,z);

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
            }
        }
        minMax = new float[2];
        minMax[0] = min;
        minMax[1] = max;

        return new ColumnGen(minMax, terrainHeight,stoneHeight);
    }

    private float[] GenerateHeight(float x, float z){
        float stoneheight, dirtheight, MountainsBiome;
        stoneheight = 0;
        dirtheight = 0;

        stoneheight = stoneBaseHeight;
        stoneheight += ComputeFBM(x,z,100,5,0.1)*stoneBaseNoiseHeight;


        // MountainsBiome = ComputeFBM(x,z,100,3,0.05);
        MountainsBiome = ComputeFBM(x,z,200,2,0.05);
        // stoneheight += MountainsBiome*100;

        //MountainHeight
        // stoneheight += ComputeFBM(x,z,200,6,0.5,3)*MountainsBiome*400;
        stoneheight += ComputeFBM(x,z,1000,6,0.5,3)*MountainsBiome*400;
        


        dirtheight = stoneheight + dirtBaseHeight;
        dirtheight += ComputeFBM(x,z,100,5,0.5)*dirtNoiseHeight*2;


        float[] val = new float[2];
        val[0] = stoneheight;
        val[1] = dirtheight;
        return val;
    }




    public float[,] GenerateWorldHeight(){
        float[,] heights = new float[2001,2001];
        float[] val;

        for(int i = 0; i < 2001; i++){
            for(int j = 0; j < 2001; j++){
                val = GenerateHeight((i-1000)*Chunk.chunkSize, (j-1000)*Chunk.chunkSize);
                heights[i,j] = val[1];
            }
        }

        return heights;
    }

    public Texture2D GenTexture2D(float[,] data){
        int width = data.GetLength(0);
        int height = data.GetLength(1);
        Texture2D texture = new Texture2D(width,height);
        
        float max = 0;
        float min = 0;
        foreach(float x in data){
            if(x > max){
                max = x;
            }
            else if(x < min){
                min = x;
            }
        }

        float val;
        for(int i = 0; i < width; i++){
            for(int j = 0; j < height; j++){
                val = data[i,j];
                val = val - min;
                val /= (max-min);
                texture.SetPixel(i,j,new Color(val,val,val,1));
            }
        }
        texture.Apply();

        return texture;

        // SaveTexture(texture, Application.persistentDataPath + "/saves/worldMap.png");

        
    }
    
}
