using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

// public class ChunkUpdater : MonoBehaviour
// {
//     public World world;
//     private Dictionary<WorldPos, Chunk> chunks;
//     // Update is called once per frame
//     void Start()
//     {
//         chunks = world.chunks;
//     }

//     void Update()
//     {
//         var job = new ChunkUpdaterJob();
//         var jobHandle = job.Schedule(chunks.Count,1);
//         jobHandle.Complete();

//     }
// }

// struct ChunkUpdaterJob : IJobParallelFor{
//     public NativeArray<Chunk.DataSurf> ChunkData;

//     public void Execute(int index){
//         var data = ChunkData[index];
//         data.SurfacePoint();
//         ChunkData[index] = data;
//     }
// }

// struct ChunkUpdaterJob2 : IJobParallelFor{
//     public NativeArray<Chunk.ChunkData> ChunkData;

//     public void Execute(int index){
       
//     }
// }