using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoiseTest;

public class TerrainGen2
{
    private long seed;

    private float scale = 1;
    private float persistance = 1;
    private int octaves = 5;
    private float lacunarity = 1;
    private float exponentiation = 1;
    private float height = 1;
    private OpenSimplexNoise Noise;


    private float stoneBaseHeight = 0;
    private float stoneBaseNoiseHeight = 4;

    private float MountainsBiomeFrequency = 0.000000000000000000000000000000000000000000000000000000005f;
    private float MountainsBiomeSize = 5;
    private float MountainsBiomeAmplitureMultiplier = 1;

    private float stoneMountainFrequency = 0.008f;

    private float dirtBaseHeight = 1;
    private float dirtNoiseHeight = 3;



    public TerrainGen2(long seed){
        this.seed = seed;
        Noise = new OpenSimplexNoise(seed);
    }

    
    private float ComputeFBM(WorldPos pos, int octaves = 1){
        return ComputeFBM(pos.x,pos.z, octaves);
    }

    private float ComputeFBM(float x, float z , int octaves = 1){
        double xs = x / scale;
        double zs = z /scale;
        double G = 2.0f * (-persistance);
        double amplitude = 1;
        double frequency = 1;
        double norm = 0;
        double total = 0;

        for (int i = 0; i <octaves ; i++){
            double noise = Get2DNoise(xs*frequency,zs*frequency)*0.5f+0.5f;
            total += noise * amplitude;
            norm += amplitude;
            amplitude *= G;
            frequency *= lacunarity;
        }

        total /= norm;
        return (float) System.Math.Pow(total, exponentiation * height);
    }

    private double Get2DNoise(double x, double y){
        return Noise.Evaluate(x,y);
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
        stoneheight += ComputeFBM(x,z,2)*stoneBaseNoiseHeight;


        // MountainsBiome = GetNoise(x,0,z,MountainsBiomeFrequency,MountainsBiomeSize);
        // MountainsBiome = GetNoise(x,0,z,0.05f,100);
        // stoneheight += GetNoise(x,0,z,stoneMountainFrequency,MountainsBiome);


        dirtheight = stoneheight + dirtBaseHeight;
        dirtheight += ComputeFBM(x,z,1)*dirtNoiseHeight;


        float[] val = new float[2];
        val[0] = stoneheight;
        val[1] = dirtheight;
        return val;
    }

    
}
