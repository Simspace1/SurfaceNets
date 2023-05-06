using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadRegions : MonoBehaviour
{
    private static LoadRegions loadRegions;
    private int timer = 0;
    private int resTimer = 0;

    public static int maxRegionRadius = 10;
    public static int fullResRegionRadius = 2;
    public static int regionColUpdates = 1;
    public static int loadDistance = maxRegionRadius+1;

    public static List<RegionPos> newRegionWaitList = new List<RegionPos>();

    private List<RegionPos> regionList = new List<RegionPos>();

    private RegionCol regionCol;
    private List<Region> createList = new List<Region>();
    private List<Region> generateList = new List<Region>();
    private List<Region> updateList = new List<Region>();
    private List<Region> renderList = new List<Region>();

    private RegionPos playerPos;
    private bool chunksGenerating = false;
    private bool chunksUpdating = false;

    private bool nextRegionCol = true;
    private bool newRegionCol = false;

    private bool unloading = false;
    private bool changingResolution = false;

    // Start is called before the first frame update
    void Start()
    {
        loadRegions = this;

        // for(int radius = 0; radius <= maxRegionRadius; radius++){
        //     for(int x = -radius; x <= radius; x++){
        //         for(int z = -radius; z <=radius; z++){
        //             if((Mathf.Abs(x) == radius || Mathf.Abs(z) == radius) && MyMath.Hypothenuse(x,z) <= maxRegionRadius){
        //                 regionList.Add(new RegionPos(x,0,z));
        //             }
        //         }
        //     }
        // }

        for(int i = 0; i <= maxRegionRadius; i++){
            if( i == 0){
                regionList.Add(new RegionPos(i,0,i));
            }
            else{
                regionList.Add(new RegionPos(i,0,0));
                regionList.Add(new RegionPos(0,0,i));
                regionList.Add(new RegionPos(-i,0,0));
                regionList.Add(new RegionPos(0,0,-i));
            }

            for(int j = 1; j < i; j++){
                regionList.Add(new RegionPos(i,0,j));
                regionList.Add(new RegionPos(-j,0,i));
                regionList.Add(new RegionPos(-i,0,-j));
                regionList.Add(new RegionPos(j,0,-i));
                regionList.Add(new RegionPos(i,0,-j));
                regionList.Add(new RegionPos(j,0,i));
                regionList.Add(new RegionPos(-i,0,j));
                regionList.Add(new RegionPos(-j,0,-i));
            }

            if(i != 0){
                regionList.Add(new RegionPos(i,0,-i));
                regionList.Add(new RegionPos(i,0,i));
                regionList.Add(new RegionPos(-i,0,i));
                regionList.Add(new RegionPos(-i,0,-i));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerPos();
        // if(LoadWaitList()){
        //     return;
        // }

        // if(!unloading && Unload()){
        //     return;
        // }

        // if(!changingResolution && ChangeResolution()){
        //     return;
        // }

        Load2();
    }

    //Load/Create regions with priority, mostly from terrain modifications
    private bool LoadWaitList()
    {
        if(newRegionWaitList.Count == 0){
            return false;
        }
        return false;
    }

    private void UpdatePlayerPos()
    {
        WorldPos ppos = new WorldPos(transform.position.x, transform.position.y, transform.position.z);
        playerPos = ppos.GetRegion();
    }

    // private void Load(){
    //     if(RenderRegions()){return;}
    //     if(UpdateRegions()){return;}
    //     if(GenerateRegions()){return;}
    //     if(CreateRegions()){return;}
    //     if(CreateRegionColumn()){return;}
    //     // UpdateFullResRegions();
    // }

    private void Load2(){
        CreateRegionColumn2();
    }

    private void UpdateFullResRegions(){
        throw new NotImplementedException();
    }

    private bool CreateRegionColumn()
    {
        if(regionCol != null){
            if(regionCol.generating){
                return true;
            }
            List<RegionSurfacePos> regions = new List<RegionSurfacePos>();
            bool loading = true;
            if(regions.Count == 0){
                regions = regionCol.GetRegionList();
                loading = false;
                World.GetWorld().AddRegionCol(regionCol);
            }

            foreach(RegionSurfacePos surfacePos in regions){
                createList.Add(regionCol.GetRegion(surfacePos.regionPos));
            }

            if(loading){
                regionCol.ClearLoadingList();
            }
            regionCol = null;
            return true;
        }
        else{
            RegionCol col = null;
            bool flag = false;
            foreach(RegionPos regionPos in regionList){
                RegionPos pos = playerPos.GetColumn().Add(regionPos);
                col = World.GetWorld().GetRegionCol(pos);

                if(col != null && !col.IsComplete()){
                    // flag = col.LoadRegions(playerPos);
                    if(flag){
                        regionCol = col;
                        return true;
                    }
                }
                else if(col == null){
                    regionCol = new RegionCol(pos,true);
                    return true;
                }
            }
            return false;
        }
    }

    private void CreateRegionColumn2()
    {
        if(nextRegionCol && newRegionCol){
            World.GetWorld().AddRegionCol(regionCol);
            newRegionCol = false;
            nextRegionCol = !FindCreateRegionCol();
        }
        else if(nextRegionCol){
            nextRegionCol = !FindCreateRegionCol();
        }        
    }

    private bool FindCreateRegionCol(){
        RegionCol col = null;
        foreach(RegionPos regionPos in regionList){
            RegionPos pos = playerPos.GetColumn().Add(regionPos);
            col = World.GetWorld().GetRegionCol(pos);

            if(col == null){
                regionCol = new RegionCol(pos,false);
                MyThreadPool.QueueJob(new ThreadJobRegionColGen(regionCol));
                newRegionCol = true;
                return true;
            }
            // else if(!col.IsComplete()){
            //     if(col.LoadRegions(playerPos)){
            //         return true;
            //     }
            // }
        }
        return false;
    }

    // private bool CreateRegions()
    // {
    //     if(createList.Count == 0){
    //         return false;
    //     }

    //     foreach(Region region in createList){
    //         Debug.Assert(region.generated, "Region "+region.regionPos.ToString()+ "was not generated by RegionCol");
    //         region.CreateAllChunks();
    //         generateList.Add(region);
    //     }
    //     createList.Clear();
    //     return true;
    // }

    // private bool GenerateRegions()
    // {
    //     if(generateList.Count == 0){
    //         return false;
    //     }
    //     if(chunksGenerating){
    //         int done = 0;
    //         foreach(Region region in generateList){
    //             if(region.chunksGenerated){
    //                 done++;
    //             }
    //         }

    //         if(done == generateList.Count){
    //             foreach(Region region in generateList){
    //                 updateList.Add(region);
    //             }
    //             generateList.Clear();
    //             chunksGenerating = false;
    //         }
    //         return true;
    //     }
    //     else{
    //         foreach(Region region in generateList){
    //             Debug.Assert(region.chunksCreated, "Chunks in region "+ region.regionPos.ToString() + " were not created");
    //             region.GenerateAllChunks();
    //         }
    //         chunksGenerating = true;
    //         return true;
    //     }
    // }

    // private bool UpdateRegions()
    // {
    //     if(updateList.Count == 0){
    //         return false;
    //     }

    //     if(chunksUpdating){
    //         int done = 0;
    //         foreach(Region region in updateList){
    //             if(region.chunksUpdatedHalf || region.chunksUpdatedFull){
    //                 done++;
    //             }
    //         }

    //         if(done == updateList.Count){
    //             foreach(Region region in updateList){
    //                 renderList.Add(region);
    //             }
    //             updateList.Clear();
    //             chunksUpdating = false;
    //         }
    //         return true;
    //     }
    //     else{
    //         foreach(Region region in updateList){
    //             Debug.Assert(region.chunksGenerated, "Chunks in region "+ region.regionPos.ToString() + " were not generated");
    //             region.UpdateAllChunks();
    //         }
    //         chunksUpdating = true;
    //         return true;
    //     }

    // }

    // private bool RenderRegions()
    // {
    //     if(renderList.Count == 0){
    //         return false;
    //     }

    //     foreach(Region region in renderList){
    //         Debug.Assert(region.chunksUpdatedHalf || region.chunksUpdatedFull, "Chunks in region "+ region.regionPos.ToString() + " were not updated");
    //         region.RenderAllChunks();
    //     }
    //     renderList.Clear();
    //     return true;
    // }

    private bool Unload(){
        if(timer < 10){
            timer++;
            return false;
        }

        timer = 0;
        List<RegionCol> toDestroy = World.GetWorld().CheckDestroyRegionColumns(playerPos, loadDistance);
        MyThreadPool.QueueJob(new ThreadJobUnloader(toDestroy));
        return true;
    }

    private void ListRemover<T>(List<T> mainList, List<T> removeList){
        foreach(T obj in removeList){
            mainList.Remove(obj);
        }
    }

    public static void NotifyRegionCol(){
        loadRegions.SetNextRegionCol(true);
    }

    public void SetNextRegionCol(bool val){
        nextRegionCol = val;
    }

    public static void NotifiyUnloadingDone(){
        loadRegions.unloading = false;
    }

    public static void NotifiyChangeResolutionDone(){
        loadRegions.changingResolution = false;
    }

    private bool ChangeResolution(){
        if(resTimer < 20){
            resTimer++;
            return false;
        }

        resTimer = 0;
        List<Region>[] resRegions = World.GetWorld().CheckChangeRegionResolution(playerPos,fullResRegionRadius);
        if(resRegions[0].Count == 0 && resRegions[1].Count == 0){
            return true;
        }
        changingResolution = true;
        MyThreadPool.QueueJob(new ThreadJobChangeRegionResolution(resRegions[0],resRegions[1]));
        return true;
    }
}
