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

    private List<RegionPos> savedRegionList = new List<RegionPos>();
    private List<RegionPos> regionList = new List<RegionPos>();
    private Dictionary<RegionPos, ChunkRegion2> regions = new Dictionary<RegionPos, ChunkRegion2>(World.regionPosEqualityComparer);

    public float [] minMax {get; private set;} = new float[2];
    private Dictionary<WorldPos, ColumnGen> gens = new Dictionary<WorldPos, ColumnGen>(World.worldPosEqC);

    public bool generated {get; private set;} = false;
    public bool destroying {get; private set;} = false;
    public bool destroyed {get; private set;} = false;
    public bool loaded {get; private set;} = false;


    public RegionCol(RegionPos pos){
        regionPos = pos.GetColumn();
        GenerateRegion();
    }

    public RegionCol(WorldPos pos){
        regionPos = pos.GetRegion().GetColumn();
        GenerateRegion();
    }

    private void GenerateRegion(){
        ThreadPool.QueueUserWorkItem(Generate, this);
        // Generate(this);
    }

    private void Generate(object state){
        WorldPos pos = regionPos.ToWorldPos();
        int x = pos.xi;
        int z = pos.zi;

        TerrainGen2 gen = World.GetWorld().gen;

        minMax[0] = 0;
        minMax[1] = 0;

        WorldPos temp = null;
        for(int i = x; i < (x+regionVoxels); i += Chunk2.chunkVoxels){
            for(int j = z; j < (z+regionVoxels); j += Chunk2.chunkVoxels){
                temp = new WorldPos(i,0,j);
                ColumnGen colGen = gen.GenerateColumnGen(temp);
                gens.Add(temp,colGen);
                MinMaxHeights(colGen);
            }
        }

        Debug.Assert(gens.Count == regionChunks* regionChunks, "RegionColumn "+ regionPos.ToColString()+ "has generated " + gens.Count + " ColumnGens instead of " + regionChunks*regionChunks);
        Debug.Assert(minMax[0] != 0 && minMax[1] != 0, "RegionColumn "+regionPos.ToColString()+ "has not generated minMax heights");

        CreateGenRegions();

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
        if(!regionPos.InColumn(pos) || !regionPos.InColumn(pos)){
            return null;
        }
        ChunkRegion2 region = null;
        regions.TryGetValue(pos,out region);
        return region;
    }

    public void AddRegion(ChunkRegion2 region){
        if(destroying || destroyed || !regionPos.InColumn(region.regionPos))
            return;
        regionList.Add(region.regionPos);
        regions.Add(region.regionPos,region);
    }

    private void CreateGenRegions(){
        int yMin = Mathf.FloorToInt(minMax[0]/regionSize);
        int yMax = Mathf.FloorToInt(minMax[1]/regionSize);

        RegionPos pos = null;
        for(int i = yMin; i <= yMax; i++){
            pos = new RegionPos(regionPos.x,i,regionPos.z);
            if(WasSavedRegion(pos)){
                throw new NotImplementedException("Loading of regions not implemented yet");
            }

            ChunkRegion2 region = new ChunkRegion2(pos, this);
            region.SetGenTerrain(true);
            AddRegion(region);
        }
    }

    private bool WasSavedRegion(RegionPos pos){
        if(savedRegionList.Count == 0){
            return false;
        }

        foreach(RegionPos sPos in savedRegionList){
            if(sPos.Equals(pos)){
                return true;
            }
        }
        return false;
    }

    public void CreateRegion(RegionPos pos){
        if(destroying || destroyed || !regionPos.InColumn(pos))
            return;
        ChunkRegion2 region = new ChunkRegion2(pos,this);
        AddRegion(region);
    }

    public ColumnGen GetColumnGen(WorldPos pos){
        ColumnGen gen = null;
        gens.TryGetValue(pos,out gen);
        return gen;
    }


}
