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

    

    




}
