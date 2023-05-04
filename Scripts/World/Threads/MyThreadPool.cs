using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyThreadPool
{
    public const int threadsCount = 4;

    private Queue<ThreadJobChunk> chunkQueue = new Queue<ThreadJobChunk>();
    private Queue<ThreadJob> jobQueue = new Queue<ThreadJob>();

    private Queue<ThreadJobChunk> postChunkQueue = new Queue<ThreadJobChunk>();
    private Queue<ThreadJob> postJobQueue = new Queue<ThreadJob>();

    private List<MyThread> threads = new List<MyThread>();

    public MyThreadPool(){
        for(int i = 0; i < 4; i++){
            MyThread newThread = new MyThread(this);
            threads.Add(newThread);
        }
    }

    public ThreadJob GetNextJob(){
        if(chunkQueue.Count > 0 ){
            return chunkQueue.Dequeue();
        }
        else if(jobQueue.Count > 0){
            return jobQueue.Dequeue();
        }
        else{
            return null;
        }
    }

    public void FinishJob(ThreadJob threadJob){

    }



}
