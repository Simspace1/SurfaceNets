using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class World : MonoBehaviour
{
    private string worldName = "world";
    private ulong worldSeed;

    private WorldData worldData;

    [SerializeField]
    private GameObject chunkPrefab;
    [SerializeField]
    private GameObject farChunkColumnPrefab;


    static WorldPosEqualityComparer WorldPosEqC = new WorldPosEqualityComparer();
    public Dictionary<WorldPos, Chunk> chunks = new Dictionary<WorldPos, Chunk>(WorldPosEqC);

    // public Dictionary<WorldPos, List<WorldPos>> chunkColumns = new Dictionary<WorldPos, List<WorldPos>>(WorldPosEqC);
    // public Dictionary<WorldPos, ChunkColumn> chunkColumns2 = new Dictionary<WorldPos, ChunkColumn>(WorldPosEqC);
    // public Dictionary<WorldPos, FarChunkCol> farChunkColumns = new Dictionary<WorldPos, FarChunkCol>(WorldPosEqC);

    public Dictionary<WorldPos, Columns> chunkColumns = new Dictionary<WorldPos, Columns>(WorldPosEqC);

    public List<Chunk> chunkUpdates = new List<Chunk>();

    private static float bottomWorldHeight = -1600;

    public static int maxChunkUpdates = 4;

    private List<ChunkThread> chunkThreads = new List<ChunkThread>();

    private bool purgeSave = false;

    


    // Start is called before the first frame update
    void Start()
    {
        if(StaticWorld.worldName != null){
            worldName = StaticWorld.worldName;
            worldSeed = StaticWorld.seed;
        }
        
        
        if(purgeSave || !SaveManager.LoadWorld(this)){
            worldData = new WorldData(worldName,worldSeed);
            SaveManager.SaveWorld(this);
        }

        // Stopwatch stopwatch = new Stopwatch();
        // stopwatch.Start();
        // print("SDist Test: "+TestDistFGen3());
        // stopwatch.Stop();
        // print("Test Time:  " + stopwatch.ElapsedTicks);
    }


    void Update()
    {
        if (chunkUpdates.Count == 0){
            return;
        }

        // SurfacePoints(1);
        // SurfacePoints2();
        // SurfacePoints3();

        // SurfacePoints4();
        // SurfacePointsTest();

        // Stopwatch stopwatch = new Stopwatch();
        // stopwatch.Start();
        SurfacePointsTh();
        // stopwatch.Stop();
        // if(stopwatch.ElapsedMilliseconds > 5){
        //     print("World Total: " + stopwatch.ElapsedMilliseconds);
        // }


    }

    void OnDestroy(){
        //MUST FIX HERE SAVE MANAGER AND PLAYERUICONTROLLER
        // SaveManager.SaveAll(this);
    }

    
    // TEST CODE ----------------------------------------------------------------------------------------
    public float TestDistFGen3(){
        float sDistF = 0;
        float y = 5;
        int xi = 1;
        int zi = 1;
        // int yi = Mathf.FloorToInt((y-(chunk.pos.y))/Chunk.voxelSize);
        

        float[,] terrainHeight = new float[3,3];
        terrainHeight[0,0] = 2;
        terrainHeight[0,1] = 2;
        terrainHeight[0,2] = 5;
        terrainHeight[1,0] = 2;
        terrainHeight[2,0] = 2;
        terrainHeight[1,1] = 1;
        terrainHeight[1,2] = 2;
        terrainHeight[2,1] = 2;
        terrainHeight[2,2] = 2;

        bool posit = y >=terrainHeight[xi,zi];
        float height = terrainHeight[xi,zi];
        float offset = Chunk.voxelSize/2;

        if(!HeightCheck(y,xi,zi,posit,height,offset, terrainHeight)){
            sDistF = y - terrainHeight[xi,zi];
        }
        else{
            List<float> dists= new List<float>();
            float dely = Mathf.Abs(y - terrainHeight[xi,zi]);
            
            for(int x = xi-1; x<= xi+1; x++){
                for(int z = zi-1; z<=zi+1; z++){
                    if(x == xi && z == zi){
                        dists.Add(Mathf.Abs(y-terrainHeight[xi,zi]));
                        continue;
                    }

                    // if(SingleHeightCheck(y,x,z,posit,height,offset,terrainHeight)){
                        dists.Add(Intercept2(dely, y,xi,zi,x,z,terrainHeight));
                    // }
                }
            }


            float min = 1000;
            foreach(float dist in dists){
                if(Mathf.Abs(dist) < min){
                    min = dist;
                }
            }
 
            sDistF = min;
            if (!posit){
                sDistF = -sDistF;
            }
        }

        return sDistF;
    }

    // returns true if surounding voxels need to be calculated for sDist generation
    bool HeightCheck(float y, int xi, int zi,bool posit, float height, float offset, float[,] terrainHeight){
        bool val = false;

        for(int x = xi-1; x<= xi+1; x++){
            for(int z = zi-1; z<=zi+1; z++){
                if(x == xi && z == zi){
                    continue;
                }

                // checks if y is above or under terrain height
                if(posit){
                    if(terrainHeight[x,z]-offset >= height && y-terrainHeight[x,z] <= Chunk.sDistLimit){
                        val = true;
                    }
                }
                else{
                    if(terrainHeight[x,z]+offset <= height && terrainHeight[x,z]-y <= Chunk.sDistLimit){
                        val = true;
                    }
                }
            }

        }
        return val;
    }

    //checks a single position to verify if it needs to be calculated for sDist
    bool SingleHeightCheck(float y, int x, int z, bool posit, float height, float offset,  float[,] terrainHeight){
        bool val;
        
        if(posit){
            val = terrainHeight[x,z]-offset >= height;
        }
        else{
            val = terrainHeight[x,z]+offset <= height;
        }

        return val;
    }

    float Intercept2(float delyi, float y, int xi, int zi, int x, int z, float[,] terrainHeight){
        // Stopwatch stopwatch = new Stopwatch();
        // stopwatch.Start();
        float d, theta, delx, dely;
        
        dely = Mathf.Abs(terrainHeight[x,z] - terrainHeight[xi,zi]);
        if(x == xi){
            delx = Mathf.Abs(zi-z)*Chunk.voxelSize;
        }
        else if(z == zi){
            delx = Mathf.Abs(xi-x)*Chunk.voxelSize;
        }
        else{
            delx = Mathf.Sqrt(Mathf.Pow((xi-x)*Chunk.voxelSize, 2) + Mathf.Pow((zi-z)*Chunk.voxelSize, 2));
        }

        theta = Mathf.Atan2(delx,dely);
        d = Mathf.Sin(theta) * delyi;
        // stopwatch.Stop();
        // print("intercept Time:  " + stopwatch.ElapsedTicks);
        return d;
    }
    // END OF TEST CODE -------------------------------------------------------------------------------------

    void SurfacePointsTh(){
        // Stopwatch stopwatch = new Stopwatch();
        // stopwatch.Start();


        int i = 0;
        int j = 0;
        List<Chunk> chunkUpdateRemove = new List<Chunk>();
        foreach(Chunk chunk in chunkUpdates){
            if(chunk.destoying){
                chunkUpdateRemove.Add(chunk);
                continue;
            }
            if(chunk.updating && i <= maxChunkUpdates){
                if(chunk.CheckUpdateTh()){
                    i++;
                    continue;
                }
                else{
                    chunk.EndUpdateTh();
                    chunkUpdateRemove.Add(chunk);
                }
            }
            else {
                if (i > maxChunkUpdates || j > 0 ){
                    continue;
                }
                else if(chunk.update){
                    // if(stopwatch.ElapsedMilliseconds > 5){
                    //     print("SurfPtsTh Start: " + stopwatch.ElapsedMilliseconds+ " Chunk Updates: "+ i);
                    // }
                    chunk.StartUpdateTh();
                    i++;
                    j++;
                }
            }
        }
        foreach(Chunk chunk in chunkUpdateRemove){
            chunkUpdates.Remove(chunk);
        }

        // stopwatch.Stop();
        // if(stopwatch.ElapsedMilliseconds > 5){
        //     print("SurfPtsTh Total: " + stopwatch.ElapsedMilliseconds);
        // }
    }

    

    // Goes throught each of the chunks that require update
    // Creates a job that calculates each of the surface points of each voxels in the chunks
    // void SurfacePoints(int chunkUpdates){
    //     int surfDataLength = chunkUpdates*(Chunk.chunkVoxels+1)*(Chunk.chunkVoxels+1)*(Chunk.chunkVoxels+1);
    //     var surfData = new NativeArray<Chunk.DataSurf>(surfDataLength, Allocator.TempJob);

    //     //instantiates each DataSurf for each chunks
    //     int i = 0;
    //     int j = 0;
    //     foreach(var chunk in chunks){
    //         if (chunk.Value.update){
    //             if(j >= maxChunkUpdates){
    //                 break;
    //             }
    //             for(int xi = 0; xi<=Chunk.chunkVoxels; xi++){
    //                 for(int yi = 0; yi<=Chunk.chunkVoxels; yi++){
    //                     for(int zi = 0; zi<=Chunk.chunkVoxels; zi++){
    //                         surfData[i] = new Chunk.DataSurf(chunk.Value,xi,yi,zi);
    //                         i ++;
    //                     }
    //                 }
    //             }
    //             j++;
    //         }
    //     }

    //     var job = new ChunkUpdaterJob{
    //         ChunkData = surfData
    //     };

    //     var jobHandle = job.Schedule(surfDataLength,1);
    //     jobHandle.Complete();

    //     i = 0;
    //     j = 0;
    //     foreach(var chunk in chunks){
    //         if (chunk.Value.update){
    //             if(j >= maxChunkUpdates){
    //                 break;
    //             }
    //             // chunk.Value.surfPts2 =  new Dictionary<WorldPos, Vector3>(WorldPosEqC);
    //             chunk.Value.surfPts =  new Dictionary<Vector3, SurfPt>();
    //             // chunk.Value.surfPts3 = new Dictionary<Vector3, Vector3>();
    //             for(int xi = 0; xi<=Chunk.chunkVoxels; xi++){
    //                 for(int yi = 0; yi<=Chunk.chunkVoxels; yi++){
    //                     for(int zi = 0; zi<=Chunk.chunkVoxels; zi++){
    //                         if (!surfData[i].surf){
    //                             i++;
    //                             continue;
    //                         }
    //                         else{
    //                             // chunk.Value.surfPts2.Add(new WorldPos(xi,yi,zi),surfData[i].surfPt);
    //                             chunk.Value.surfPts.Add(new Vector3(xi*Chunk.voxelSize,yi*Chunk.voxelSize,zi*Chunk.voxelSize),new SurfPt(surfData[i].surfPt.x,surfData[i].surfPt.y,surfData[i].surfPt.z));
    //                             // chunk.Value.surfPts3.Add(new Vector3(xi*Chunk.voxelSize,yi*Chunk.voxelSize,zi*Chunk.voxelSize),surfData[i].surfPt);
    //                             i++;
    //                         }
    //                     }
    //                 }
    //             }
    //             // Stopwatch stopWatch = new Stopwatch();
    //             // stopWatch.Start();
    //             chunk.Value.UpdateChunk();
    //             // stopWatch.Stop();
    //             // print("time: "+ stopWatch.ElapsedTicks);
    //             j++;
    //             chunk.Value.update = false;
    //         }
    //     }
    //     surfData.Dispose();
    // }

    // // Goes throught each of the chunks that require update
    // // Creates a job that calculates each of the surface points of each voxels in the chunks
    // void SurfacePoints2(){
    //     int surfDataLength = (Chunk.chunkVoxels+1)*(Chunk.chunkVoxels+1)*(Chunk.chunkVoxels+1);        

    //     //instantiates each DataSurf for each chunks
    //     int i = 0;
    //     int j = 0;
    //     foreach(var chunk in chunkUpdates){
    //         if(j >= maxChunkUpdates){
    //             break;
    //         }
    //         i = 0;
    //         var surfData = new NativeArray<Chunk.DataSurf>(surfDataLength, Allocator.TempJob);
    //         for(int xi = 0; xi<=Chunk.chunkVoxels; xi++){
    //             for(int yi = 0; yi<=Chunk.chunkVoxels; yi++){
    //                 for(int zi = 0; zi<=Chunk.chunkVoxels; zi++){
    //                     surfData[i] = new Chunk.DataSurf(chunk,xi,yi,zi);
    //                     i ++;
    //                 }
    //             }
    //         }
    //         var job = new ChunkUpdaterJob{
    //             ChunkData = surfData
    //         };

    //         var jobHandle = job.Schedule(surfDataLength,1);
    //         jobHandle.Complete();

    //         i=0;
    //         chunk.surfPts =  new Dictionary<Vector3, SurfPt>();
    //         for(int xi = 0; xi<=Chunk.chunkVoxels; xi++){
    //             for(int yi = 0; yi<=Chunk.chunkVoxels; yi++){
    //                 for(int zi = 0; zi<=Chunk.chunkVoxels; zi++){
    //                     if (!surfData[i].surf){
    //                         i++;
    //                         continue;
    //                     }
    //                     else{
    //                         chunk.surfPts.Add(new Vector3(xi*Chunk.voxelSize,yi*Chunk.voxelSize,zi*Chunk.voxelSize),new SurfPt(surfData[i].surfPt.x,surfData[i].surfPt.y,surfData[i].surfPt.z));
    //                         i++;
    //                     }
    //                 }
    //             }
    //         }
    //         j++;
    //         chunk.UpdateChunk();
    //         chunk.update = false;
    //         surfData.Dispose();
    //     }
    //     for(int k = 0; k < j; k++){
    //         chunkUpdates.RemoveAt(0);
    //     }
    // }


    // // Goes throught each of the chunks that require update
    // // Creates a job that calculates each of the surface points of each voxels in the chunks
    // //Current functioning one
    // void SurfacePoints3(){
    //     int surfDataLength = (Chunk.chunkVoxels+1)*(Chunk.chunkVoxels+1)*(Chunk.chunkVoxels+1);        

    //     //instantiates each DataSurf for each chunks
    //     int i = 0;
    //     int j = 0;
        
    //     Chunk chunk = chunkUpdates[0];
    //     i = 0;
    //     var surfData = new NativeArray<Chunk.DataSurf>(surfDataLength, Allocator.TempJob);
    //     for(int xi = 0; xi<=Chunk.chunkVoxels; xi++){
    //         for(int yi = 0; yi<=Chunk.chunkVoxels; yi++){
    //             for(int zi = 0; zi<=Chunk.chunkVoxels; zi++){
    //                 surfData[i] = new Chunk.DataSurf(chunk,xi,yi,zi);
    //                 i ++;
    //             }
    //         }
    //     }
    //     var job = new ChunkUpdaterJob{
    //         ChunkData = surfData
    //     };

    //     var jobHandle = job.Schedule(surfDataLength,1);
    //     jobHandle.Complete();

    //     i=0;
    //     chunk.surfPts =  new Dictionary<Vector3, SurfPt>();
    //     for(int xi = 0; xi<=Chunk.chunkVoxels; xi++){
    //         for(int yi = 0; yi<=Chunk.chunkVoxels; yi++){
    //             for(int zi = 0; zi<=Chunk.chunkVoxels; zi++){
    //                 if (!surfData[i].surf){
    //                     i++;
    //                     continue;
    //                 }
    //                 else{
    //                     chunk.surfPts.Add(new Vector3(xi*Chunk.voxelSize,yi*Chunk.voxelSize,zi*Chunk.voxelSize),new SurfPt(surfData[i].surfPt.x,surfData[i].surfPt.y,surfData[i].surfPt.z));
    //                     i++;
    //                 }
    //             }
    //         }
    //     }
    //     j++;
    //     chunk.UpdateChunk();
    //     chunk.update = false;
    //     chunkUpdates.RemoveAt(0);
    //     surfData.Dispose();
        
        
            
        
    // }



    // void SurfacePoints4(){
    //     Chunk chunk = chunkUpdates[0];

    //     var chunkData = new NativeHashMap<Vector3,float>(4913,Allocator.TempJob);
    //     var surfData = new NativeHashMap<Vector3,Vector3>(4913,Allocator.TempJob);

        

    //     float sDistF;
    //     for(int xi = 0; xi<=Chunk.chunkVoxels; xi++){
    //         for(int yi = 0; yi<=Chunk.chunkVoxels; yi++){
    //             for(int zi = 0; zi<=Chunk.chunkVoxels; zi++){
    //                 sDistF = chunk.GetSDistF(xi,yi,zi);
    //                 if(sDistF >= 2 || sDistF <= -2){
    //                     continue;
    //                 }
    //                 else{
    //                     chunkData.Add(new Vector3(xi,yi,zi), sDistF);
    //                 }
    //             }
    //         }
    //     }





    //     chunkData.Dispose();
    //     surfData.Dispose();
    // }


    // void SurfacePointsTest(){
        

    //     var chunkData = new NativeArray<Chunk.ChunkData>(chunkUpdates.Count,Allocator.TempJob);

        
    //     int i = 0;
    //     foreach(var chunk in chunkUpdates){
    //         chunkData[i] = new Chunk.ChunkData();
    //     }

    //     var job = new ChunkUpdaterJob2{
    //         ChunkData = chunkData
    //     };

    //     var jobHandle = job.Schedule(chunkUpdates.Count,1);
    //     jobHandle.Complete();



    //     chunkData.Dispose();
    // }
    // public void CreateChunk(float x, float y,float z){
    //     WorldPos worldPos = new WorldPos(x,y,z);

    //     GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(x,y,z), Quaternion.Euler(Vector3.zero)) as GameObject;
    //     Chunk newChunk = newChunkObject.GetComponent<Chunk>();

    //     newChunk.pos = worldPos;
    //     newChunk.world = this;

    //     chunks.Add(worldPos,newChunk);


    //     //Test Generation
    //     // Voxel voxel = null;
    //     // for (float xi = 0; xi<chunkSize; xi += voxelSize){
    //     //     for (float yi = 0; yi<chunkSize; yi+= voxelSize){
    //     //         for (float zi = 0; zi < chunkSize; zi += voxelSize){
    //     //             if(yi <= 7){
    //     //                 voxel = new Voxel();
    //     //                 voxel.sDistF = -0.2f;
    //     //             }
    //     //             else{
    //     //                 voxel = new Voxel();
    //     //                 voxel.sDistF = +0.2f;
    //     //             }
    //     //             SetVoxel(x+xi,y+yi,z+zi,voxel);
    //     //         }
    //     //     }
    //     // }

        
    //     var terrainGen = new TerrainGen();
    //     newChunk = terrainGen.ChunkGen(newChunk);

    // }

    public void CreateChunk(float x, float y,float z,TerrainGen gen){
        WorldPos worldPos = new WorldPos(x,y,z);

        GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(x,y,z), Quaternion.Euler(Vector3.zero)) as GameObject;
        Chunk newChunk = newChunkObject.GetComponent<Chunk>();

        newChunk.pos = worldPos;
        newChunk.world = this;

        chunks.Add(worldPos,newChunk);

        newChunk = gen.ChunkGenC2(newChunk);

    }

    public void CreateChunk(WorldPos pos, ChunkColumn chunkColumn){
        GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(pos.x,pos.y,pos.z), Quaternion.Euler(Vector3.zero)) as GameObject;
        Chunk newChunk = newChunkObject.GetComponent<Chunk>();

        newChunk.pos = pos;
        newChunk.world = this;

        chunks.Add(pos,newChunk);
        chunkColumn.chunks.Add(newChunk);

        newChunk = chunkColumn.gen.ChunkGenC2(newChunk);

        newChunk.update = true;
        chunkUpdates.Add(newChunk);
    }

    public void CreateChunk(ChunkData chunkData, ChunkColumn chunkColumn){
        GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(chunkData.pos.x,chunkData.pos.y,chunkData.pos.z), Quaternion.Euler(Vector3.zero)) as GameObject;
        Chunk newChunk = newChunkObject.GetComponent<Chunk>();

        newChunk.pos = chunkData.pos;
        newChunk.voxels = chunkData.voxels;
        newChunk.world = this;
        // newChunk.sDists = chunkData.sDists;
        newChunk.meshData = chunkData.meshData.Revert();

        chunks.Add(chunkData.pos,newChunk);
        chunkColumn.chunks.Add(newChunk);
    }

    public void CreateChunkInst(ChunkData chunkData, ChunkColumn chunkColumn){
        GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(chunkData.pos.x,chunkData.pos.y,chunkData.pos.z), Quaternion.Euler(Vector3.zero)) as GameObject;
        Chunk newChunk = newChunkObject.GetComponent<Chunk>();

        newChunk.pos = chunkData.pos;
        newChunk.world = this;

        chunks.Add(chunkData.pos,newChunk);
        chunkColumn.chunks.Add(newChunk);
    }



    public List<Chunk> CreateChunkColumn(List<WorldPos> column){
        List<Chunk> chunkList = new List<Chunk>();

        foreach(WorldPos pos in column){
            GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(pos.x,pos.y,pos.z), Quaternion.Euler(Vector3.zero)) as GameObject;
            Chunk newChunk = newChunkObject.GetComponent<Chunk>();
            
            newChunk.pos = pos;
            newChunk.world = this;

            chunkList.Add(newChunk);
            chunks.Add(pos,newChunk);
        }
        return chunkList;
    }

    // public FarChunkCol CreateFarChunkColumn(WorldPos pos){
    //     GameObject newFarChunkColumnObject = Instantiate(farChunkColumnPrefab, new Vector3(pos.x,pos.y,pos.z), Quaternion.Euler(Vector3.zero)) as GameObject;
    //     FarChunkCol newFarChunkColumn = newFarChunkColumnObject.GetComponent<FarChunkCol>();

    //     newFarChunkColumn.pos = pos;
    //     newFarChunkColumn.world = this;
    //     newFarChunkColumn.CreateFilter();

    //     farChunkColumns.Add(pos, newFarChunkColumn);
    //     return newFarChunkColumn;
    // }

    public FarChunkCol CreateFarChunkColumn(Columns col){
        GameObject newFarChunkColumnObject = Instantiate(farChunkColumnPrefab, new Vector3(col.pos.x,col.pos.y,col.pos.z), Quaternion.Euler(Vector3.zero)) as GameObject;
        FarChunkCol newFarChunkColumn = newFarChunkColumnObject.GetComponent<FarChunkCol>();

        newFarChunkColumn.pos = col.pos;
        newFarChunkColumn.world = this;
        newFarChunkColumn.CreateFilter();
        newFarChunkColumn.column = col;
        newFarChunkColumn.gen = col.gen;

        // farChunkColumns.Add(col.pos, newFarChunkColumn);
        return newFarChunkColumn;
    }

    // public void DestroyFarChunkColumn(WorldPos pos){
    //     FarChunkCol col;
    //     if(farChunkColumns.TryGetValue(pos, out col)){
    //         Object.Destroy(col.gameObject);
    //         farChunkColumns.Remove(pos);
    //     }
    // }

    public void DestroyFarChunkColumn(Columns col){
        if(col.farChunkCol != null){
            Object.Destroy(col.farChunkCol.gameObject);
            col.farChunkCol = null;
        }
        
    }
    

    // public void UpdateChunkColumn(float x, float z){
    //     List<WorldPos> chunkColumn;
    //     if(chunkColumns.TryGetValue(new WorldPos(x,0,z), out chunkColumn)){
    //         Chunk chunk;
    //         foreach(var pos in chunkColumn){
    //             chunk = GetChunk(pos.x,pos.y,pos.z);
    //             chunkUpdates.Add(chunk);
    //             chunk.update = true;
    //         }
    //     }
    // }

    public void DestroyChunk(float x, float y, float z){
        Chunk chunk = null;
        if (chunks.TryGetValue(new WorldPos(x,y,z), out chunk)){
            //Insert saving here

            Object.Destroy(chunk.gameObject);
            chunks.Remove(new WorldPos(x,y,z));
        }
    }

    // public void DestroyChunkColumn(float x, float y, float z){
    //     List<WorldPos> chunkColumn;
    //     if(chunkColumns.TryGetValue(new WorldPos(x,y,z), out chunkColumn)){
    //         //Insert saving of chunkColumn here
    //         int count = chunkColumn.Count;
    //         for (int i = 0; i < count; i++){
    //             DestroyChunk(chunkColumn[0].x,chunkColumn[0].y, chunkColumn[0].z);
    //             chunkColumn.RemoveAt(0);
    //         }

    //         chunkColumns.Remove(new WorldPos(x,y,z));
    //     }
    // }

    // public void DestroyChunkColumn2(WorldPos pos){
    //     ChunkColumn chunkColumn;
    //     if(chunkColumns2.TryGetValue(pos, out chunkColumn)){
    //         // SaveManager.SaveChunkColumn(chunkColumn);

    //         foreach(Chunk chunk in chunkColumn.chunks){
    //             //Insert Saving of chunk here
                
    //             Object.Destroy(chunk.gameObject);
    //             chunk.destoying = true;
    //             chunks.Remove(chunk.pos);
    //         }

    //         chunkColumn.destroying = true;
    //         chunkColumns2.Remove(pos);
    //     }
    // }

    public void DestroyChunkColumn(Columns column){

        foreach(Chunk chunk in column.chunkColumn.chunks){
            //Insert Saving of chunk here
            
            Object.Destroy(chunk.gameObject);
            chunk.destoying = true;
            chunks.Remove(chunk.pos);
        }

        column.chunkColumn.destroying = true;
    }

    public Chunk GetChunk(float x, float y,float z){
        WorldPos pos = new WorldPos(x,y,z);
        float multiple = Chunk.chunkSize;
        float posx = Mathf.FloorToInt(x/multiple)*multiple;
        float posy = Mathf.FloorToInt(y/multiple)*multiple;
        float posz = Mathf.FloorToInt(z/multiple)*multiple;
        pos.SetPos(posx,posy,posz);
        Chunk containerChunk = null;
        chunks.TryGetValue(pos, out containerChunk);
        return containerChunk;
    }

    public Chunk GetChunk(int xi, int yi, int zi){
        // WorldPos pos = new WorldPos(xi,yi,zi);
        int multiple = Chunk.chunkVoxels;
        float multiplef = Chunk.chunkVoxels;
        int posx = Mathf.FloorToInt(xi/multiplef)*multiple;
        int posy = Mathf.FloorToInt(yi/multiplef)*multiple;
        int posz = Mathf.FloorToInt(zi/multiplef)*multiple;
        WorldPos pos = new WorldPos(posx,posy,posz);
        // pos.SetPos(posx,posy,posz);
        Chunk containerChunk = null;
        chunks.TryGetValue(pos, out containerChunk);
        return containerChunk;
    }

    public Chunk GetChunk(WorldPos posin){
        int multiple = Chunk.chunkVoxels;
        float multiplef = Chunk.chunkVoxels;
        int posx = Mathf.FloorToInt(posin.xi/multiplef)*multiple;
        int posy = Mathf.FloorToInt(posin.yi/multiplef)*multiple;
        int posz = Mathf.FloorToInt(posin.zi/multiplef)*multiple;
        WorldPos pos = new WorldPos(posx,posy,posz);
        Chunk containerChunk = null;
        chunks.TryGetValue(pos, out containerChunk);
        return containerChunk;
    }

    public Voxel GetVoxel(float x, float y, float z){
        Chunk containerChunk = GetChunk(x,y,z);
        if(containerChunk != null){
            Voxel voxel = containerChunk.GetVoxel(x - containerChunk.pos.x, y - containerChunk.pos.y, z-containerChunk.pos.z);
            return voxel;
        }
        else{
            return new Voxel();
        }
    }

    public Voxel GetVoxel(int xi, int yi, int zi){
        Chunk containerChunk = GetChunk(xi,yi,zi);
        if(containerChunk != null){
            Voxel voxel = containerChunk.GetVoxel(xi - containerChunk.pos.xi, yi - containerChunk.pos.yi, zi-containerChunk.pos.zi);
            return voxel;
        }
        else{
            return new Voxel();
        }
    }

    public Voxel GetVoxel(WorldPos pos){
        Chunk containerChunk = GetChunk(pos);
        if(containerChunk != null){
            Voxel voxel = containerChunk.GetVoxel(WorldPos.Sub(pos, containerChunk.pos));
            return voxel;
        }
        else{
            return new Voxel();
        }
    }

    public Voxel GetVoxelDb(float x, float y, float z){
        Chunk containerChunk = GetChunk(x,y,z);
        if(containerChunk != null){
            Voxel voxel = containerChunk.GetVoxelDb(x - containerChunk.pos.x, y - containerChunk.pos.y, z-containerChunk.pos.z);
            return voxel;
        }
        else{
            return new Voxel();
        }
    }

    // public float GetSDistF(int xi, int yi, int zi){
    //     Chunk containerChunk = GetChunk(xi,yi,zi);
    //     if(containerChunk != null){
    //         float sDistF = containerChunk.GetSDistF(xi - containerChunk.pos.xi, yi - containerChunk.pos.yi, zi-containerChunk.pos.zi);
    //         return sDistF;
    //     }
    //     else{
    //         return 1f;
    //     }
    // }

    // public void SetSDistF(int xi, int yi, int zi, float sDistF){
    //     Chunk chunk = GetChunk(xi,yi,zi);
    //     if(chunk != null){
    //         chunk.SetSDistF(xi-chunk.pos.xi, yi-chunk.pos.yi, zi-chunk.pos.zi,sDistF);
    //         chunk.update = true;
    //     }
    // }

    // public void SetVoxel(float x, float y, float z, Voxel voxel, float sDistF){
    //     Chunk chunk = GetChunk(x,y,z);
    //     if(chunk != null){
    //         chunk.SetVoxel(x-chunk.pos.x, y-chunk.pos.y, z-chunk.pos.z,voxel,sDistF);
    //         chunk.update = true;

    //         UpdateIfEqual(x - chunk.pos.x, voxelSize, new WorldPos(x - 1, y, z));
    //         UpdateIfEqual(x - chunk.pos.x, Chunk.chunkSize - voxelSize, new WorldPos(x + 1, y, z));
    //         UpdateIfEqual(y - chunk.pos.y, voxelSize, new WorldPos(x, y - 1, z));
    //         UpdateIfEqual(y - chunk.pos.y, Chunk.chunkSize - voxelSize, new WorldPos(x, y + 1, z));
    //         UpdateIfEqual(z - chunk.pos.z, voxelSize, new WorldPos(x, y, z - 1));
    //         UpdateIfEqual(z - chunk.pos.z, Chunk.chunkSize - voxelSize, new WorldPos(x, y, z + 1));
    //     }        
    // }

    public void SetVoxel(WorldPos pos, Voxel voxel){
        Chunk chunk = GetChunk(pos);
        if(chunk != null){
            int xi = pos.xi - chunk.pos.xi;
            int yi = pos.yi - chunk.pos.yi;
            int zi = pos.zi - chunk.pos.zi;
            chunk.SetVoxel(new WorldPos(xi,yi,zi), voxel);
            chunk.modified = true;
            if(!chunk.update){
                chunk.update = true;
                chunkUpdates.Add(chunk);
            }

            UpdateIfEqual(xi, 1, new WorldPos(chunk.pos.xi - Chunk.chunkVoxels, chunk.pos.yi, chunk.pos.zi));
            UpdateIfEqual(xi, Chunk.chunkVoxels-1, new WorldPos(chunk.pos.xi + Chunk.chunkVoxels, chunk.pos.yi, chunk.pos.zi));
            UpdateIfEqual(yi, 1, new WorldPos(chunk.pos.xi, chunk.pos.yi-Chunk.chunkVoxels, chunk.pos.zi));
            UpdateIfEqual(yi, Chunk.chunkVoxels-1, new WorldPos(chunk.pos.xi, chunk.pos.yi+Chunk.chunkVoxels, chunk.pos.zi));
            UpdateIfEqual(zi, 1, new WorldPos(chunk.pos.xi, chunk.pos.yi, chunk.pos.zi-Chunk.chunkVoxels));
            UpdateIfEqual(zi, Chunk.chunkVoxels-1, new WorldPos(chunk.pos.xi, chunk.pos.yi, chunk.pos.zi+Chunk.chunkVoxels));

        }
    }

    public void SetSDistF(WorldPos pos, Voxel voxel, float sDistf){
        Chunk chunk = GetChunk(pos);
        if(chunk != null){
            int xi = pos.xi - chunk.pos.xi;
            int yi = pos.yi - chunk.pos.yi;
            int zi = pos.zi - chunk.pos.zi;
            chunk.SetSDistF(new WorldPos(xi,yi,zi), voxel, sDistf);
            chunk.modified = true;
            if(!chunk.update){
                chunk.update = true;
                chunkUpdates.Add(chunk);
            }

            UpdateIfEqual(xi, 1, new WorldPos(chunk.pos.xi - Chunk.chunkVoxels, chunk.pos.yi, chunk.pos.zi));
            UpdateIfEqual(xi, Chunk.chunkVoxels-1, new WorldPos(chunk.pos.xi + Chunk.chunkVoxels, chunk.pos.yi, chunk.pos.zi));
            UpdateIfEqual(yi, 1, new WorldPos(chunk.pos.xi, chunk.pos.yi-Chunk.chunkVoxels, chunk.pos.zi));
            UpdateIfEqual(yi, Chunk.chunkVoxels-1, new WorldPos(chunk.pos.xi, chunk.pos.yi+Chunk.chunkVoxels, chunk.pos.zi));
            UpdateIfEqual(zi, 1, new WorldPos(chunk.pos.xi, chunk.pos.yi, chunk.pos.zi-Chunk.chunkVoxels));
            UpdateIfEqual(zi, Chunk.chunkVoxels-1, new WorldPos(chunk.pos.xi, chunk.pos.yi, chunk.pos.zi+Chunk.chunkVoxels));

        }
    }

    // public void SetVoxelsDistF(float x,float y,float z,float sDistF){
    //     Chunk chunk = GetChunk(x,y,z);
    //     if(chunk != null){
    //         chunk.SetVoxelsDistF(x-chunk.pos.x, y-chunk.pos.y, z-chunk.pos.z,sDistF);
    //         chunk.update = true;

    //         UpdateIfEqual(x - chunk.pos.x, voxelSize, new WorldPos(x - 1, y, z));
    //         UpdateIfEqual(x - chunk.pos.x, Chunk.chunkSize - voxelSize , new WorldPos(x + 1, y, z));
    //         UpdateIfEqual(y - chunk.pos.y, voxelSize, new WorldPos(x, y - 1, z));
    //         UpdateIfEqual(y - chunk.pos.y, Chunk.chunkSize - voxelSize, new WorldPos(x, y + 1, z));
    //         UpdateIfEqual(z - chunk.pos.z, voxelSize, new WorldPos(x, y, z - 1));
    //         UpdateIfEqual(z - chunk.pos.z, Chunk.chunkSize - voxelSize, new WorldPos(x, y, z + 1));
    //     }
    // }

    void UpdateIfEqual(int val1, int val2, WorldPos pos){  
        if(val1 == val2){
            // Chunk chunk;
            // if (chunks.TryGetValue(pos, out chunk)){
            //     if(!chunk.update){
            //         chunk.update = true;
            //         chunkUpdates.Add(chunk);
            //     }
            // }
            // else{
                

            //     // ChunkColumn chunkColumn;
            //     // if(chunkColumns2.TryGetValue(new WorldPos(pos.xi,0,pos.zi), out chunkColumn)){
                    // // CreateChunk(pos,chunkColumn);
            //     // }  
            // }


            Columns column;
            if(chunkColumns.TryGetValue(new WorldPos(pos.xi,0,pos.zi), out column)){
                if(column.chunkColumn != null){
                    bool found = false;
                    foreach(Chunk chunk in column.chunkColumn.chunks){
                        if (WorldPos.Equals(chunk.pos, pos)){
                            found = true;
                            if(chunk.update){
                                break;
                            }
                            else{
                                chunk.update = true;
                                chunkUpdates.Add(chunk);
                                break;
                            }
                        }
                    }

                    if (!found){
                        CreateChunk(pos,column.chunkColumn);
                    }
                }
                else{
                    LoadChunks.CreateChunkColumn(column);
                }
            }
        }
    }

    void UpdateIfEqual(float value1, float value2, WorldPos pos){
        if(value1 == value2){
            Chunk chunk = GetChunk(pos.x,pos.y,pos.z);
            if(chunk!=null){
                chunkUpdates.Add(chunk);
                chunk.update = true;
            } 
        }
    }

    public string GetWorldName(){
        return worldName;
    }

    public ulong GetWorldSeed(){
        return worldSeed;
    }

    public void SetWorldData(WorldData wData){
        worldName = wData.worldName;
        worldSeed = wData.worldSeed;
        worldData = wData;
    }

    public WorldData GetWorldData(){
        return worldData;
    }
}
