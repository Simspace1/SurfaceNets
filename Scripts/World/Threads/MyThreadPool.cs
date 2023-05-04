using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyThreadPool : MonoBehaviour
{
    public const int threadsCount = 4;
    private static MyThreadPool myThreadPool;

    private Queue<ThreadJobChunk> chunkQueue = new Queue<ThreadJobChunk>();
    private Queue<ThreadJob> jobQueue = new Queue<ThreadJob>();

    private Queue<ThreadJobChunk> postChunkQueue = new Queue<ThreadJobChunk>();
    private Queue<ThreadJob> postJobQueue = new Queue<ThreadJob>();

    private readonly object syncLock = new object();

    private List<MyThread> threads = new List<MyThread>();

    void Start(){
        myThreadPool = this;
        for(int i = 0; i < 4; i++){
            MyThread newThread = new MyThread(this);
            threads.Add(newThread);
        }
    }

    void Update(){
        PostProcessJob();
    }

    void OnDestroy(){
        foreach(MyThread thread in threads){
            thread.Destroy();
        }
    }


    public ThreadJob GetNextJob(){
        lock(syncLock){
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
    }

    public void FinishJob(ThreadJob job){
        if(job is ThreadJobChunk){
            postChunkQueue.Enqueue((ThreadJobChunk) job);
        }
        else{
            postJobQueue.Enqueue(job);
        }
    }

    public static MyThreadPool GetThreadPool(){
        return myThreadPool;
    }

    public void QueueJob(ThreadJob job){
        jobQueue.Enqueue(job);
    }

    public void QueueJob(ThreadJobChunk job){
        chunkQueue.Enqueue(job);
    }

    private void PostProcessJob(){
        if(postJobQueue.Count > 0){
            ThreadJob job = postJobQueue.Dequeue();
            if(job.postProcess){
                job.PostProcess();
            }
        }
    }



}