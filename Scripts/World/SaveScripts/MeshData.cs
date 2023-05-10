using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MeshData
{
    public List<float[]> vertices = new List<float[]>(); // float[3]
    public List<int> triangles = new List<int>();
    public List<float[]> uv = new List<float[]>(); // float[2]
    public List<float[]> colVertices = new List<float[]>(); // float[3]
    public List<int> colTriangles = new List<int>();
    public List<float[]> normals = new List<float[]>(); // float[3]
    

    public MeshData(MyMesh mesh){
        foreach(Vector3 ver in mesh.vertices){
            vertices.Add(new float[]{ver.x,ver.y,ver.z});
        }

        foreach(Vector2 uv in mesh.uv){
            this.uv.Add(new float[]{uv.x,uv.y});
        }

        // foreach(Vector3 colVer in mesh.colVertices){
        //     colVertices.Add(new float[]{colVer.x,colVer.y,colVer.z});
        // }

        // foreach(Vector3 norm in mesh.normals){
        //     normals.Add(new float[]{norm.x,norm.y,norm.z});
        // }

        triangles = mesh.triangles;
        // colTriangles = mesh.colTriangles;
    }

    public MyMesh Revert(){
        MyMesh mesh = new MyMesh();

        foreach(float[] ver in vertices){
            mesh.vertices.Add(new Vector3(ver[0],ver[1],ver[2]));
        }

        foreach(float[] ver in uv){
            mesh.uv.Add(new Vector2(ver[0],ver[1]));
        }

        // foreach(float[] ver in colVertices){
        //     mesh.colVertices.Add(new Vector3(ver[0],ver[1],ver[2]));
        // }

        // foreach(float[] ver in normals){
        //     mesh.normals.Add(new Vector3(ver[0],ver[1],ver[2]));
        // }

        mesh.triangles = triangles;
        // mesh.colTriangles = colTriangles;

        return mesh;
    }
}
