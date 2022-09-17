using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class FarChunkCol : MonoBehaviour
{   
    public World world;
    public WorldPos pos;
    public MyMesh meshData;
    public bool render = false;
    public Columns column;

    public TerrainGen gen;

    MeshFilter filter;
    MeshCollider coll;

    public void Start(){
        if(filter == null){
            filter = gameObject.GetComponent<MeshFilter>();
            coll = gameObject.GetComponent<MeshCollider>();
        }
        

        // Create();
    }

    // public void Update(){
    //     if(!render){
    //         Render();
    //     }
    // }

    public void Create(){
        meshData = new MyMesh();
        gen = new TerrainGen();
        gen.MmTerrainHeight(pos);

        Voxel[,] voxels = gen.FarVoxelGen(this);
        float[,] heights = gen.FarHeightConversion();
        float[,] terrain = gen.Terrain();

        float x,z;
        for(int xi = 0; xi < Chunk.chunkVoxels+1; xi++){
            for(int zi = 0; zi < Chunk.chunkVoxels+1; zi++){
                x = xi*Chunk.voxelSize;
                z = zi*Chunk.voxelSize;
                meshData.AddVertex(new Vector3(x+Chunk.voxelSize, heights[xi+1,zi], z));
                meshData.AddVertex(new Vector3(x+Chunk.voxelSize, heights[xi+1,zi+1], z+Chunk.voxelSize));
                meshData.AddVertex(new Vector3(x, heights[xi,zi+1], z+Chunk.voxelSize));
                meshData.AddVertex(new Vector3(x, heights[xi,zi], z));
                meshData.AddQuadTriangles();
                meshData.uv.AddRange(voxels[xi,zi].FaceUVs());
            }
        }        
    }

    public void CreateFilter(){
        if(filter == null){
            filter = gameObject.GetComponent<MeshFilter>();
            coll = gameObject.GetComponent<MeshCollider>();
        }
    }

    public void Render(){
        if(filter == null || coll == null){
            return;
        }
        
        render = true;

        filter.mesh.Clear();
        filter.mesh.vertices = meshData.vertices.ToArray();
        filter.mesh.triangles = meshData.triangles.ToArray();
        filter.mesh.SetUVs(0, meshData.uv);
        // filter.mesh.SetNormals(meshData.normals,0,meshData.vertices.Count);
        filter.mesh.RecalculateNormals();


        coll.sharedMesh = null;
        Mesh mesh = new Mesh();
        mesh.vertices = meshData.colVertices.ToArray();
        mesh.triangles = meshData.colTriangles.ToArray();
        mesh.RecalculateNormals();

        coll.sharedMesh = mesh;
    }

    public void UnRender(){
        render = false;
        filter.mesh.Clear();
        coll.sharedMesh.Clear();
    }
}
