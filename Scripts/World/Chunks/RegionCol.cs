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

    //Saved vars
    public RegionPos regionPos {get; private set;}
    private List<RegionSurfacePos> savedRegionList = new List<RegionSurfacePos>();

    private List<RegionSurfacePos> loadedRegionList = new List<RegionSurfacePos>();
    private List<RegionSurfacePos> nonLoadedRegionsList = new List<RegionSurfacePos>();
    private List<RegionSurfacePos> regionList = new List<RegionSurfacePos>();
    private Dictionary<RegionPos, Region> regions = new Dictionary<RegionPos, Region>(World.regionPosEqualityComparer);

    private Queue<RegionSurfacePos> loadingRegions = new Queue<RegionSurfacePos>();

    public float [] minMax {get; private set;} = new float[2];
    private Dictionary<WorldPos, ColumnGen> gens = new Dictionary<WorldPos, ColumnGen>(World.worldPosEqC);

    public bool generated {get; private set;} = false;
    public bool destroying {get; private set;} = false;
    public bool destroyed {get; private set;} = false;
    public bool loaded {get; private set;} = false;
    public bool complete {get; private set;} = false;
    public bool modified {get; private set;} = false;
    public bool generating {get; private set;} = false;
    public bool fullRes {get; private set;} = false;


    public RegionCol(RegionPos pos, bool generate, bool fullRes = false){
        regionPos = pos.GetColumn();
        if(generate){
            GenerateRegion();
        }
    }

    public RegionCol(WorldPos pos){
        regionPos = pos.GetRegion().GetColumn();
        GenerateRegion();
    }

    private void GenerateRegion(){
        generating = true;
        ThreadPool.QueueUserWorkItem(Generate, this);
        // Generate(this);
    }

    private void loadRegions(object state){
        foreach(RegionSurfacePos surfacePos in loadingRegions){
            throw new NotImplementedException("Loading of regions not implemented yet");
        }
        generating = false;
    }

    public void Generate(){
        if(generated){
            return;
        }

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
        generating = false;
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
        generating = false;
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

    public Region GetRegion(WorldPos pos){
        return GetRegion(pos.GetRegion());
    }

    public Region GetRegion(RegionPos pos){
        if(!regionPos.InColumn(pos)){
            return null;
        }
        Region region = null;
        regions.TryGetValue(pos,out region);
        return region;
    }

    public void AddRegion(Region region, bool genTerrain = false){
        if(destroying || destroyed || !regionPos.InColumn(region.regionPos))
            return;
        regionList.Add(new RegionSurfacePos(region.regionPos,genTerrain));
        region.SetSurface(genTerrain);
        regions.Add(region.regionPos,region);
    }

    private void CreateGenRegions(){
        int yMin = Mathf.FloorToInt((minMax[0]+regionSize/2)/regionSize);
        int yMax = Mathf.FloorToInt((minMax[1]+regionSize/2)/regionSize);

        RegionPos pos = null;
        for(int i = yMin; i <= yMax; i++){
            pos = new RegionPos(regionPos.x,i,regionPos.z);
            if(WasSavedRegion(pos)){
                throw new NotImplementedException("Loading of regions not implemented yet");
                //Add region to loadedRegion list
            }
            else{
                CreateRegion(pos,true);
            }
        }
        if(!CheckComplete()){
            CreateSurfaceRegions();
        }
    }

    private void CreateSurfaceRegions(){
        foreach(RegionSurfacePos surfacePos in savedRegionList){
            if(surfacePos.surface && !AlreadyCreated(surfacePos)){
                throw new NotImplementedException("Loading of regions not implemented yet");
                //Add region to loadedRegion list
            }
            else{
                nonLoadedRegionsList.Add(surfacePos);
            }
        }
    }

    private bool AlreadyCreated(RegionSurfacePos surfacePos){
        if(regionList.Count == 0){
            return false;
        }
        
        foreach(RegionSurfacePos surfacePos1 in regionList){
            if(surfacePos.Equals(surfacePos1)){
                return true;
            }
        }
        return false;
    }

    private bool WasSavedRegion(RegionPos pos){
        if(savedRegionList.Count == 0){
            return false;
        }

        foreach(RegionSurfacePos sPos in savedRegionList){
            if(sPos.regionPos.Equals(pos)){
                return true;
            }
        }
        return false;
    }

    public void CreateRegion(RegionPos pos, bool genTerrain = false){
        if(destroying || destroyed || !regionPos.InColumn(pos))
            return;
        Region region = new Region(pos,this);
        AddRegion(region , genTerrain);
    }

    public ColumnGen GetColumnGen(WorldPos pos){
        ColumnGen gen = null;
        gens.TryGetValue(pos.ToColumn(),out gen);
        return gen;
    }

    public List<RegionSurfacePos> GetRegionList(){
        return regionList;
    }

    //checks before nonLoadedRegionsList were added
    private bool CheckComplete(){
        if(savedRegionList.Count == 0 || loadedRegionList.Count == savedRegionList.Count){
            complete = true;
            return complete;
        }
        else{
            complete = false;
            return complete;
        }
    }

    public bool IsComplete(){
        if(nonLoadedRegionsList.Count > 0){
            complete = false;
            return complete;
        }
        else{
            complete = true;
            return complete;
        }
    }

    public List<RegionSurfacePos> GetNonLoadedRegionsList(){
        return nonLoadedRegionsList;
    }


    //loads unloaded regions that are in range of player
    // public bool LoadRegions(RegionPos playerpos){
    //     foreach (RegionSurfacePos surfacePos in nonLoadedRegionsList){
    //         if(surfacePos.regionPos.y <= playerpos.y + 10 && surfacePos.regionPos.y >= playerpos.y -10){
    //             loadingRegions.Add(surfacePos);
    //         }
    //     }

    //     if(loadingRegions.Count > 0){
    //         generating = true;
    //         ThreadPool.QueueUserWorkItem(loadRegions,this);
    //         return true;
    //     }
    //     else{
    //         return false;
    //     }
    // }

    public bool LoadRegions(RegionPos playerPos){
        if(loadingRegions.Count > 0){
            return false;
        }

        foreach (RegionSurfacePos surfacePos in nonLoadedRegionsList){
            if(surfacePos.regionPos.y <= playerPos.y + 10 && surfacePos.regionPos.y >= playerPos.y -10){
                loadingRegions.Enqueue(surfacePos);
            }
        }

        foreach(RegionSurfacePos surfacePos1 in loadingRegions){
            nonLoadedRegionsList.Remove(surfacePos1);
            loadedRegionList.Add(surfacePos1);
        }

        if(loadingRegions.Count > 0){
            generating = true;
            MyThreadPool.QueueJob(new ThreadJobRegionColVert(this));
            return true;
        }
        else{
            return false;
        }
    }

    public void GenerateVertical(){
        foreach(RegionSurfacePos surfacePos in loadingRegions){
            throw new NotImplementedException("Loading of regions not implemented yet");
        }
    }

    public void CreateVertical(){
        Region region = null;
        foreach(RegionSurfacePos surfacePos in loadingRegions){
            regions.TryGetValue(surfacePos.regionPos, out region);
            if(region != null){
                region.CreateAllChunks();
            }
        }
    }

    public void QueueVerticalUpdates(){
        Region region = null;
        while(loadingRegions.Count > 0){
            RegionSurfacePos surfacePos = loadingRegions.Dequeue();
            regions.TryGetValue(surfacePos.regionPos, out region);
            if(region != null){
                region.QueueAllChunkUpdates();
            }
        }
    }

    // public List<RegionSurfacePos> GetLoadingRegions(){
    //     return loadingRegions;
    // }

    public void ClearLoadingList(){
        loadingRegions.Clear();
    }

    public void CreateAllChunks(){
        if(destroyed){
            return;
        }
        foreach(var regionEntry in regions){
            regionEntry.Value.CreateAllChunks();
        }
    }

    public void QueueAllChunkUpdates(){
        foreach(var regionEntry in regions){
            regionEntry.Value.QueueAllChunkUpdates();
            if(regionEntry.Value.fullRes){
                regionEntry.Value.SetChunkUpdatedFull(true);
            }
            else{
                regionEntry.Value.SetChunkUpdatedhalf(true);
            }
        }
    }

    public void Save(){
        if(modified){
            throw new NotImplementedException("Saving of RegionCol not implemented yet");
        }
    }

    public void Destroy(){
        destroyed = true;
        
        foreach(var regionEntry in regions){
            regionEntry.Value.Destroy();
        }
    }


    public List<Region>[] CheckChangeRegionResolution(RegionPos playerPos, int fullResRegionRadius){
        List<Region> regionsUpRes = new List<Region>();
        List<Region> regionsDownRes = new List<Region>();
        foreach(var regionEntry in regions){
            if(regionEntry.Value.chunksUpdatedHalf && Mathf.Abs(regionEntry.Key.x - playerPos.x) <= fullResRegionRadius && Mathf.Abs(regionEntry.Key.z - playerPos.z) <= fullResRegionRadius && Mathf.Abs(regionEntry.Key.y - playerPos.y) <= fullResRegionRadius && !regionEntry.Value.fullRes){
                regionsUpRes.Add(regionEntry.Value);
            }
            else if(regionEntry.Value.fullRes && regionEntry.Value.chunksUpdatedFull && (Mathf.Abs(regionEntry.Key.x - playerPos.x) > fullResRegionRadius || Mathf.Abs(regionEntry.Key.z - playerPos.z) > fullResRegionRadius || Mathf.Abs(regionEntry.Key.y - playerPos.y) > fullResRegionRadius)){
                regionsDownRes.Add(regionEntry.Value);
            }
        }
        return new List<Region>[]{regionsUpRes, regionsDownRes};
    }


}
