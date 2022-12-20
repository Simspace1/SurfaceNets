using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Columns 
{
    public FarChunkCol farChunkCol;
    public ChunkColumn chunkColumn;
    public World world;
    public WorldPos pos;
    public ColumnGen gen {get; private set;}

    public bool updating = false;

    public Columns(World world, WorldPos pos, ColumnGen gen){
        this.world = world;
        this.pos = pos;
        this.gen = gen;
    }

    // public Columns(FarChunkCol farChunkCol){
    //     this.farChunkCol = farChunkCol;
    //     this.pos = farChunkCol.pos;
    //     this.gen = farChunkCol.gen;
    //     this.world = farChunkCol.world;
    //     farChunkCol.SetColumn(this);
    // }

    // public Columns(ChunkColumn chunkColumn){
    //     this.chunkColumn = chunkColumn;
    //     this.pos = chunkColumn.pos;
    //     this.gen = chunkColumn.gen;
    //     this.world = chunkColumn.world;
    //     chunkColumn.column = this;
    // }

    // public Columns(FarChunkCol farChunkCol, ChunkColumn chunkColumn){
    //     this.farChunkCol = farChunkCol;
    //     this.chunkColumn = chunkColumn;
    //     if(WorldPos.Equals(chunkColumn.pos, farChunkCol.pos)){
    //         this.pos = farChunkCol.pos;
    //         this.gen = farChunkCol.gen;
    //         this.world = chunkColumn.world;
    //         farChunkCol.SetColumn(this);
    //         chunkColumn.column = this;
    //     }
    //     else{
    //         throw new System.Exception("Columns WorldPos not equal");
    //     }
    // }

    public void CreateFarChunkCol(){
        farChunkCol = world.CreateFarChunkColumn(this);
    }

    public void CreateChunkColumn(){
        chunkColumn = new ChunkColumn(this);
    }

    public void RenderEndChunkColumn(){
        if (farChunkCol != null){
            farChunkCol.UnRender();
        }
    }

    public void DestroyChunkColumn(){
        if(chunkColumn != null && !chunkColumn.destroying){
            if(farChunkCol != null && !farChunkCol.render){
                farChunkCol.Render();
            }
            world.DestroyChunkColumn(this);
            chunkColumn = null;
        }
    }

    public static void Destroy(Columns column){
        LoadChunks.RemoveColumnFLists(column);
        if(column.farChunkCol != null){
            column.world.DestroyFarChunkColumn(column);
        }

        if(column.chunkColumn != null && !column.chunkColumn.destroying){
            column.world.DestroyChunkColumn(column);
        }
        if(column.chunkColumn != null || column.farChunkCol != null){
            return;
        }
        column.world.RemoveColumns(column.pos);
    }

    public bool CheckModified(){
        if(chunkColumn == null){
            return false;
        }
        else{
            return chunkColumn.CheckModified();
        }
    }

    public Chunk GetChunk(WorldPos pos){
        if(chunkColumn == null){
            return null;
        }
        else{
            return chunkColumn.GetChunk(pos);
        }
    }

    public void SetGen(ColumnGen gen){
        if(this.gen == null){
            this.gen = gen;
        }
    }
}
