using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]

public class MyMesh {
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uv = new List<Vector2>();
    public List<Vector2> uv2 = new List<Vector2>();
    public List<Vector2> uv3 = new List<Vector2>();
    public List<Vector2> uv4 = new List<Vector2>();
    // public List<Vector3> colVertices = new List<Vector3>();
    // public List<int> colTriangles = new List<int>();
    // public List<Vector3> normals = new List<Vector3>();
    // public bool useRenderDataForCol;
    
    public MyMesh(){

    }

    public void AddQuadTriangles()
     {
         triangles.Add(vertices.Count - 1);
         triangles.Add(vertices.Count - 2);
         triangles.Add(vertices.Count - 4);
         triangles.Add(vertices.Count - 2);
         triangles.Add(vertices.Count - 3);
         triangles.Add(vertices.Count - 4);
        //  if (useRenderDataForCol)
        //  {
        //      colTriangles.Add(colVertices.Count - 1);
        //      colTriangles.Add(colVertices.Count - 2);
        //      colTriangles.Add(colVertices.Count - 4);
        //      colTriangles.Add(colVertices.Count - 2);
        //      colTriangles.Add(colVertices.Count - 3);
        //      colTriangles.Add(colVertices.Count - 4);
        //  }
     }

    //  public void AddNormals(Vector3 normal){
    //      normals.Add(normal);
    //  }

     public void AddVertex(Vector3 vertex){
         vertices.Add(vertex);
        //  if(useRenderDataForCol){
        //      colVertices.Add(vertex);
        //  }
     }

    //  public void AddTriangle(int tri){
    //      triangles.Add(tri);
    //      if(useRenderDataForCol){
    //          colTriangles.Add(tri - (vertices.Count - colVertices.Count));
    //      }
    //  }
}