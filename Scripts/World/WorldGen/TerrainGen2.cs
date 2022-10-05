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

    
    private double ComputeFBM(WorldPos pos){
        double xs = pos.x / scale;
        double zs = pos.z /scale;
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
        return System.Math.Pow(total, exponentiation * height);
    }

    private double ComputeFBM(float x, float z){
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
        return System.Math.Pow(total, exponentiation * height);
    }

    private double Get2DNoise(double x, double y){
        return Noise.Evaluate(x,y);
    }

    public ColumnGen GenColumn(WorldPos pos){
        return new ColumnGen();
    }

    
}
