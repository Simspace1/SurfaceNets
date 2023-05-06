using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class World : MonoBehaviour
{
    private static World world;

    private string worldName = "world";
    private long worldSeed = 0;

    private WorldData worldData;

    [SerializeField]
    private GameObject chunkPrefab;
    [SerializeField]
    private GameObject chunkPrefab2;
    [SerializeField]
    private GameObject farChunkColumnPrefab;
    [SerializeField]
    private GameObject regionsPrefab;
    [SerializeField]
    private GameObject chunksContainer;
    [SerializeField]
    private GameObject regionsContainer;


    public static WorldPosEqualityComparer worldPosEqC = new WorldPosEqualityComparer();
    public static RegionPosEqualityComparer regionPosEqualityComparer = new RegionPosEqualityComparer();
    public static RegionPosColumnEqualityComparer regionPosColumnEqualityComparer = new RegionPosColumnEqualityComparer();

    private Dictionary<RegionPos, RegionCol> regionsColumns = new Dictionary<RegionPos, RegionCol>(regionPosColumnEqualityComparer);


    private Dictionary<WorldPos, Columns> chunkColumns = new Dictionary<WorldPos, Columns>(worldPosEqC);

    private List<Chunk> chunkUpdates = new List<Chunk>();

    private Dictionary<WorldPos, ChunkRegions> chunkRegions = new Dictionary<WorldPos, ChunkRegions>(worldPosEqC);

    // private static float bottomWorldHeight = -1600;

    public static int maxChunkUpdates = 4;

    // private List<ChunkThread> chunkThreads = new List<ChunkThread>();

    [SerializeField]
    public bool purgeSave  = false ;

    public TerrainGen2 gen {get; private set;}



    // TEST VARS
    RegionCol regionCol;
    bool test = false;
    // bool test1 = false;

    Stopwatch stopwatch;

    // Start is called before the first frame update
    void Start()
    {
        world = this;

        gen = new TerrainGen2(worldSeed);
        if(StaticWorld.worldName != null){
            worldName = StaticWorld.worldName;
            worldSeed = StaticWorld.seed;
        }
        
        
        if(purgeSave || !SaveManager.LoadWorld(this)){
            worldData = new WorldData(worldName,worldSeed);
            SaveManager.SaveWorld(this);
        }


        //TEST CODE
        // stopwatch = new Stopwatch();
        // stopwatch.Start();
        // regionCol = new RegionCol(new RegionPos(0,0,0),false);
        
        // stopwatch.Stop();
        // print("test " + stopwatch.ElapsedMilliseconds);
        

        // Stopwatch stopwatch = new Stopwatch();
        // stopwatch.Start();
        // print("SDist Test: "+TestDistFGen3());
        // stopwatch.Stop();
        // print("Test Time:  " + stopwatch.ElapsedTicks);
    }


    void Update()
    {
        // if(!test){
        //     test = true;
        //     MyThreadPool.QueueJob(new ThreadJobRegionColGen(regionCol));
        // }

        if (chunkUpdates.Count == 0){
            return;
        }

        
        // if(regionCol.generated && !test){
        //     test = true;
        //     print("test ");

        //     regionCol.GetRegion(new RegionPos(0,0,0)).CreateAllChunks();
        //     regionCol.GetRegion(new RegionPos(0,1,0)).CreateAllChunks();

        //     regionCol.GetRegion(new RegionPos(0,0,0)).GenerateAllChunks();
        //     regionCol.GetRegion(new RegionPos(0,1,0)).GenerateAllChunks();

        //     var stopwatch = new Stopwatch();
        //     stopwatch.Start();
        //     regionCol.GetRegion(new RegionPos(0,0,0)).UpdateAllChunks();
        //     regionCol.GetRegion(new RegionPos(0,1,0)).UpdateAllChunks();
        //     stopwatch.Stop();


        //     print("test 2 " + stopwatch.ElapsedMilliseconds + " ms, " +stopwatch.ElapsedTicks + " ticks" );
        // }
        // else if(test && !test1){
        //     test1 = true;
        //     regionCol.GetRegion(new RegionPos(0,0,0)).RenderAllChunks();
        //     regionCol.GetRegion(new RegionPos(0,1,0)).RenderAllChunks();
        // }


        // Stopwatch stopwatch = new Stopwatch();
        // stopwatch.Start();
        SurfacePointsTh();
        // stopwatch.Stop();
        // if(stopwatch.ElapsedMilliseconds > 5){
        //     print("World Total: " + stopwatch.ElapsedMilliseconds);
        // }


    }

    void OnDestroy(){
        // SaveAndQuit();
    }

    public void SaveAll(){
        foreach(var col in chunkColumns){
            if(col.Value.CheckModified()){
                SaveManager.SaveChunkColumn(col.Value.chunkColumn);
            }  
        }
        SaveManager.SaveWorld(this);
    }

    public void SaveAndQuit(){
        foreach(var regions in chunkRegions){
            if(regions.Value.modified){
                SaveManager.SaveChunkRegion(regions.Value);
                regions.Value.modified = false;
            }
        }
    }

    public void Save(){
        foreach(var regions in chunkRegions){
            if(regions.Value.modified){
                SaveManager.SaveChunkRegion(regions.Value);
                regions.Value.modified = false;                
            }
        }
    }

    public static World GetWorld(){
        return world;
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
            // else if(){

            // }
            else if(chunk.updating && i <= maxChunkUpdates){
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
        GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(pos.x,pos.y,pos.z), Quaternion.Euler(Vector3.zero),chunksContainer.transform) as GameObject;
        Chunk newChunk = newChunkObject.GetComponent<Chunk>();
        

        newChunk.SetPos(pos);
        newChunk.SetWorld(this);

        chunkColumn.chunks.Add(newChunk);
        newChunk.col = chunkColumn;

        if(chunkColumn.rendered || chunkColumn.created){
            chunkColumn.col.gen.ChunkGenC2(newChunk);
        }
        // newChunk = chunkColumn.col.gen.ChunkGenC2(newChunk);

        newChunk.update = true;
        chunkUpdates.Add(newChunk);
    }

    public Chunk2 CreateChunk(WorldPos pos, GameObject regionObject){
        GameObject newChunkObject = Instantiate(chunkPrefab2, new Vector3(pos.x,pos.y,pos.z), Quaternion.Euler(Vector3.zero), regionObject.transform) as GameObject;
        Chunk2 newChunk = newChunkObject.GetComponent<Chunk2>();

        return newChunk;
    }


    //Creates chunk at chunkData location
    public void CreateChunkInst(ChunkData chunkData, ChunkColumn chunkColumn){
        GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(chunkData.pos.x,chunkData.pos.y,chunkData.pos.z), Quaternion.Euler(Vector3.zero), chunksContainer.transform) as GameObject;
        Chunk newChunk = newChunkObject.GetComponent<Chunk>();

        newChunk.SetPos(chunkData.pos);
        newChunk.SetWorld(this);

        newChunk.col = chunkColumn;

        chunkColumn.chunks.Add(newChunk);
    }



    public List<Chunk> CreateChunkColumn(ChunkColumn col){
        List<Chunk> chunkList = new List<Chunk>();

        foreach(WorldPos pos in col.chunkColumn){
            GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(pos.x,pos.y,pos.z), Quaternion.Euler(Vector3.zero), chunksContainer.transform) as GameObject;
            Chunk newChunk = newChunkObject.GetComponent<Chunk>();
            
            newChunk.SetPos(pos);
            newChunk.SetWorld(this);
            newChunk.col = col;

            chunkList.Add(newChunk);
        }
        return chunkList;
    }


    //Creates the "holder" gameobject for all chunks in a region
    // DEPRECATED
    public GameObject CreateRegion(RegionPos rPos){
        WorldPos pos = rPos.ToWorldPos();
        GameObject regionObject = Instantiate(regionsPrefab, new Vector3(pos.x,pos.y,pos.z),Quaternion.Euler(Vector3.zero), regionsContainer.transform) as GameObject;
        regionObject.name = "Region "+rPos.ToString();
        return regionObject;
    }

    //Create Region
    public Region CreateRegion(RegionPos rPos, RegionCol rCol){
        WorldPos pos = rPos.ToWorldPos();
        GameObject regionObject = Instantiate(regionsPrefab, new Vector3(pos.x,pos.y,pos.z),Quaternion.Euler(Vector3.zero), regionsContainer.transform) as GameObject;
        regionObject.name = "Region "+rPos.ToString();
        Region region = regionObject.GetComponent<Region>();
        region.SetRegionCol(rCol);
        region.SetRegionPos(rPos);
        return region;
    }


    public FarChunkCol CreateFarChunkColumn(Columns col){
        GameObject newFarChunkColumnObject = Instantiate(farChunkColumnPrefab, new Vector3(col.pos.x,col.pos.y,col.pos.z), Quaternion.Euler(Vector3.zero),chunksContainer.transform) as GameObject;
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
        if(column == null || column.chunkColumn == null || column.chunkColumn.chunks == null){
            return;
        }
        foreach(Chunk chunk in column.chunkColumn.chunks){
            if(chunk == null){
                continue;
            }
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
        if(col == null){
            return null;
        }

        return col.GetChunk(new WorldPos(posx,posy,posz));        
    }

    public Voxel GetVoxel(float x, float y, float z){
        Chunk containerChunk = GetChunk(x,y,z);
        if(containerChunk != null){
            WorldPos chunkPos = containerChunk.GetPos();
            Voxel voxel = containerChunk.GetVoxel(x - chunkPos.x, y - chunkPos.y, z-chunkPos.z);
            return voxel;
        }
        else{
            return new VoxelAir();
        }
    }

    public Voxel GetVoxel(int xi, int yi, int zi){
        Chunk containerChunk = GetChunk(xi,yi,zi);
        if(containerChunk != null){
            WorldPos chunkPos = containerChunk.GetPos();
            Voxel voxel = containerChunk.GetVoxel(xi - chunkPos.xi, yi - chunkPos.yi, zi-chunkPos.zi);
            return voxel;
        }
        else{
            return new VoxelAir();
        }
    }

    public Voxel GetVoxel(WorldPos pos){
        Chunk containerChunk = GetChunk(pos);
        if(containerChunk != null){
            Voxel voxel = containerChunk.GetVoxel(WorldPos.Sub(pos, containerChunk.GetPos()));
            return voxel;
        }
        else{
            return new VoxelAir();
        }
    }

    public Voxel GetVoxelDb(float x, float y, float z){
        Chunk containerChunk = GetChunk(x,y,z);
        if(containerChunk != null){
            WorldPos chunkPos = containerChunk.GetPos();
            Voxel voxel = containerChunk.GetVoxelDb(x - chunkPos.x, y - chunkPos.y, z-chunkPos.z);
            return voxel;
        }
        else{
            return new VoxelAir();
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
            WorldPos chunkPos = chunk.GetPos();
            int xi = pos.xi - chunkPos.xi;
            int yi = pos.yi - chunkPos.yi;
            int zi = pos.zi - chunkPos.zi;
            chunk.SetVoxel(new WorldPos(xi,yi,zi), voxel);
            if(!chunk.col.modified && !chunk.col.col.region.CheckChunkColumns(chunk.GetPos())){
                chunk.col.col.region.AddChunkColumn(chunk.col);
            }
            chunk.modified = true;
            chunk.col.modified = true;
            if(!chunk.update){
                chunk.update = true;
                chunkUpdates.Add(chunk);
            }

            UpdateIfEqual(xi, 1, new WorldPos(chunkPos.xi - Chunk.chunkVoxels,chunkPos.yi, chunkPos.zi));
            UpdateIfEqual(xi, Chunk.chunkVoxels-1, new WorldPos(chunkPos.xi + Chunk.chunkVoxels, chunkPos.yi, chunkPos.zi));
            UpdateIfEqual(yi, 1, new WorldPos(chunkPos.xi, chunkPos.yi-Chunk.chunkVoxels, chunkPos.zi));
            UpdateIfEqual(yi, Chunk.chunkVoxels-1, new WorldPos(chunkPos.xi, chunkPos.yi+Chunk.chunkVoxels, chunkPos.zi));
            UpdateIfEqual(zi, 1, new WorldPos(chunkPos.xi, chunkPos.yi, chunkPos.zi-Chunk.chunkVoxels));
            UpdateIfEqual(zi, Chunk.chunkVoxels-1, new WorldPos(chunkPos.xi, chunkPos.yi, chunkPos.zi+Chunk.chunkVoxels));

        }
    }

    public void SetSDistF(WorldPos pos, Voxel voxel, float sDistf){
        Chunk chunk = GetChunk(pos);
        if(chunk != null){
            WorldPos chunkPos = chunk.GetPos();
            int xi = pos.xi - chunkPos.xi;
            int yi = pos.yi - chunkPos.yi;
            int zi = pos.zi - chunkPos.zi;
            chunk.SetSDistF(new WorldPos(xi,yi,zi), voxel, sDistf);
            if(!chunk.col.modified && !chunk.col.col.region.CheckChunkColumns(chunk.GetPos())){
                chunk.col.col.region.AddChunkColumn(chunk.col);
            }
            chunk.modified = true;
            chunk.col.modified = true;
            if(!chunk.update){
                chunk.update = true;
                chunkUpdates.Add(chunk);
            }

            UpdateIfEqual(xi, 1, new WorldPos(chunkPos.xi - Chunk.chunkVoxels, chunkPos.yi, chunkPos.zi));
            UpdateIfEqual(xi, Chunk.chunkVoxels-1, new WorldPos(chunkPos.xi + Chunk.chunkVoxels, chunkPos.yi, chunkPos.zi));
            UpdateIfEqual(yi, 1, new WorldPos(chunkPos.xi, chunkPos.yi-Chunk.chunkVoxels, chunkPos.zi));
            UpdateIfEqual(yi, Chunk.chunkVoxels-1, new WorldPos(chunkPos.xi, chunkPos.yi+Chunk.chunkVoxels, chunkPos.zi));
            UpdateIfEqual(zi, 1, new WorldPos(chunkPos.xi, chunkPos.yi, chunkPos.zi-Chunk.chunkVoxels));
            UpdateIfEqual(zi, Chunk.chunkVoxels-1, new WorldPos(chunkPos.xi, chunkPos.yi, chunkPos.zi+Chunk.chunkVoxels));

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
                        if (WorldPos.Equals(chunk.GetPos(), pos)){
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
        col.region = ChunkRegions.GetRegions(pos);
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

            if(distance >= columnDistance && !entry.Value.updating){
                toDestroy.Add(entry.Value);
            }
        }
        return toDestroy;

    }

    public void RemoveColumns(WorldPos pos){
        chunkColumns.Remove(pos);
    }

    public ChunkRegions GetChunkRegions(WorldPos pos){
        ChunkRegions region = null;
        chunkRegions.TryGetValue(pos, out region);
        return region;
    }

    public void AddChunkRegions(ChunkRegions region){
        chunkRegions.Add(region.GetPos(), region);
    }

    public void AddRegionCol(RegionCol regionCol){
        regionsColumns.Add(regionCol.regionPos,regionCol);
    }

    public RegionCol GetRegionCol(RegionPos pos){
        RegionCol regionCol = null;
        regionsColumns.TryGetValue(pos,out regionCol);
        return regionCol;
    }


    //Method to identify which regionColumns must be removed
    public List<RegionCol> CheckDestroyRegionColumns(RegionPos playerPos, int loadDistance){
        List<RegionCol> toDestroy = new List<RegionCol>();
        foreach(var colEntry in regionsColumns){
            if(Mathf.Abs(colEntry.Key.x - playerPos.x) > loadDistance || Mathf.Abs(colEntry.Key.z - playerPos.z) > loadDistance){
                toDestroy.Add(colEntry.Value);
            }
        }
        return toDestroy;
    }

    public void DestroyRegionColumns(List<RegionCol> toDestroy){
        foreach(RegionCol regionCol in toDestroy){
            regionsColumns.Remove(regionCol.regionPos);
            regionCol.Destroy();
        }
    }

    public List<Region>[] CheckChangeRegionResolution(RegionPos playerPos, int fullResRegionRadius){
        List<Region>[] regions = new List<Region>[]{new List<Region>(), new List<Region>()};
        List<Region>[] regionsTemp = new List<Region>[2];
        foreach(var colEntry in regionsColumns){
            regionsTemp = colEntry.Value.CheckChangeRegionResolution(playerPos,fullResRegionRadius);
            regions[0].AddRange(regionsTemp[0]);
            regions[1].AddRange(regionsTemp[1]);
        }
        return regions;
    }

}
