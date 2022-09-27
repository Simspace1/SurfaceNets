using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class LoadChunks : MonoBehaviour
{

    [SerializeField]
    private World world;

    private List<WorldPos> updateList = new List<WorldPos>();
    private List<WorldPos> buildList = new List<WorldPos>();

    // private List<ChunkColumn> createList = new List<ChunkColumn>();
    // private List<ChunkColumn> renderList = new List<ChunkColumn>();
    // private List<ChunkColumnFarAndClose> destroyList = new List<ChunkColumnFarAndClose>();

    static List<Columns> farCreateList = new List<Columns>();
    static List<Columns> createList1 = new List<Columns>();
    static List<Columns> renderList1 = new List<Columns>();

    FarChunkColThread farChunkColThread;

    int timer = 0;
    public static int loadRadius = 10;
    public static float loadDistance = loadRadius * Chunk.chunkSize;
    public static int farLoadRadius = 40;
    private static int loadDiameter = loadRadius*2+1;
    private static int farLoadDiameter = farLoadRadius*2+1;
    public static int farChunkupdates = 10;

    private WorldPos[] chunkPositions = new WorldPos[loadDiameter*loadDiameter];
    private WorldPos[] farChunkPositions = new WorldPos[farLoadDiameter*farLoadDiameter];

    // bool built = false;

    // Stopwatch stopWatch = new Stopwatch();


    // Start is called before the first frame update
    void Start()
    {   
        int i = 0;
        for (int radius = 0 ; radius <= loadRadius; radius++){
            
            for(float x = -radius; x<=radius; x++){
                // for(int y = -radius; y<=radius; y++){
                    for(float z = -radius; z<=radius; z++){
                        if(radius == Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(x,2)+Mathf.Pow(z,2)))){
                            chunkPositions[i] = new WorldPos(x*Chunk.chunkSize,0,z*Chunk.chunkSize);
                            i++;
                        }
                    }
                // }
            }
        }

        i = 0;
        for (int radius = 0 ; radius <= farLoadRadius; radius++){
            
            for(float x = -radius; x<=radius; x++){
                // for(int y = -radius; y<=radius; y++){
                    for(float z = -radius; z<=radius; z++){
                        if(radius == Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(x,2)+Mathf.Pow(z,2)))){
                            farChunkPositions[i] = new WorldPos(x*Chunk.chunkSize,0,z*Chunk.chunkSize);
                            i++;
                        }
                    }
                // }
            }
        }
        
        // stopWatch.Start();
    }

    // Update is called once per frame
    void Update()
    {

        
        //Old Generation
        // built = false;
        // if(DeleteChunks())
        //     return;
        // FindAndLoadChunks();
        // RenderChunks();
        // // FindChunksToLoad();
        // // LoadAndRenderChunks();

        
        if(DeleteColumns()){
            return;
        }
        
        // Stopwatch stopwatch2 = new Stopwatch();
        // stopwatch2.Start();
        // FindLoadCreateRenderChunks();
        // stopwatch2.Stop();
        // if(stopwatch2.ElapsedMilliseconds > 10){
        //     print("Total Load: "+ stopwatch2.ElapsedMilliseconds);
        // }

        // TempGenFarCol();

        FindLoadCreateRenderColumns();
        
    }



    void FindLoadCreateRenderColumns(){
        

        List<Columns> createListRemover = new List<Columns>();
        List<Columns> renderListRemover = new List<Columns>();
        List<Columns> farCreateListRemover = new List<Columns>();

        
        //Creates and Render the FarChunkCols
        if(farCreateList.Count <= 0){

        }
        else if(farChunkColThread == null || (!farChunkColThread.CreateCheck() && farChunkColThread.rendered)){
            List<FarChunkCol> farChunkCols = new List<FarChunkCol>();
            foreach(Columns column in farCreateList){
                column.farChunkCol = world.CreateFarChunkColumn(column);
                farChunkCols.Add(column.farChunkCol);
                farCreateListRemover.Add(column);
            }
            farChunkColThread = new FarChunkColThread(farChunkCols);
        }
        else if(!farChunkColThread.CreateCheck() && !farChunkColThread.rendered){
            farChunkColThread.Render();
        }

        foreach(Columns column in farCreateListRemover){
            farCreateList.Remove(column);
        }
        


        int i = 0;
        int j = 0;

        //Check for create ChunkColumn
        // int count = 0;
        foreach(Columns column in createList1){
            if(column.chunkColumn.creating){
                if (column.chunkColumn.CreateCheck1()){
                    i++;
                    continue;
                }
                else{
                    if(column.chunkColumn.creating2){
                        if(column.chunkColumn.CreateCheck2()){
                            i++;
                            continue;
                        }
                        else{
                            column.chunkColumn.CreateEnd();
                            createListRemover.Add(column);
                        } 
                    }
                    else{
                        if(j > 0){
                            continue;
                        }
                        column.chunkColumn.CreateStart2();
                        i++;
                        j++;
                    }
                }
            }
            else{
                if(j > 0){
                    continue;
                }
                column.chunkColumn.CreateStart();
                j++;
                i++;
            }
        }


        //remove ChunkColumns from create list if they are done creating
        foreach(Columns column in createListRemover){
            createList1.Remove(column);
        }
        //If there are any chunksColumns still creating wait for them to finish
        if(i > 0){
            // if(stopWatch.ElapsedMilliseconds > 10){
            //     print("Creation Return: "+ stopWatch.ElapsedMilliseconds);
            // } 
            // print("Chunk create check count: "+ count);
            // print("Creation Return: "+ stopWatch.ElapsedMilliseconds);
            
            return;
        }
        

        //Check for render ChunkColumn
        foreach(Columns column in renderList1){
            if(world.GetChunkUpdateCount() >= World.maxChunkUpdates){
                break;
            }
            else if(!column.chunkColumn.rendered){
                column.chunkColumn.Render();
            }
            else if(column.chunkColumn.CheckRendered()){
                column.RenderEndChunkColumn();
                renderListRemover.Add(column);
            }
        }

        //remove ChunkColumns from render list if they are done rendering
        foreach(Columns column in renderListRemover){
            renderList1.Remove(column);
        }

        //If there are any chunksColumns still rendering wait for them to finish
        if(renderList1.Count > 0 ){
            // if(stopWatch.ElapsedMilliseconds > 10){
            //     print("Render Return: "+ stopWatch.ElapsedMilliseconds);
            // }
            return;
        }


        

        float pposx = Mathf.FloorToInt(transform.position.x/Chunk.chunkSize)*Chunk.chunkSize;
        float pposy = Mathf.FloorToInt(transform.position.y/Chunk.chunkSize)*Chunk.chunkSize;
        float pposz = Mathf.FloorToInt(transform.position.z/Chunk.chunkSize)*Chunk.chunkSize;
        WorldPos playerPos = new WorldPos(pposx,pposy,pposz);
        i = 0;

        foreach(WorldPos chunkColumnPos in farChunkPositions){
            if(chunkColumnPos == null){
                break;
            }

            //Create Pos for checking chunks
            WorldPos newChunkColumnPos = new WorldPos(chunkColumnPos.x+playerPos.x,0,chunkColumnPos.z+playerPos.z);

            Columns column;
            //Check if Column exists
            column = world.TryGetColumn(newChunkColumnPos);
            if(column != null){
                //Check if close enough to do close Generation
                if(i < 1 && WorldPos.Distance(playerPos, newChunkColumnPos) < loadDistance){
                    if(column.chunkColumn == null){
                        CreateAdjChunkColumns(newChunkColumnPos);
                        column.CreateChunkColumn();
                        createList1.Add(column);
                        renderList1.Add(column);
                        i++;
                    }
                    else if(column.chunkColumn.created && !column.chunkColumn.rendered){
                        CreateAdjChunkColumns(newChunkColumnPos);
                        renderList1.Add(column);
                        i++;
                    }
                }
            }
            //Creates far chunks and Columns
            else if(farCreateList.Count < farChunkupdates){
                Columns col = new Columns(world, newChunkColumnPos);
                world.AddColumns(newChunkColumnPos, col);
                farCreateList.Add(col);
            }
            //If chunkColumn is not created and farChunkUpdates is exceeded stop generating
            else{
                break;
            }
        }
    }

    private void CreateAdjChunkColumns(WorldPos pos){
        WorldPos newChunkColumnPosX = new WorldPos(pos.x+Chunk.chunkSize,0,pos.z);
        WorldPos newChunkColumnPosZ = new WorldPos(pos.x,0,pos.z+Chunk.chunkSize);
        WorldPos newChunkColumnPosXZ = new WorldPos(pos.x+Chunk.chunkSize,0,pos.z+Chunk.chunkSize);

        List<WorldPos> adjChunkColumn = new List<WorldPos>();
        adjChunkColumn.Add(newChunkColumnPosX);
        adjChunkColumn.Add(newChunkColumnPosZ);
        adjChunkColumn.Add(newChunkColumnPosXZ);

        Columns columnAdj;
        foreach(WorldPos posAdj in adjChunkColumn){
            columnAdj = world.TryGetColumn(posAdj);
            if(columnAdj != null){
                if(columnAdj.chunkColumn == null){
                    columnAdj.chunkColumn = new ChunkColumn(columnAdj);
                    createList1.Add(columnAdj);
                }
            }
            else{
                Columns col = new Columns(world, posAdj);
                world.AddColumns(posAdj, col);
                farCreateList.Add(col);
                col.CreateChunkColumn();
                createList1.Add(col);
            }
        }
    }



    // void FindLoadCreateRenderChunks(){
    //     // Stopwatch stopWatch = new Stopwatch();
    //     // stopWatch.Start();

    //     float pposx = Mathf.FloorToInt(transform.position.x/Chunk.chunkSize)*Chunk.chunkSize;
    //     float pposy = Mathf.FloorToInt(transform.position.y/Chunk.chunkSize)*Chunk.chunkSize;
    //     float pposz = Mathf.FloorToInt(transform.position.z/Chunk.chunkSize)*Chunk.chunkSize;
    //     WorldPos playerPos = new WorldPos(pposx,pposy,pposz);

    //     int i = 0;
    //     bool check;
    //     bool checkRender;
    //     if(farChunkColThread == null){
    //         check = false;
    //         checkRender = true;
    //     }
    //     else{
    //         check = farChunkColThread.CreateCheck();
    //         checkRender = farChunkColThread.rendered;
    //     }
        
    //     FarChunkCol farChunkCol;
    //     if(!check && !checkRender){
    //         farChunkColThread.Render();
    //     }
    //     else if(!check){
    //         List<FarChunkCol> farChunkCols = new List<FarChunkCol>();
    //         foreach(WorldPos chunkColumnPos in farChunkPositions){
    //             if(chunkColumnPos == null){
    //                 break;
    //             }
    //             WorldPos newFarChunkColumnPos = new WorldPos(
    //                 chunkColumnPos.x+playerPos.x,
    //                 0,
    //                 chunkColumnPos.z+playerPos.z);
                
    //             if(!world.farChunkColumns.TryGetValue(newFarChunkColumnPos, out farChunkCol)){
                    
    //                 if(i >= 10){
    //                     break;
    //                 }
                    

    //                 ChunkColumn chunkColumn1;
    //                 if(!world.chunkColumns2.TryGetValue(newFarChunkColumnPos, out chunkColumn1) || !chunkColumn1.rendered){
    //                     farChunkCol = world.CreateFarChunkColumn(newFarChunkColumnPos);
    //                     farChunkCols.Add(farChunkCol);
    //                     i++;
    //                 }
    //             }
    //         }
    //         farChunkColThread = new FarChunkColThread(farChunkCols);
    //     }
        

    //     if(world.chunkUpdates.Count > 0){
    //         return;
    //     }

    //     List<ChunkColumn> createListRemover = new List<ChunkColumn>();
    //     List<ChunkColumn> renderListRemover = new List<ChunkColumn>();

    //     i = 0;
    //     int j = 0;

    //     //Check for create ChunkColumn
    //     // int count = 0;
    //     foreach(ChunkColumn chunkColumn in createList){
    //         if(chunkColumn.creating){
    //             if (chunkColumn.CreateCheck1()){
    //                 i++;
    //                 continue;
    //             }
    //             else{
    //                 if(chunkColumn.creating2){
    //                     if(chunkColumn.CreateCheck2()){
    //                         i++;
    //                         continue;
    //                     }
    //                     else{
    //                         chunkColumn.CreateEnd();
    //                         createListRemover.Add(chunkColumn);
    //                     } 
    //                 }
    //                 else{
    //                     if(j > 0){
    //                         continue;
    //                     }
    //                     chunkColumn.CreateStart2();
    //                     // count = count + chunkColumn.chunks.Count;
    //                     i++;
    //                     j++;
    //                 }
                    
    //             }
    //         }
    //         else{
    //             if(j > 0){
    //                 continue;
    //             }
    //             chunkColumn.CreateStart();
    //             j++;
    //             i++;
    //         }
    //     }

        

    //     //remove ChunkColumns from create list if they are done creating
    //     foreach(ChunkColumn chunkColumn in createListRemover){
    //         createList.Remove(chunkColumn);
    //     }
    //     //If there are any chunksColumns still creating wait for them to finish
    //     if(i > 0){
    //         if(stopWatch.ElapsedMilliseconds > 10){
    //             print("Creation Return: "+ stopWatch.ElapsedMilliseconds);
    //         } 
    //         // print("Chunk create check count: "+ count);
    //         // print("Creation Return: "+ stopWatch.ElapsedMilliseconds);
            
    //         return;
    //     }
    //     i = 0;

    //     //Check for render ChunkColumn
    //     foreach(ChunkColumn chunkColumn in renderList){
    //         if(world.chunkUpdates.Count < World.maxChunkUpdates){
    //             chunkColumn.Render();
    //             if(world.farChunkColumns.TryGetValue(chunkColumn.pos, out farChunkCol)){
    //                 destroyList.Add(new ChunkColumnFarAndClose(farChunkCol, chunkColumn));
    //                 // world.DestroyFarChunkColumn(chunkColumn.pos);
    //             }
    //             renderListRemover.Add(chunkColumn);
    //         }
    //         else{
    //             break;
    //         }
    //     }

    //     //remove ChunkColumns from render list if they are done rendering
    //     foreach(ChunkColumn chunkColumn in renderListRemover){
    //         renderList.Remove(chunkColumn);
    //     }

    //     //If there are any chunksColumns still rendering wait for them to finish
    //     if(renderList.Count > 0 ){
    //         if(stopWatch.ElapsedMilliseconds > 10){
    //             print("Render Return: "+ stopWatch.ElapsedMilliseconds);
    //         }
    //         return;
    //     }


    //     // float pposx = Mathf.FloorToInt(transform.position.x/Chunk.chunkSize)*Chunk.chunkSize;
    //     // float pposy = Mathf.FloorToInt(transform.position.y/Chunk.chunkSize)*Chunk.chunkSize;
    //     // float pposz = Mathf.FloorToInt(transform.position.z/Chunk.chunkSize)*Chunk.chunkSize;
    //     // WorldPos playerPos = new WorldPos(pposx,pposy,pposz);

    //     foreach(WorldPos chunkColumnPos in chunkPositions){
    //         if(chunkColumnPos == null) {
    //             break;
    //         }

    //         WorldPos newChunkColumnPos = new WorldPos(
    //             chunkColumnPos.x+playerPos.x,
    //             0,
    //             chunkColumnPos.z+playerPos.z);

    //         WorldPos newChunkColumnPosX = new WorldPos(
    //             chunkColumnPos.x+playerPos.x+Chunk.chunkSize,
    //             0,
    //             chunkColumnPos.z+playerPos.z);

    //         WorldPos newChunkColumnPosZ = new WorldPos(
    //             chunkColumnPos.x+playerPos.x,
    //             0,
    //             chunkColumnPos.z+playerPos.z+Chunk.chunkSize);

    //         WorldPos newChunkColumnPosXZ = new WorldPos(
    //             chunkColumnPos.x+playerPos.x+Chunk.chunkSize,
    //             0,
    //             chunkColumnPos.z+playerPos.z+Chunk.chunkSize);

    //         List<WorldPos> adjChunkColumn = new List<WorldPos>();
    //         adjChunkColumn.Add(newChunkColumnPosX);
    //         adjChunkColumn.Add(newChunkColumnPosZ);
    //         adjChunkColumn.Add(newChunkColumnPosXZ);

    //         ChunkColumn chunkColumn;
    //         ChunkColumn chunkColumnAdj;
    //         if(world.chunkColumns2.TryGetValue(newChunkColumnPos, out chunkColumn)){
                
    //             if(chunkColumn.created && !chunkColumn.rendered){
    //                 foreach(WorldPos pos in adjChunkColumn){
    //                     if(!world.chunkColumns2.TryGetValue(pos, out chunkColumnAdj)){
    //                         chunkColumnAdj = new ChunkColumn(world, pos);
    //                         createList.Add(chunkColumnAdj);
    //                         world.chunkColumns2.Add(pos,chunkColumnAdj);
    //                     }
    //                 }
    //                 renderList.Add(chunkColumn);
    //             }
    //             else{
    //                 continue;
    //             }
    //         }
    //         else{
    //             foreach(WorldPos pos in adjChunkColumn){
    //                 if(!world.chunkColumns2.TryGetValue(pos, out chunkColumnAdj)){
    //                     chunkColumnAdj = new ChunkColumn(world, pos);
    //                     createList.Add(chunkColumnAdj);
    //                     world.chunkColumns2.Add(pos,chunkColumnAdj);
    //                 }
    //             }

    //             chunkColumn = new ChunkColumn(world, newChunkColumnPos);
    //             createList.Add(chunkColumn);
    //             renderList.Add(chunkColumn);
    //             world.chunkColumns2.Add(newChunkColumnPos,chunkColumn);
    //             break;
    //         }
    //     }
    //     if(stopWatch.ElapsedMilliseconds > 10){
    //         print("End: "+ stopWatch.ElapsedMilliseconds);
    //     } 
    //     stopWatch.Stop();
    // }



    void FindChunksToLoad(){
        WorldPos playerPos = new WorldPos(
            Mathf.FloorToInt(transform.position.x/Chunk.chunkSize)*Chunk.chunkSize,
            Mathf.FloorToInt(transform.position.y/Chunk.chunkSize)*Chunk.chunkSize,
            Mathf.FloorToInt(transform.position.z/Chunk.chunkSize)*Chunk.chunkSize);

        int j = 0;
        if (updateList.Count == 0){
            for(int i = 0; i<chunkPositions.Length;i++){
                if(chunkPositions[i] == null){
                    return;
                }

                WorldPos newChunkPos = new WorldPos(
                    chunkPositions[i].x*Chunk.chunkSize+playerPos.x,
                    chunkPositions[i].y*Chunk.chunkSize+playerPos.y,
                    chunkPositions[i].z*Chunk.chunkSize+playerPos.z);
                
                Chunk newChunk = world.GetChunk(newChunkPos.x,newChunkPos.y,newChunkPos.z);

                if(newChunk != null && (newChunk.rendered || updateList.Contains(newChunkPos))){
                    continue;
                }

                for(float x = newChunkPos.x -Chunk.chunkSize; x <= newChunkPos.x +Chunk.chunkSize; x += Chunk.chunkSize){
                    for(float y = newChunkPos.y -Chunk.chunkSize; y <= newChunkPos.y +Chunk.chunkSize; y += Chunk.chunkSize){
                        for(float z = newChunkPos.z -Chunk.chunkSize; z <= newChunkPos.z +Chunk.chunkSize; z += Chunk.chunkSize){
                            buildList.Add(new WorldPos(x,y,z));
                        }
                    }
                }
                updateList.Add(new WorldPos(newChunkPos.x, newChunkPos.y, newChunkPos.z));
                j++;

                if(j == 1){
                    return;
                }
            }
        }
    }

    // void FindAndLoadChunks(){
    //     float pposx = Mathf.FloorToInt(transform.position.x/Chunk.chunkSize)*Chunk.chunkSize;
    //     float pposy = Mathf.FloorToInt(transform.position.y/Chunk.chunkSize)*Chunk.chunkSize;
    //     float pposz = Mathf.FloorToInt(transform.position.z/Chunk.chunkSize)*Chunk.chunkSize;
    //     WorldPos playerPos = new WorldPos(pposx,pposy,pposz);
        
    //     if (updateList.Count == 0){
    //         for(int i = 0; i <chunkPositions.Length; i++){
    //             if(chunkPositions[i] == null){
    //                 // stopWatch.Stop();
    //                 // print("time: "+ stopWatch.ElapsedMilliseconds);
    //                 return;
    //             }

    //             WorldPos newChunkColumnPos = new WorldPos(
    //                 chunkPositions[i].x+playerPos.x,
    //                 0,
    //                 chunkPositions[i].z+playerPos.z);
                
    //             List<WorldPos> chunkColumn;
    //             if(!world.chunkColumns.TryGetValue(newChunkColumnPos, out chunkColumn)){
                    
    //                 //Load column from file
                    
                    
    //                 chunkColumn = new List<WorldPos>();
    //                 TerrainGen gen = new TerrainGen();
    //                 float [] minMax = gen.MmTerrainHeight(newChunkColumnPos);

    //                 float min = Mathf.FloorToInt((minMax[0])/Chunk.chunkSize)*Chunk.chunkSize;
    //                 float max = Mathf.CeilToInt((minMax[0]+Chunk.chunkSize)/Chunk.chunkSize)*Chunk.chunkSize;
    //                 for(float y = min; y <= max; y +=Chunk.chunkSize){
    //                     BuildChunk(new WorldPos(newChunkColumnPos.x,y,newChunkColumnPos.z) , gen);
    //                     chunkColumn.Add(new WorldPos(newChunkColumnPos.x,y,newChunkColumnPos.z));
    //                 }

    //                 world.UpdateChunkColumn(newChunkColumnPos.x-Chunk.chunkSize,newChunkColumnPos.z);
    //                 world.UpdateChunkColumn(newChunkColumnPos.x,newChunkColumnPos.z-Chunk.chunkSize);
    //                 world.UpdateChunkColumn(newChunkColumnPos.x-Chunk.chunkSize,newChunkColumnPos.z-Chunk.chunkSize);
                    
    //                 world.chunkColumns.Add(newChunkColumnPos,chunkColumn);
    //                 built = true;
    //                 return;
    //             }

    //         }
    //     }
    // }

    // void FindAndLoadChunks2(){
    //     float pposx = Mathf.FloorToInt(transform.position.x/Chunk.chunkSize)*Chunk.chunkSize;
    //     float pposy = Mathf.FloorToInt(transform.position.y/Chunk.chunkSize)*Chunk.chunkSize;
    //     float pposz = Mathf.FloorToInt(transform.position.z/Chunk.chunkSize)*Chunk.chunkSize;
    //     WorldPos playerPos = new WorldPos(pposx,pposy,pposz);
        
    //     if (updateList.Count == 0){
    //         for(int i = 0; i <chunkPositions.Length; i++){
    //             if(chunkPositions[i] == null){
    //                 // stopWatch.Stop();
    //                 // print("time: "+ stopWatch.ElapsedMilliseconds);
    //                 return;
    //             }

    //             WorldPos newChunkColumnPos = new WorldPos(
    //                 chunkPositions[i].x+playerPos.x,
    //                 0,
    //                 chunkPositions[i].z+playerPos.z);
                
    //             List<WorldPos> chunkColumn;
    //             if(!world.chunkColumns.TryGetValue(newChunkColumnPos, out chunkColumn)){
                    
    //                 //Load column from file
                    
                    
    //                 chunkColumn = new List<WorldPos>();
    //                 TerrainGen gen = new TerrainGen();
    //                 float [] minMax = gen.MmTerrainHeight(newChunkColumnPos);

    //                 float min = Mathf.FloorToInt((minMax[0])/Chunk.chunkSize)*Chunk.chunkSize;
    //                 float max = Mathf.CeilToInt((minMax[0]+Chunk.chunkSize)/Chunk.chunkSize)*Chunk.chunkSize;
    //                 for(float y = min; y <= max; y +=Chunk.chunkSize){
    //                     BuildChunk(new WorldPos(newChunkColumnPos.x,y,newChunkColumnPos.z) , gen);
    //                     chunkColumn.Add(new WorldPos(newChunkColumnPos.x,y,newChunkColumnPos.z));
    //                 }

    //                 world.UpdateChunkColumn(newChunkColumnPos.x-Chunk.chunkSize,newChunkColumnPos.z);
    //                 world.UpdateChunkColumn(newChunkColumnPos.x,newChunkColumnPos.z-Chunk.chunkSize);
    //                 world.UpdateChunkColumn(newChunkColumnPos.x-Chunk.chunkSize,newChunkColumnPos.z-Chunk.chunkSize);
                    
    //                 world.chunkColumns.Add(newChunkColumnPos,chunkColumn);
    //                 built = true;
    //                 return;
    //             }

    //         }
    //     }
    // }

    void BuildChunk(WorldPos pos,TerrainGen gen){
        if(world.GetChunk(pos.x,pos.y,pos.z) == null){
            world.CreateChunk(pos.x,pos.y,pos.z,gen);
            updateList.Add(new WorldPos(pos.x,pos.y,pos.z));
        }
    }

    // void BuildChunk(WorldPos pos){
    //     if(world.GetChunk(pos.x,pos.y,pos.z) == null && pos.y >= World.bottomWorldHeight){
    //         world.CreateChunk(pos.x,pos.y,pos.z);
    //     }
    // }

    // void LoadAndRenderChunks(){
    //     if (buildList.Count != 0){
    //         int count = buildList.Count;
    //         for (int i = 0; i < count; i++){
    //             BuildChunk(buildList[0]);
    //             buildList.RemoveAt(0);
    //         }
    //         return;
    //     }
    //     if(updateList.Count != 0){
    //         int count = updateList.Count;
    //         for (int i = 0; i< count; i++){
    //             Chunk chunk = world.GetChunk(updateList[0].x,updateList[0].y,updateList[0].z);
    //             if(chunk != null){
    //                 chunk.update = true;
    //             }
    //             updateList.RemoveAt(0);
    //         }
    //     }
    // }

    // void RenderChunks(){
    //     if(updateList.Count != 0 && !built){
    //         int count = updateList.Count;
    //         for (int i = 0; i< 1; i++){
    //             Chunk chunk = world.GetChunk(updateList[0].x,updateList[0].y,updateList[0].z);
    //             if(chunk != null){
    //                 chunk.update = true;
    //                 world.chunkUpdates.Add(chunk);
    //             }
    //             updateList.RemoveAt(0);
    //         }
    //     }
    // }


    bool DeleteChunks1(){
        if(timer == 10){
            var chunksToDelete = new List<WorldPos>();
            foreach(var chunk in world.chunks){
                float distance = Vector3.Distance(new Vector3(chunk.Value.pos.x,chunk.Value.pos.y,chunk.Value.pos.z), new Vector3(transform.position.x,transform.position.y,transform.position.z));

                if (distance > (loadRadius+2)*Chunk.chunkSize){
                    chunksToDelete.Add(chunk.Key);
                }
            }

            foreach (var chunk in chunksToDelete){
                world.DestroyChunk(chunk.x,chunk.y,chunk.z);
            }
            timer = 0;
            return true;
        }
        timer ++;
        return false;
    }

    // bool DeleteChunks(){
    //     if(timer == 10){
    //         var chunksColumnToDelete = new List<WorldPos>();
    //         foreach (KeyValuePair<WorldPos, List<WorldPos>> entry in world.chunkColumns){
    //             float distance = Vector3.Distance(new Vector3(entry.Key.x,0,entry.Key.z), new Vector3(transform.position.x,0,transform.position.z));

    //             if (distance > (loadRadius+2)*Chunk.chunkSize){
    //                 chunksColumnToDelete.Add(new WorldPos(entry.Key.x,0,entry.Key.z));
    //             }
    //         }

    //         foreach (var chunk in chunksColumnToDelete){
    //             world.DestroyChunkColumn(chunk.x,chunk.y,chunk.z);
    //         }
    //         timer = 0;
    //         return true;
    //     }
    //     timer ++;
    //     return false;
    // }

    // bool DeleteChunks2(){
    //     if(timer == 10){
    //         List<WorldPos> chunksColumnToDelete = new List<WorldPos>();
    //         foreach (var entry in world.chunkColumns2){
    //             float distance = Vector3.Distance(new Vector3(entry.Key.x,0,entry.Key.z), new Vector3(transform.position.x,0,transform.position.z));

    //             if (distance > (loadRadius+2)*Chunk.chunkSize){
    //                 chunksColumnToDelete.Add(new WorldPos(entry.Key.x,0,entry.Key.z));
    //             }
    //         }

    //         foreach (WorldPos pos in chunksColumnToDelete){
    //             world.DestroyChunkColumn2(pos);
    //         }
    //         timer = 0;
    //         return true;
    //     }
    //     timer ++;
    //     return false;
    // }


    // bool DeleteChunksFar(){
    //     if(timer == 10){
    //         List<WorldPos> farChunksColumnToDelete = new List<WorldPos>();
    //         foreach (var entry in world.farChunkColumns){
    //             float distance = Vector3.Distance(new Vector3(entry.Key.x,0,entry.Key.z), new Vector3(transform.position.x,0,transform.position.z));

    //             if (distance > (farLoadRadius+2)*Chunk.chunkSize){
    //                 farChunksColumnToDelete.Add(new WorldPos(entry.Key.x,0,entry.Key.z));
    //             }
    //         }

    //         List<WorldPos> chunksColumnToDelete = new List<WorldPos>();
    //         foreach (var entry in world.chunkColumns2){
    //             float distance = Vector3.Distance(new Vector3(entry.Key.x,0,entry.Key.z), new Vector3(transform.position.x,0,transform.position.z));

    //             if (distance > (loadRadius+2)*Chunk.chunkSize){
    //                 chunksColumnToDelete.Add(new WorldPos(entry.Key.x,0,entry.Key.z));
    //             }
    //         }

    //         foreach (WorldPos pos in chunksColumnToDelete){
    //             world.DestroyChunkColumn2(pos);
    //         }

    //         foreach (WorldPos pos in farChunksColumnToDelete){
    //             world.DestroyFarChunkColumn(pos);
    //         }

    //         List<ChunkColumnFarAndClose> destroyListRemove = new List<ChunkColumnFarAndClose>();
    //         foreach (ChunkColumnFarAndClose cols in destroyList){
    //             if(cols.col.CheckRendered()){
    //                 world.DestroyFarChunkColumn(cols.farChunkCol.pos);
    //                 destroyListRemove.Add(cols);
    //             }
    //         }

    //         foreach(ChunkColumnFarAndClose cols in destroyListRemove){
    //             destroyList.Remove(cols);
    //         }

    //         timer = 0;
    //         return true;
    //     }
    //     timer ++;
    //     return false;
    // }


    public bool DeleteColumns(){
        if(timer < 10){
            timer ++;
            return false;
        }
        else{
            Vector3 ppos = new Vector3(transform.position.x,0,transform.position.z);
            float chunkDistance = (loadRadius+2)*Chunk.chunkSize;
            float columnDistance = (farLoadRadius+2)*Chunk.chunkSize;

            List<Columns> toDestroy;

            toDestroy = world.CheckDestroyColumn(ppos,chunkDistance,columnDistance);

            foreach(Columns column in toDestroy){
                Columns.Destroy(column);
            }
            
            timer = 0;
            return true;
        }
    }

    public static void CreateChunkColumn(Columns column){
        column.CreateChunkColumn();
        createList1.Add(column);
    }

}
