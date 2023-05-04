using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MyThread
{
    private MyThreadPool threadPool;
    private ThreadJob threadJob;
    private Thread thread;

    private bool destroy = false;

    public MyThread(MyThreadPool threadPool){
        this.threadPool = threadPool;
        Start();
    }

    private void Start(){
        thread = new Thread(new ThreadStart(Process));
    }

    public void Destroy(){
        destroy = true;
    }

    private void Process(){
        while(!destroy){
            threadJob = threadPool.GetNextJob();
            if(threadJob == null){
                Thread.Sleep(10);
            }
            else{
                threadJob.Process();
                threadPool.QueuePostProcess(threadJob);
            }
        }
    }
    
}
