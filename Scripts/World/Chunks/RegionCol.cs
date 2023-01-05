using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class RegionCol
{
    public const int regionChunks = 4;
    public const int regionSize = regionChunks*Chunk.chunkSize;
    public const int regionVoxels = regionChunks*Chunk.chunkVoxels;

    public RegionPos regionPos {get; private set;}

    private List<RegionPos> regionList = new List<RegionPos>();
    private Dictionary<RegionPos, ChunkRegion2> regions = new Dictionary<RegionPos, ChunkRegion2>(World.regionPosEqualityComparer);

    public float [] minMax {get; private set;} = new float[2];
    private Dictionary<WorldPos, ColumnGen> gens = new Dictionary<WorldPos, ColumnGen>(World.worldPosEqC);

    public bool generated {get; private set;} = false;
    public bool destroying {get; private set;} = false;
    public bool destroyed {get; private set;} = false;

    public RegionCol(RegionPos pos){
        regionPos = pos.GetColumn();
        GenerateHeights();
    }

    public RegionCol(WorldPos pos){
        regionPos = pos.GetRegion().GetColumn();
        GenerateHeights();
    }

    private void GenerateHeights(){
        ThreadPool.QueueUserWorkItem(Generate, this);
    }

    private void Generate(object state){
        WorldPos pos = regionPos.ToWorldPos();
        int x = pos.xi;
        int z = pos.zi;

        TerrainGen2 gen = World.GetWorld().gen;

        minMax[0] = 0;
        minMax[1] = 0;

        WorldPos temp = null;
        for(int i = x; i < (x+regionVoxels); i += Chunk.chunkVoxels){
            for(int j = z; j < (z+regionVoxels); j += Chunk.chunkVoxels){
                temp = new WorldPos(i,0,j);
                ColumnGen colGen = gen.GenerateColumnGen(temp);
                gens.Add(temp,colGen);
                MinMaxHeights(colGen);
            }
        }

        generated = true;
    }

    private void MinMaxHeights(ColumnGen gen){
        if(minMax[0] == 0 && minMax[1] == 0){
            minMax[0] = gen.minMax[0];
            minMax[1] = gen.minMax[1];
        }
        else if(minMax[0] > gen.minMax[0]){
            minMax[0] = gen.minMax[0];
        }
        else if(minMax[1] < gen.minMax[1]){
            minMax[1] = gen.minMax[1];
        }
    }

    public ChunkRegion2 GetRegion(WorldPos pos){
        return GetRegion(pos.GetRegion());
    }

    public ChunkRegion2 GetRegion(RegionPos pos){
        ChunkRegion2 region = null;
        regions.TryGetValue(pos,out region);
        return region;
    }

    public void AddRegion(ChunkRegion2 region){
        if(destroying || destroyed)
            return;
        regionList.Add(region.regionPos);
        regions.Add(region.regionPos,region);
    }


}