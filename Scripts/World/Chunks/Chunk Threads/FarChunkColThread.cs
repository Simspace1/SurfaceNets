using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class FarChunkColThread
{
    public List<FarChunkCol> farChunkCols;
    public Thread createThread;
    public bool rendered = false;

    public FarChunkColThread(List<FarChunkCol> farChunkCols){
        this.farChunkCols = farChunkCols;
        createThread = new Thread(new ThreadStart(() => Create()));
        createThread.Start();
    }

    public bool CreateCheck(){
        return createThread.IsAlive;
    }

    private void Create(){
        foreach(FarChunkCol col in farChunkCols){
            col.Create();
        }
    }

    public void Render(){
        rendered = true;
        foreach(FarChunkCol col in farChunkCols){
            col.Render();
        }
    }
}
