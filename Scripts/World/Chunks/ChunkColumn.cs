using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ChunkColumn
{
    public World world;
    public WorldPos pos;
    public List<WorldPos> chunkColumn = new List<WorldPos>();
    public List<Chunk> chunks;
    public bool rendered = false;
    public bool created = false;
    public bool creating = false;
    public bool rendering = false;
    public bool destroying = false;
    public bool creating2 = false;
    public bool loaded = false;
    public bool modified = false;
    private ColumnData data;
    public string path = Application.persistentDataPath;
    public Columns column;

    public TerrainGen gen;


    private Thread CreateThread;
    private Thread CreateThread2;

    public ChunkColumn(World world, WorldPos pos){
        this.world = world;
        this.pos = pos;
        CreateThread = new Thread(new ThreadStart(() => Create()));
        CreateThread2 = new Thread(new ThreadStart(() => Create2()));
        
    }

    public ChunkColumn(Columns column){
        this.world = column.world;
        this.pos = column.pos;
        this.column = column;
        this.gen = column.gen;
        CreateThread = new Thread(new ThreadStart(() => Create()));
        CreateThread2 = new Thread(new ThreadStart(() => Create2()));
    }


    public void CreateStart(){
        creating = true;
        // CreateThread = new Thread(new ThreadStart(() => Create()));
        CreateThread.Start();
    }

    public void CreateStart2(){
        if(loaded){
            data.ReCreateChunks(this);
        }
        else{
            chunks = world.CreateChunkColumn(chunkColumn);
        }
        // CreateThread2 = new Thread(new ThreadStart(() => Create2()));
        creating2 = true;
        CreateThread2.Start();
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

    public bool CreateCheck(){
        if(CreateThread.IsAlive){
            return true;
        }
        else if(CreateThread2 == null){
            creating2 = true;
            CreateStart2();
            return true;
        }
        else if(CreateThread2.IsAlive){
            return true;
        }
        else{
            return false;
        }
    }

    public bool CreateCheck1(){
        return CreateThread.IsAlive;
    }

    public bool CreateCheck2(){
        return CreateThread2.IsAlive;
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

    public void RenderEnd(){

    }

    public void DestroyEnd(){

    }

    void Create(){
        if(world.GetWorldData().Contains(pos)){
            loaded = true;
            if(gen == null){
                gen = new TerrainGen();
            }
            data = SaveManager.LoadChunkColumn(this);
            data.Revert1(this);
        }
        else{
            if(gen == null){
                gen = new TerrainGen();
                gen.MmTerrainHeight(pos);
            }
            else if(!gen.generated){
                gen.MmTerrainHeight(pos);
            }
            float [] minMax = gen.minMax;

            float min = Mathf.FloorToInt((minMax[0]-Chunk.chunkSize)/Chunk.chunkSize)*Chunk.chunkSize;
            float max = Mathf.CeilToInt((minMax[1]+Chunk.chunkSize)/Chunk.chunkSize)*Chunk.chunkSize;

            WorldPos chunkPos;
            for(float y = min; y <= max; y +=Chunk.chunkSize){
                chunkPos = new WorldPos(pos.x,y,pos.z);
                chunkColumn.Add(chunkPos);
            }
        }
    }

    void Create2(){
        if(loaded){
            data.Revert2(this);
        }
        else{
            foreach(Chunk chunk in chunks){
                gen.ChunkGenC2(chunk);
            }
        }
    }


    public void Render(){
        if(loaded){
            foreach(Chunk chunk in chunks){
                if(chunk.meshData !=null){
                    chunk.RenderMesh(chunk.meshData);
                }
                else{
                    world.AddChunkUpdate(chunk);
                    chunk.update = true;
                }
            }
            rendered = true;
            loaded = false;
        }        
        else{
            foreach(Chunk chunk in chunks){
                world.AddChunkUpdate(chunk);
                chunk.update = true;
            }
            rendered = true;
        }    
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

}
