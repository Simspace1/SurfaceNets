using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkRegions
{
    public const int regionSize = 4* Chunk.chunkVoxels;

    [SerializeField]
    private WorldPos regionPos;

    [SerializeField]
    private List<WorldPos> columnsPos;

    [SerializeField]
    private List<string> chunkColumnsSave;

    private List<ChunkColumn> chunkColumns;

    [System.NonSerialized]
    public bool modified = false;

    public ChunkRegions(WorldPos pos){
        regionPos = pos;
        columnsPos = new List<WorldPos>();
        chunkColumnsSave = new List<string>();
        chunkColumns = new List<ChunkColumn>();
    }

    public static ChunkRegions JsonToRegion(string save){
        ChunkRegions region = JsonUtility.FromJson<ChunkRegions>(save);
        region.RevertColumns();

        return region;
    }

    public string RegionToJson(){
        return JsonUtility.ToJson(this);
    }

    public void AddChunkColumn(ChunkColumn col){
        columnsPos.Add(col.columnPos);
        chunkColumnsSave.Add(col.ChunkColumnToJSON());
        modified = true;
    }

    private void RevertColumns(){
        chunkColumns = new List<ChunkColumn>();

        foreach(string save in chunkColumnsSave){
            chunkColumns.Add(ChunkColumn.JsonToChunkColumn(save));
        }
    }

    public WorldPos GetPos(){
        return regionPos;
    }

    public static WorldPos GetRegionPos(WorldPos chunkPos){
        

        int x = chunkPos.xi;
        int z = chunkPos.zi;

        int xi = x / regionSize;
        int zi = z / regionSize;

        int Xi = xi * regionSize;
        int Zi = zi * regionSize;

        return new WorldPos(Xi, 0, Zi);
    }

    public ChunkColumn GetChunkColumn(WorldPos pos){
        foreach(ChunkColumn col in chunkColumns){
            if(col.columnPos.Equals(pos)){
                col.loaded = true;
                return col;
            }
        }
        return null;
    }

    

    




}
