using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class Region : MonoBehaviour
{
    public RegionPos regionPos {get; private set;}

    public RegionCol regionCol {get; private set;}

    private List<WorldPos> savedColumnsList = new List<WorldPos>();
    private Dictionary<WorldPos, Columns2> columns = new Dictionary<WorldPos, Columns2>(World.worldPosEqC);

    public GameObject regionObject {get; private set;}

    private MeshFilter filter;
    private MeshCollider coll;
    private MyMesh meshData;   

    public bool generated {get; private set;} = false;
    public bool destroying {get; private set;} = false;
    public bool destroyed {get; private set;} = false;
    public bool loaded {get; private set;} = false;
    public bool modified {get; private set;} = false;
    public bool surface {get; private set;} = false;
    public bool fullRes {get; private set;} = false;

    public bool chunksCreated {get; private set;} = false;
    public bool chunksGenerated {get; private set;} = false;
    public bool chunksUpdatedFull {get; private set;} = false;
    public bool chunksUpdatedHalf {get; private set;} = false;
    public bool chunksRendered {get; private set;} = false;

    public Region(RegionPos pos, RegionCol regionCol){
        Debug.Assert(regionCol.regionPos.InColumn(pos), "Created a region "+ pos.ToString() +" in the Wrong RegionColumn " + regionCol.regionPos.ToColString());

        this.regionPos = pos;
        this.regionCol = regionCol;

        GenColumns();
    }

    void Start(){
        if(filter == null) {
            filter = gameObject.GetComponent<MeshFilter>();
            coll = gameObject.GetComponent<MeshCollider>();
        }
    }

    private void GenColumns(){
        WorldPos pos = regionPos.ToWorldPos();
        int x = pos.xi;
        int y = pos.yi;
        int z = pos.zi;

        WorldPos temp = null;
        for(int i = x; i < x+RegionCol.regionVoxels; i+= Chunk2.chunkVoxels){
            for(int j = z; j < z+RegionCol.regionVoxels; j+= Chunk2.chunkVoxels){
                temp = new WorldPos(i,y,j);
                if(WasSavedColumn(temp)){
                    throw new NotImplementedException("Chunk Columns loading not implemented yet");
                }
                else{
                    Columns2 col = new Columns2(temp, this);
                    AddColumns(col);
                }
            }
        }

        Debug.Assert(columns.Count == RegionCol.regionChunks* RegionCol.regionChunks,"ChunkRegion "+ regionPos.ToString()+ "has generated " + columns.Count + " Columns instead of " +RegionCol.regionChunks* RegionCol.regionChunks);
        generated = true;
    
    }

    private bool WasSavedColumn(WorldPos pos){
        if(savedColumnsList.Count == 0){
            return false;
        }

        foreach(WorldPos sPos in savedColumnsList){
            if(sPos.Equals(pos)){
                return true;
            }
        }
        return false;
    }

    public Columns2 GetColumn(WorldPos pos){
        if(!regionPos.Equals(pos.GetRegion())){
            return null;
        }
        Columns2 col = null;
        columns.TryGetValue(pos, out col);
        return col;
    }

    private void AddColumns(Columns2 col){
        if(destroying || destroyed || !col.columnPos.GetRegion().Equals(regionPos))
            return;
        columns.Add(col.columnPos,col);
    }

    //test code for chunk generation
    public void CreateAllChunks(){
        regionObject = World.GetWorld().CreateRegion(regionPos);

        foreach(var colEntry in columns){
            colEntry.Value.CreateChunks();
        }
        chunksCreated = true;
    }

    //test code for chunk generation
    public void GenerateAllChunks(){
        Debug.Assert(chunksCreated,"Chunks of region:" + regionPos.ToString()+ " have not been created" );
        ThreadPool.QueueUserWorkItem(GenerateAllChunks,this);
        // GenerateAllChunks(this);
    }

    private void GenerateAllChunks(object state){
        foreach(var colEntry in columns){
            colEntry.Value.GenerateChunks();
        }
        chunksGenerated = true;
    }

    //test code for chunk updates
    public void UpdateAllChunks(){
        Debug.Assert(chunksGenerated,"Chunks of region:" + regionPos.ToString()+ " have not been generated");
        if(fullRes){
            ThreadPool.QueueUserWorkItem(UpdateAllChunksFull,this);
            // UpdateAllChunksFull(this);
        }
        else{
            ThreadPool.QueueUserWorkItem(UpdateAllChunksHalf,this);
            // UpdateAllChunksHalf(this);
        }
        
    }

    private void UpdateAllChunksFull(object state){
        foreach(var colEntry in columns){
            colEntry.Value.UpdateChunksFull();
        }
        chunksUpdatedFull = true;
    }

    private void UpdateAllChunksHalf(object state){
        foreach(var colEntry in columns){
            colEntry.Value.UpdateChunksHalf();
        }
        chunksUpdatedHalf = true;
    }

    public void RenderAllChunks(){
        foreach(var colEntry in columns){
            colEntry.Value.RenderChunks();
        }
        chunksRendered = true;
    }

    public void SetSurface(bool surface){
        this.surface = surface;
    }

    public void QueueAllChunkUpdates(){
        foreach(var colEntry in columns){  
            MyThreadPool.QueueJob(new ThreadJobChunkColumn(colEntry.Value));
        }
    }

    public void Destroy(){
        destroyed = true;
        foreach(var colEntry in columns){
            colEntry.Value.Destroy();
        }
        UnityEngine.Object.Destroy(regionObject);
    }

    public void SetChunkUpdatedFull(bool val){
        chunksUpdatedFull = val;
    }

    public void SetChunkUpdatedhalf(bool val){
        chunksUpdatedHalf = val;
    }

    public void QueueRes(bool fullRes){
        this.fullRes = fullRes;
        foreach(var colEntry in columns){
            MyThreadPool.QueueJob(new ThreadJobChangeChunkColumnResolution(colEntry.Value, fullRes));
        }
        
        if(fullRes){
            chunksUpdatedFull = true;
            chunksUpdatedHalf = false;
        }
        else{
            chunksUpdatedFull = false;
            chunksUpdatedHalf = true;
        }
    }

    public void SetRegionCol(RegionCol regionCol){
        this.regionCol = regionCol;
    }

    public void SetRegionPos(RegionPos regionPos){
        this.regionPos = regionPos;
    }

    public void GenerateCheapMesh(){
        GenerateBadMesh();
        // UpdateRegion(GenerateSurfacePoints(GenerateVoxelArray()));
    }
    
    //Generates Mesh with only the generated surface
    //IS NOT MEANT TO BE USED PERMANENTLY
    //PLACEHOLDER
    public void GenerateBadMesh(){
        meshData = new MyMesh();
        meshData.useRenderDataForCol = true;

        Voxel voxel = new Voxel();
        
        WorldPos regionPos = this.regionPos.ToWorldPos();

        float x,z;
        foreach(var genEntry in regionCol.GetAllGens()){
            WorldPos colPos = genEntry.Key.Substract(regionPos);
            for(int xi = -1; xi < Chunk2.chunkVoxels+1; xi+=2){
                for(int zi = -1; zi < Chunk2.chunkVoxels+1; zi+=2){
                    x = colPos.x + xi*Chunk2.voxelSize;
                    z = colPos.z + zi*Chunk2.voxelSize;
                    if(CheckHeights(regionPos,genEntry.Value,xi,zi,2)){
                        meshData.AddVertex(new Vector3(x+2*Chunk.voxelSize, genEntry.Value.GetHeight(xi+2,zi)-regionPos.y, z));
                        meshData.AddVertex(new Vector3(x+2*Chunk.voxelSize, genEntry.Value.GetHeight(xi+2,zi+2)-regionPos.y, z+2*Chunk.voxelSize));
                        meshData.AddVertex(new Vector3(x, genEntry.Value.GetHeight(xi,zi+2)-regionPos.y, z+2*Chunk.voxelSize));
                        meshData.AddVertex(new Vector3(x, genEntry.Value.GetHeight(xi,zi)-regionPos.y, z));
                        meshData.AddQuadTriangles();
                        meshData.uv.AddRange(voxel.FaceUVs());
                    }
                }
            }
        }
    }

    private bool CheckHeights(WorldPos pos,ColumnGen gen, int xi, int zi, int inc){
        float ymax = pos.y+RegionCol.regionSize;
        float ymin = pos.y;
        if((gen.GetHeight(xi,zi) >= ymin && gen.GetHeight(xi,zi) <= ymax) || (gen.GetHeight(xi+inc,zi) >= ymin && gen.GetHeight(xi+inc,zi) <= ymax) || (gen.GetHeight(xi,zi+inc) >= ymin && gen.GetHeight(xi,zi+inc) <= ymax) || (gen.GetHeight(xi+inc,zi+inc) >= ymin && gen.GetHeight(xi+inc,zi+inc) <= ymax)){
            return true;
        }
        else{
            return false;
        }
    }

    private Voxel[,,] GenerateVoxelArray(){
        throw new NotImplementedException("Generate Voxel array not implemented");   
    }

    private List<SurfPt> GenerateSurfacePoints(Voxel[,,] voxels){
        throw new NotImplementedException("Generate Surface points not implemented");   
    }

    private void UpdateRegion(List<SurfPt> surfPts){
        throw new NotImplementedException("UpdateRegion not implemented");   
    }

    public void RenderCheapMesh(){
        if(destroyed){
            return;
        }

        filter.mesh.Clear();
        filter.mesh.vertices = meshData.vertices.ToArray();
        filter.mesh.triangles = meshData.triangles.ToArray();
        filter.mesh.SetUVs(0, meshData.uv);
        filter.mesh.RecalculateNormals();

        coll.sharedMesh = null;
        Mesh mesh = new Mesh();
        mesh.vertices = meshData.colVertices.ToArray();
        mesh.triangles = meshData.colTriangles.ToArray();
        mesh.RecalculateNormals();

        coll.sharedMesh = mesh;
    }

    public void ActivateMesh(){
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<MeshCollider>().enabled = true;
    }

    public void DeactivateMesh(){
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<MeshCollider>().enabled = false;
    }

    private void GetGen(WorldPos pos){
        
    }

    
}
