using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoiseTest;

public class TerrainGen2
{
    private long seed;

    private float scale = 1;
    private float persistance = 1;
    private int octaves = 1;
    private float lacunarity = 1;
    private float exponentiation = 1;
    private float height = 1;
    private OpenSimplexNoise Noise;

    public TerrainGen2(long seed){
        this.seed = seed;
        Noise = new OpenSimplexNoise(seed);
    }

    
    // private float ComputeFBM(WorldPos pos){
    //     float xs = pos.x / scale;
    //     float zs = pos.z /scale;
    //     float G = 2.0f * (-persistance);
    //     float amplitude = 1;
    //     float frequency = 1;
    //     float norm = 0;
    //     float total = 0;

    //     for (int i = 0; i <octaves ; i++){
    //         float noise = GetNoise(xs*frequency,zs*frequency)*0.5f+0.5f;
    //         total += noise * amplitude;
    //         norm += amplitude;
    //         amplitude *= G;
    //         frequency *= lacunarity;
    //     }

    //     total /= norm;
    //     return Mathf.Pow(total, exponentiation * height);
    // }

    // private float ComputeFBM(float x, float z){
    //     float xs = x / scale;
    //     float zs = z /scale;
    //     float G = 2.0f * (-persistance);
    //     float amplitude = 1;
    //     float frequency = 1;
    //     float norm = 0;
    //     float total = 0;

    //     for (int i = 0; i <octaves ; i++){
    //         float noise = GetNoise(xs*frequency,zs*frequency)*0.5f+0.5f;
    //         total += noise * amplitude;
    //         norm += amplitude;
    //         amplitude *= G;
    //         frequency *= lacunarity;
    //     }

    //     total /= norm;
    //     return Mathf.Pow(total, exponentiation * height);
    // }

    
}
