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
    private long worldSeed = 0;

    private WorldData worldData;

    [SerializeField]
    private GameObject chunkPrefab;
    [SerializeField]
    private GameObject farChunkColumnPrefab;


    static WorldPosEqualityComparer WorldPosEqC = new WorldPosEqualityComparer();


    private Dictionary<WorldPos, Columns> chunkColumns = new Dictionary<WorldPos, Columns>(WorldPosEqC);

    private List<Chunk> chunkUpdates = new List<Chunk>();

    private static float bottomWorldHeight = -1600;

    public static int maxChunkUpdates = 4;

    private List<ChunkThread> chunkThreads = new List<ChunkThread>();

    [SerializeField]
    private bool purgeSave = false;

    public TerrainGen2 gen {get; private set;}


    // Start is called before the first frame update
    void Start()
    {
        gen = new TerrainGen2(worldSeed);
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


        // Stopwatch stopwatch = new Stopwatch();
        // stopwatch.Start();
        SurfacePointsTh();
        // stopwatch.Stop();
        // if(stopwatch.ElapsedMilliseconds > 5){
        //     print("World Total: " + stopwatch.ElapsedMilliseconds);
        // }


    }

    void OnDestroy(){
        SaveAll();
    }

    public void SaveAll(){
        foreach(var col in chunkColumns){
            if(col.Value.CheckModified()){
                SaveManager.SaveChunkColumn(col.Value.chunkColumn);
            }  
        }
        SaveManager.SaveWorld(this);
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


    public void CreateChunk(WorldPos pos, ChunkColumn chunkColumn){
        GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(pos.x,pos.y,pos.z), Quaternion.Euler(Vector3.zero)) as GameObject;
        Chunk newChunk = newChunkObject.GetComponent<Chunk>();

        newChunk.SetPos(pos);
        newChunk.SetWorld(this);

        chunkColumn.chunks.Add(newChunk);

        // newChunk = chunkColumn.col.gen.ChunkGenC2(newChunk);

        newChunk.update = true;
        chunkUpdates.Add(newChunk);
    }


    //Creates chunk at chunkData location
    public void CreateChunkInst(ChunkData chunkData, ChunkColumn chunkColumn){
        GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(chunkData.pos.x,chunkData.pos.y,chunkData.pos.z), Quaternion.Euler(Vector3.zero)) as GameObject;
        Chunk newChunk = newChunkObject.GetComponent<Chunk>();

        newChunk.SetPos(chunkData.pos);
        newChunk.SetWorld(this);

        chunkColumn.chunks.Add(newChunk);
    }



    public List<Chunk> CreateChunkColumn(List<WorldPos> column){
        List<Chunk> chunkList = new List<Chunk>();

        foreach(WorldPos pos in column){
            GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(pos.x,pos.y,pos.z), Quaternion.Euler(Vector3.zero)) as GameObject;
            Chunk newChunk = newChunkObject.GetComponent<Chunk>();
            
            newChunk.SetPos(pos);
            newChunk.SetWorld(this);

            chunkList.Add(newChunk);
        }
        return chunkList;
    }


    public FarChunkCol CreateFarChunkColumn(Columns col){
        GameObject newFarChunkColumnObject = Instantiate(farChunkColumnPrefab, new Vector3(col.pos.x,col.pos.y,col.pos.z), Quaternion.Euler(Vector3.zero)) as GameObject;
        FarChunkCol newFarChunkColumn = newFarChunkColumnObject.GetComponent<FarChunkCol>();

        newFarChunkColumn.SetPos(col.pos);
        newFarChunkColumn.SetWorld(this);
        newFarChunkColumn.CreateFilter();
        newFarChunkColumn.SetColumn(col);

        return newFarChunkColumn;
    }


    public void DestroyFarChunkColumn(Columns col){
        if(col.farChunkCol != null){
            Object.Destroy(col.farChunkCol.gameObject);
            col.farChunkCol = null;
        }
        
    }

    public void DestroyChunkColumn(Columns column){

        foreach(Chunk chunk in column.chunkColumn.chunks){
            //Insert Saving of chunk here
            
            Object.Destroy(chunk.gameObject);
            chunk.destoying = true;
        }

        column.chunkColumn.destroying = true;
    }

    public Chunk GetChunk(float x, float y,float z){
        WorldPos pos = new WorldPos(x,y,z);
        return GetChunk(pos);
    }

    public Chunk GetChunk(int xi, int yi, int zi){
        WorldPos pos = new WorldPos(xi,yi,zi);
        return GetChunk(pos);
    }

    private Columns GetColumn(WorldPos posin){
        if(posin.yi != 0){
            posin.SetPos(posin.xi,0,posin.zi);
        }

        Columns col = null;
        chunkColumns.TryGetValue(posin ,out col);
        return col;
    }

    public Chunk GetChunk(WorldPos posin){
        int multiple = Chunk.chunkVoxels;
        float multiplef = Chunk.chunkVoxels;
        int posx = Mathf.FloorToInt(posin.xi/multiplef)*multiple;
        int posy = Mathf.FloorToInt(posin.yi/multiplef)*multiple;
        int posz = Mathf.FloorToInt(posin.zi/multiplef)*multiple;

        Columns col = GetColumn(new WorldPos(posx,0,posz));

        Chunk chunk = col.GetChunk(new WorldPos(posx,posy,posz));        
        return chunk;
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

    public long GetWorldSeed(){
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

    public int GetChunkUpdateCount(){
        return chunkUpdates.Count;
    }

    public void AddChunkUpdate(Chunk chunk){
        chunkUpdates.Add(chunk);
    }

    public void AddColumns(WorldPos pos, Columns col){
        chunkColumns.Add(pos, col);
    }

    public Columns TryGetColumn(WorldPos pos){
        Columns column;
        bool check = chunkColumns.TryGetValue(pos, out column);

        if(check){
            return column; 
        }
        else{
            return null;
        }
    }

    public List<Columns> CheckDestroyColumn(Vector3 ppos, float chunkDistance, float columnDistance){

        float distance;
        List<Columns> toDestroy = new List<Columns>();
        foreach (var entry in chunkColumns){
            distance = Vector3.Distance(new Vector3(entry.Key.x,0,entry.Key.z), ppos);

            if (entry.Value.chunkColumn != null && distance >= chunkDistance){
                entry.Value.DestroyChunkColumn();
            }

            if(distance >= columnDistance){
                toDestroy.Add(entry.Value);
            }
        }
        return toDestroy;

    }

    public void RemoveColumns(WorldPos pos){
        chunkColumns.Remove(pos);
    }
}
