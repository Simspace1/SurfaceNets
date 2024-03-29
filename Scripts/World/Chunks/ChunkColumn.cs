using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ChunkColumn
{   
    [System.NonSerialized]
    public World world;
    
    public WorldPos columnPos;
    
    public List<WorldPos> chunkColumn = new List<WorldPos>();

    [System.NonSerialized]
    public List<Chunk> chunks;

    [SerializeField]
    private List<string> saveChunks;

    

    [System.NonSerialized]
    public bool rendered = false;
    [System.NonSerialized]
    public bool created = false;
    [System.NonSerialized]
    public bool creating = false;
    [System.NonSerialized]
    public bool rendering = false;
    [System.NonSerialized]
    public bool destroying = false;
    [System.NonSerialized]
    public bool creating2 = false;
    [System.NonSerialized]
    public bool loaded = false;
    [System.NonSerialized]
    public bool modified = false;
    [System.NonSerialized]
    private bool createTh1 = false;
    [System.NonSerialized]
    private bool createTh2 = false;
    private ColumnData data;
    [System.NonSerialized]
    public string path = Application.persistentDataPath;
    [System.NonSerialized]
    public Columns col;


    private Thread CreateThread;
    private Thread CreateThread2;

    // public ChunkColumn(World world, WorldPos pos){
    //     this.world = world;
    //     this.pos = pos;
    //     CreateThread = new Thread(new ThreadStart(() => Create()));
    //     CreateThread2 = new Thread(new ThreadStart(() => Create2()));
        
    // }

    public ChunkColumn(Columns column){
        this.world = column.world;
        this.columnPos = column.pos;
        this.col = column;
        // CreateThread = new Thread(new ThreadStart(() => Create()));
        // CreateThread2 = new Thread(new ThreadStart(() => Create2()));
    }


    public void CreateStart(){
        creating = true;
        createTh1 = true;
        // CreateThread = new Thread(new ThreadStart(() => Create()));
        // CreateThread.Start();
        ThreadPool.QueueUserWorkItem(Create);
    }

    public void CreateStart2(){
        // if(loaded){
        //     data.ReCreateChunks(this);

        // }
        // else{
        chunks = world.CreateChunkColumn(this);
        
        // if(loaded){
        //     JSONToChunk();
        // }
        // }
        // CreateThread2 = new Thread(new ThreadStart(() => Create2()));
        creating2 = true;
        createTh2 = true;
        // CreateThread2.Start();
        ThreadPool.QueueUserWorkItem(Create2);
    }

    // public void RenderStart(){
    //     rendering = true;
    //     RenderThread = new Thread(new ThreadStart(() => Render()));
    //     RenderThread.Start();
    // }

    // public void DestroyStart(){
    //     destroying = true;
    //     DestroyThread = new Thread(new ThreadStart(() => Destroy()));
    //     DestroyThread.Start();
    // }

    
    public bool CreateCheck1(){
        // return CreateThread.IsAlive;
        return createTh1;
    }

    public bool CreateCheck2(){
        // return CreateThread2.IsAlive;
        return createTh2;
    }

    // public bool RenderCheck(){
    //     return RenderThread.IsAlive;
    // }

    // public bool DestroyCheck(){
    //     return DestroyThread.IsAlive;
    // }

    public void CreateEnd(){
        creating = false;
        creating2 = false;
        created = true;
        // CreateThread = null;
        // CreateThread2 = null;
    }


    private void Create(object state){
        
        // if(world.GetWorldData().Contains(columnPos)){
        //     loaded = true;
        //     if(col.gen == null){
        //         col.SetGen(world.gen.GenerateColumnGen(columnPos));
        //     }
        //     data = SaveManager.LoadChunkColumn(this);
        //     data.Revert1(this);
        // }
        
        if(loaded){
            if(col.gen == null){
                col.SetGen(world.gen.GenerateColumnGen(columnPos));
            }
        }
        else{
            if(col.gen == null){
                col.SetGen(world.gen.GenerateColumnGen(columnPos));
            }
            float [] minMax = col.gen.minMax;

            float min = Mathf.FloorToInt((minMax[0]-Chunk.chunkSize)/Chunk.chunkSize)*Chunk.chunkSize;
            float max = Mathf.CeilToInt((minMax[1]+Chunk.chunkSize)/Chunk.chunkSize)*Chunk.chunkSize;

            WorldPos chunkPos;
            for(float y = min; y <= max; y +=Chunk.chunkSize){
                chunkPos = new WorldPos(columnPos.x,y,columnPos.z);
                chunkColumn.Add(chunkPos);
            }
        }
        createTh1 = false;
    }

    private void Create2(object state){
        if(loaded){
            // data.Revert2(this);
            JSONToChunk();
        }
        else{
            foreach(Chunk chunk in chunks){
                col.gen.ChunkGenC2(chunk);
            }
        }
        createTh2 = false;
    }


    public void Render(){
        // if(loaded){
        //     foreach(Chunk chunk in chunks){
        //         if(chunk.meshData !=null){
        //             chunk.RenderMesh(chunk.meshData);
        //         }
        //         else{
        //             world.AddChunkUpdate(chunk);
        //             chunk.update = true;
        //         }
        //     }
        //     rendered = true;
        //     loaded = false;
        // }        
        // else{
            foreach(Chunk chunk in chunks){
                world.AddChunkUpdate(chunk);
                chunk.update = true;
            }
            rendered = true;
        // }
    }

    public bool CheckRendered(){
        if(!rendered){
            return false;
        }
        foreach(Chunk chunk in chunks){
            if(!chunk.rendered){
                return false;
            }
        }
        return true;
    }

    public bool CheckModified(){
        if(!created){
            return false;
        }
        foreach(Chunk chunk in chunks){
            if(chunk.modified){
                modified = true;
                return true;
            }
        }
        return false;
    }

    public Chunk GetChunk(WorldPos pos){
        foreach(Chunk chunk in chunks){
            if(chunk.GetPos().Equals(pos)){
                return chunk;
            }
        }
        return null;
    }

    public bool UpdatingQueued(){
        foreach(Chunk chunk in chunks){
            if(chunk.update){
                return true;
            }
        }
        return false;
    }

    public bool ChunkUpdating(){
        foreach(Chunk chunk in chunks){
            if(chunk.updating){
                return true;
            }
        }
        return false;
    }

    private void ChunksToJSON(){
        saveChunks = new List<string>();

        foreach(Chunk chunk in chunks){
            saveChunks.Add(chunk.ChunkToJSON());
        }
    }

    public string ChunkColumnToJSON(){
        ChunksToJSON();
        string save = JsonUtility.ToJson(this);
        return save;
    }

    public static ChunkColumn JsonToChunkColumn(string save){
        ChunkColumn col = JsonUtility.FromJson<ChunkColumn>(save);
        col.loaded = true;
        col = FixPos(col);
        return col;
    }

    private static ChunkColumn FixPos(ChunkColumn col){
        col.columnPos.FixfloatPos();
        foreach(WorldPos pos in col.chunkColumn){
            pos.FixfloatPos();
        }
        return col;
    }

    private void JSONToChunk(){
        string[] save = saveChunks.ToArray();

        int i = 0;
        foreach(Chunk chunk in chunks){
            chunk.JSONToChunk(save[i]);
            i++;
        }

    }

}
