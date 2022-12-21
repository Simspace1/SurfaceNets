using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class FarChunkColThread
{
    public List<FarChunkCol> farChunkCols;
    // public Thread createThread;
    public bool rendered = false;
    public bool updateThread = false;

    

    public FarChunkColThread(List<FarChunkCol> farChunkCols){
        this.farChunkCols = farChunkCols;
        // createThread = new Thread(new ThreadStart(() => Create()));
        // createThread.Start();

        updateThread = true;
        ThreadPool.QueueUserWorkItem(Create2, this);
    }

    private void Create2(object state)
    {
        FarChunkColThread farChunkColThread = (FarChunkColThread) state;
        foreach(FarChunkCol col in farChunkCols){
            col.Create();
            col.SetNotCreating();
        }
        updateThread = false;
    }

    public bool CreateCheck(){
        // return createThread.IsAlive;
        return updateThread;
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
