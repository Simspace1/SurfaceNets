using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class LoadChunks : MonoBehaviour
{

    [SerializeField]
    private World world;

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

    Stopwatch stopWatch = new Stopwatch();


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
        
        stopWatch.Start();
    }

    // Update is called once per frame
    void Update()
    {

        
        if(DeleteColumns()){
            return;
        }

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
                column.farChunkCol.SetCreating();
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
            if(column.chunkColumn == null){
                createListRemover.Add(column);
                continue;
            }
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
            if(column.chunkColumn == null){
                renderListRemover.Add(column);
                continue;
            }
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

            // //Check end of farChunk Loading
            // if(farChunkPositions[5168].Equals(chunkColumnPos)){
            //     // stopWatch.Stop();
            //     print("FarChunk Loading Time:  " + stopWatch.ElapsedMilliseconds);
            // }

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

                        // //Check how long to load "all" chunks
                        // if(WorldPos.Distance(playerPos, newChunkColumnPos) < loadDistance && WorldPos.Distance(playerPos, newChunkColumnPos) >= loadDistance-Chunk.chunkSize){
                        //     // stopWatch.Stop();
                        //     print("Chunk Loading:  " + stopWatch.ElapsedMilliseconds);
                        // }
                        
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
                Columns col = new Columns(world, newChunkColumnPos, world.gen.GenerateColumnGen(newChunkColumnPos));
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
                Columns col = new Columns(world, posAdj, world.gen.GenerateColumnGen(posAdj));
                world.AddColumns(posAdj, col);
                farCreateList.Add(col);
                col.CreateChunkColumn();
                createList1.Add(col);
            }
        }
    }



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
                if(column.farChunkCol != null && column.farChunkCol.creating){
                    column.farChunkCol.GetType();
                    continue;
                }
                column.DestroyChunkColumn();
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

    public static void RemoveColumnFLists(Columns column){
        if(column.farChunkCol != null){
            farCreateList.Remove(column);
        }
        if(column.chunkColumn != null){
            createList1.Remove(column);
            renderList1.Remove(column);
        }
    }

}
