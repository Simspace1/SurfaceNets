using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class ChunkThread 
{
    public Thread thread;
    public Chunk chunk;

    public ChunkThread(Chunk chunk){
        this.chunk = chunk;
        thread = new Thread(new ThreadStart(() => ThreadSurface()));
    }

    public void Start(){
        thread.Start();
    }

    public void Render(){
        chunk.RenderMesh(chunk.meshData);
    }

    public bool IsAlive(){
        return thread.IsAlive;
    }

    void ThreadSurface(){
        chunk.UpdateChunk();
    }

}
