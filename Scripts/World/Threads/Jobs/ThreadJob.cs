using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ThreadJob
{
    public bool postProcess {get; private set;}

    public ThreadJob(bool postProcess){
        this.postProcess = postProcess;
    }

    public abstract void Process();
    public abstract void PostProcess();
}
