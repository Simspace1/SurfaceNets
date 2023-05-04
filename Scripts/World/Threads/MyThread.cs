using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MyThread
{
    private ThreadJob threadJob;
    private Thread thread;

    private bool destroy = false;

    public MyThread(){
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
            threadJob = MyThreadPool.GetNextJob();
            if(threadJob == null){
                Thread.Sleep(10);
            }
            else{
                threadJob.Process();
                MyThreadPool.QueuePostProcess(threadJob);
            }
        }
    }
    
}
