using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk2 : MonoBehaviour
{
    public const int chunkSize = 8;
    public const float voxelSize = 0.5f;
    public const int chunkVoxels = 16;
    public const float sDistLimit =  1 ;//3f*voxelSize;  // 10000000;

    [SerializeField]
    private WorldPos pos;
    public WorldPos chunkPos {get => pos; private set => pos = value;}

    public Columns2 column {get; private set;}

    private Voxel [, ,] voxels = new Voxel[chunkVoxels,chunkVoxels,chunkVoxels];
    private Dictionary<Vector3, SurfPt> surfPts = new Dictionary<Vector3, SurfPt>();

    private MeshFilter filter;
    private MeshCollider coll;

    static bool splatter = false;

    private MyMesh meshData;
    private MyMesh farMesh;

    private World world;

    public bool updating {get; private set;} = false;
    public bool rendered {get; private set;} = false;

    // Start is called before the first frame update
    void Start()
    {
        filter = gameObject.GetComponent<MeshFilter>();
        coll = gameObject.GetComponent<MeshCollider>();
        world = World.GetWorld();
    }


    public void SetPos(WorldPos pos){
        chunkPos = pos;
    }

    public void SetColumn(Columns2 col){
        column = col;
    }

    //Gets Voxel at location, converts location to array location
    public Voxel GetVoxel(float x, float y, float z){
        if(InRange(x,y,z)){
            int xi = Mathf.FloorToInt(x/voxelSize);
            int yi = Mathf.FloorToInt(y/voxelSize);
            int zi = Mathf.FloorToInt(z/voxelSize);
            return voxels[xi,yi,zi];
        }
        else{
            return world.GetVoxel(chunkPos.x+x,chunkPos.y+y,chunkPos.z+z);
        }        
    } 

    public Voxel GetVoxel(WorldPos pos){
        if(InRange(pos)){
            return voxels[pos.xi,pos.yi,pos.zi];
        }
        else{
            return world.GetVoxel(WorldPos.Add(this.chunkPos,pos));
        }
    }

    //Gets Voxel at location, input array location
    public Voxel GetVoxel(int xi, int yi, int zi){
        if(InRange(xi,yi,zi)){
            return voxels[xi,yi,zi];
        }
        else{
            return world.GetVoxel(chunkPos.xi+xi,chunkPos.yi+yi,chunkPos.zi+zi);
        }
    }

     public void SetVoxel(int xi, int yi, int zi, Voxel voxel){
        if(InRange(xi,yi,zi)){
            voxels[xi,yi,zi] = voxel;
        }
        else{
            world.SetVoxel(WorldPos.Add(new WorldPos(xi,yi,zi),this.chunkPos), voxel);
        }
    }

    public void SetVoxel(WorldPos pos ,Voxel voxel){
        if(InRange(pos)){
            voxels[pos.xi,pos.yi,pos.zi] = voxel;
        }
        else{
            world.SetVoxel(WorldPos.Add(this.chunkPos,pos),voxel);
        }
    }


    //Checks if coordinate is in chunk
    public static bool InRange(float x,float y, float z){
        if(x < 0 || x >= chunkSize || y < 0 || y >= chunkSize || z < 0 || z >= chunkSize){
            return false;
        }
        return true;
    }

    //WorldPos version of InRange
    public static bool InRange(WorldPos pos){
        if(pos.xi < 0 || pos.xi >= chunkVoxels || pos.yi < 0 || pos.yi >= chunkVoxels || pos.zi < 0 || pos.zi >= chunkVoxels){
            return false;
        }
        return true;
    }

    //int version of InRange
    public static bool InRange(int xi,int yi, int zi){
        if(xi < 0 || xi >= chunkVoxels || yi < 0 || yi >= chunkVoxels || zi < 0 || zi >= chunkVoxels){
            return false;
        }
        return true;
    }

     // Main code for updating and calculating surface of the Chunk
    public void UpdateChunk2(object stateIn){

        Chunk2 state = (Chunk2) stateIn;
        //Updates surface points                   
        state.SurfacePoints();
        
        MyMesh meshData = new MyMesh();
        meshData.useRenderDataForCol = true;

        // Booleans to check whether there is a connecting side mesh
        // bool side1= true,side2= true,side3= true,side4 = true;
        
        //Variables and constants for convenience
        Voxel voxel0, voxel1, voxel2, temp, temp2;
        // float vS = voxelSize;
        SurfPt surfpt1,surfpt2,surfpt3;
        foreach (KeyValuePair<Vector3, SurfPt> entry in state.surfPts){
            //Veryfy there is a value at the Key location in dictionary
            if(entry.Value != null){
                //Sets values for key and a voxel for easier access and reuse
                float x = entry.Key.x, y = entry.Key.y, z = entry.Key.z;
                voxel0 = state.GetVoxel(x,y,z);
                voxel2 = state.GetVoxel(x+voxelSize,y+voxelSize,z+voxelSize);

                //Verifys if the mesh is supposed to be formed in case of close surface points
                voxel1 = state.GetVoxel(x+voxelSize,y,z+voxelSize);
                if(!Voxel.SameSignsDistF(voxel1,voxel2)){
                    //Checks if there are more surface points to form an mesh on the xz plane
                    if(state.surfPts.TryGetValue(new Vector3(x+voxelSize,y,z),out surfpt1) && state.surfPts.TryGetValue(new Vector3(x,y,z+voxelSize),out surfpt2) && state.surfPts.TryGetValue(new Vector3(x+voxelSize,y,z+voxelSize),out surfpt3)){
                        //Calculates the booleans to check weather there are connecting meshes on all sides
                        // side1 = Contunity(x-vS,y,z, x-vS,y,z+vS) || Contunity(x,y+vS,z, x,y+vS,z+vS) || Contunity(x,y-vS,z, x,y-vS,z+vS);
                        // side2 = Contunity(x,y,z+2*vS, x+vS,y,z+2*vS) || Contunity(x,y+vS,z+vS, x+vS,y+vS,z+vS) || Contunity(x,y-vS,z+vS, x+vS,y-vS,z+vS);
                        // side3 = Contunity(x+2*vS,y,z, x+2*vS,y,z+vS) || Contunity(x+vS,y+vS,z, x+vS,y+vS,z+vS) || Contunity(x+vS,y-vS,z, x+vS,y-vS,z+vS);
                        // side4 = Contunity(x,y,z-vS, x+vS,y,z-vS) || Contunity(x,y+vS,z, x+vS,y+vS,z) || Contunity(x,y-vS,z, x+vS,y-vS,z);

                        
                        //Chooses which direction to render mesh
                        if(voxel1.sDistF < voxel2.sDistF ){
                            meshData.AddVertex(new Vector3(surfpt1.x,surfpt1.y,surfpt1.z));
                            meshData.AddVertex(new Vector3(surfpt3.x,surfpt3.y,surfpt3.z));
                            meshData.AddVertex(new Vector3(surfpt2.x,surfpt2.y,surfpt2.z));  
                            meshData.AddVertex(new Vector3(entry.Value.x,entry.Value.y,entry.Value.z));                                                     
                        }
                        else{
                            meshData.AddVertex(new Vector3(surfpt1.x,surfpt1.y,surfpt1.z));
                            meshData.AddVertex(new Vector3(entry.Value.x,entry.Value.y,entry.Value.z));
                            meshData.AddVertex(new Vector3(surfpt2.x,surfpt2.y,surfpt2.z));
                            meshData.AddVertex(new Vector3(surfpt3.x,surfpt3.y,surfpt3.z));
                        }
                        meshData.AddQuadTriangles();
                        if(voxel0.sDistF<0){
                            meshData.uv.AddRange(voxel0.FaceUVs());

                            if(splatter){
                                temp = state.GetVoxel(x+voxelSize,y,z);
                                state.AirVoxelFace(temp, voxel0, meshData.uv2);

                                temp = state.GetVoxel(x,y,z+voxelSize);
                                state.AirVoxelFace(temp, voxel0, meshData.uv3);
                                
                                temp = state.GetVoxel(x+voxelSize,y,z+voxelSize);
                                state.AirVoxelFace(temp, voxel0, meshData.uv4);
                            }
                        }
                        else{
                            if(voxel1.sDistF<0){
                                meshData.uv.AddRange(voxel1.FaceUVs());
                                temp2 = voxel1;
                            }
                            else if(voxel2.sDistF<0){
                                meshData.uv.AddRange(voxel2.FaceUVs());
                                temp2 = voxel2;
                            }
                            else{
                                meshData.uv.AddRange(voxel0.FaceUVs());
                                temp2 = voxel0;
                            }
                            

                            if(splatter){
                                temp = state.GetVoxel(x+voxelSize,y,z);
                                state.AirVoxelFace(temp, temp2, meshData.uv2);

                                temp = state.GetVoxel(x,y,z+voxelSize);
                                state.AirVoxelFace(temp, temp2, meshData.uv3);
                                
                                temp = state.GetVoxel(x+voxelSize,y,z+voxelSize);
                                state.AirVoxelFace(temp, temp2, meshData.uv4);
                            }
                        }
                    }
                }
                //Repeat same code but for xy plane mesh
                voxel1 = state.GetVoxel(x+voxelSize,y+voxelSize,z);
                if(!Voxel.SameSignsDistF(voxel1,voxel2)){
                    if(state.surfPts.TryGetValue(new Vector3(x+voxelSize,y,z),out surfpt1) && state.surfPts.TryGetValue(new Vector3(x,y+voxelSize,z),out surfpt2) && state.surfPts.TryGetValue(new Vector3(x+voxelSize,y+voxelSize,z),out surfpt3)){
                        // side1 = Contunity(x-vS,y,z, x-vS,y+vS,z) || Contunity(x,y,z+vS, x,y+vS,z+vS) || Contunity(x,y,z-vS, x,y+vS,z-vS);
                        // side2 = Contunity(x,y+2*vS,z, x+vS,y+2*vS,z) || Contunity(x,y+vS,z+vS, x+vS,y+vS,z+vS) || Contunity(x,y+vS,z-vS, x+vS,y+vS,z-vS);
                        // side3 = Contunity(x+2*vS,y,z, x+2*vS,y+vS,z) || Contunity(x+vS,y,z+vS, x+vS,y+vS,z+vS) || Contunity(x+vS,y,z-vS, x+vS,y+vS,z-vS);
                        // side4 = Contunity(x,y-vS,z, x+vS,y-vS,z) || Contunity(x,y,z+vS, x+vS,y,z+vS) || Contunity(x,y,z-vS, x+vS,y,z-vS);

                        
                        if(voxel1.sDistF < voxel2.sDistF ){
                            meshData.AddVertex(new Vector3(surfpt1.x,surfpt1.y,surfpt1.z));
                            meshData.AddVertex(new Vector3(entry.Value.x,entry.Value.y,entry.Value.z));
                            meshData.AddVertex(new Vector3(surfpt2.x,surfpt2.y,surfpt2.z));
                            meshData.AddVertex(new Vector3(surfpt3.x,surfpt3.y,surfpt3.z));
                        }
                        else{
                            meshData.AddVertex(new Vector3(surfpt1.x,surfpt1.y,surfpt1.z));
                            meshData.AddVertex(new Vector3(surfpt3.x,surfpt3.y,surfpt3.z));
                            meshData.AddVertex(new Vector3(surfpt2.x,surfpt2.y,surfpt2.z));  
                            meshData.AddVertex(new Vector3(entry.Value.x,entry.Value.y,entry.Value.z));                                                     
                        }
                        meshData.AddQuadTriangles();
                        if(voxel0.sDistF<0){
                            meshData.uv.AddRange(voxel0.FaceUVs());

                            if(splatter){
                                temp = state.GetVoxel(x+voxelSize,y,z);
                                state.AirVoxelFace(temp, voxel0, meshData.uv2);

                                temp = state.GetVoxel(x,y+voxelSize,z);
                                state.AirVoxelFace(temp, voxel0, meshData.uv3);
                                
                                temp = state.GetVoxel(x+voxelSize,y+voxelSize,z);
                                state.AirVoxelFace(temp, voxel0, meshData.uv4);
                            }
                        }
                        else{
                            if(voxel1.sDistF<0){
                                meshData.uv.AddRange(voxel1.FaceUVs());
                                temp2 = voxel1;
                            }
                            else if(voxel2.sDistF<0){
                                meshData.uv.AddRange(voxel2.FaceUVs());
                                temp2 = voxel2;
                            }
                            else{
                                meshData.uv.AddRange(voxel0.FaceUVs());
                                temp2 = voxel0;
                            }
                            

                            if(splatter){
                                temp = state.GetVoxel(x+voxelSize,y,z);
                                state.AirVoxelFace(temp, temp2, meshData.uv2);

                                temp = state.GetVoxel(x,y+voxelSize,z);
                                state.AirVoxelFace(temp, temp2, meshData.uv3);
                                
                                temp = state.GetVoxel(x+voxelSize,y+voxelSize,z);
                                state.AirVoxelFace(temp, temp2, meshData.uv4);
                            }
                        }
                    }
                }
                //Repeat same code but for yz plane mesh
                voxel1 = state.GetVoxel(x,y+voxelSize,z+voxelSize);
                if(!Voxel.SameSignsDistF(voxel1,voxel2)){  
                    if(state.surfPts.TryGetValue(new Vector3(x,y+voxelSize,z),out surfpt1) && state.surfPts.TryGetValue(new Vector3(x,y,z+voxelSize),out surfpt2) && state.surfPts.TryGetValue(new Vector3(x,y+voxelSize,z+voxelSize),out surfpt3)){                    
                        // side1 = Contunity(x,y-vS,z, x,y-vS,z+vS) || Contunity(x+vS,y,z, x+vS,y,z+vS) || Contunity(x-vS,y,z, x-vS,y,z+vS);
                        // side2 = Contunity(x,y,z+2*vS, x,y+vS,z+2*vS) || Contunity(x+vS,y,z+vS, x+vS,y+vS,z+vS) || Contunity(x-vS,y,z+vS, x-vS,y+vS,z+vS);
                        // side3 = Contunity(x,y+2*vS,z, x,y+2*vS,z+vS) || Contunity(x+vS,y+vS,z, x+vS,y+vS,z+vS) || Contunity(x-vS,y+vS,z, x-vS,y+vS,z+vS);
                        // side4 = Contunity(x,y,z-vS, x,y+vS,z-vS) || Contunity(x+vS,y,z, x+vS,y+vS,z) || Contunity(x-vS,y,z, x-vS,y+vS,z);
                    
                        
                        if(voxel1.sDistF < voxel2.sDistF){
                            meshData.AddVertex(new Vector3(surfpt1.x,surfpt1.y,surfpt1.z));
                            meshData.AddVertex(new Vector3(entry.Value.x,entry.Value.y,entry.Value.z));
                            meshData.AddVertex(new Vector3(surfpt2.x,surfpt2.y,surfpt2.z));
                            meshData.AddVertex(new Vector3(surfpt3.x,surfpt3.y,surfpt3.z));                    
                        }
                        else{
                            meshData.AddVertex(new Vector3(surfpt1.x,surfpt1.y,surfpt1.z));
                            meshData.AddVertex(new Vector3(surfpt3.x,surfpt3.y,surfpt3.z));
                            meshData.AddVertex(new Vector3(surfpt2.x,surfpt2.y,surfpt2.z));
                            meshData.AddVertex(new Vector3(entry.Value.x,entry.Value.y,entry.Value.z));                                                      
                        }
                        meshData.AddQuadTriangles();
                        if(voxel0.sDistF<0){
                            meshData.uv.AddRange(voxel0.FaceUVs());

                            if(splatter){
                                temp = state.GetVoxel(x,y+voxelSize,z);
                                state.AirVoxelFace(temp, voxel0, meshData.uv2);

                                temp = state.GetVoxel(x,y,z+voxelSize);
                                state.AirVoxelFace(temp, voxel0, meshData.uv3);
                                
                                temp = state.GetVoxel(x,y+voxelSize,z+voxelSize);
                                state.AirVoxelFace(temp, voxel0, meshData.uv4);
                            }
                        }
                        else{
                            if(voxel1.sDistF<0){
                                meshData.uv.AddRange(voxel1.FaceUVs());
                                temp2 = voxel1;
                            }
                            else if(voxel2.sDistF<0){
                                meshData.uv.AddRange(voxel2.FaceUVs());
                                temp2 = voxel2;
                            }
                            else{
                                meshData.uv.AddRange(voxel0.FaceUVs());
                                temp2 = voxel0;
                            }
                            

                            if(splatter){
                                temp = state.GetVoxel(x,y+voxelSize,z);
                                state.AirVoxelFace(temp, temp2, meshData.uv2);

                                temp = state.GetVoxel(x,y,z+voxelSize);
                                state.AirVoxelFace(temp, temp2, meshData.uv3);
                                
                                temp = state.GetVoxel(x,y+voxelSize,z+voxelSize);
                                state.AirVoxelFace(temp, temp2, meshData.uv4);
                            }
                        }                 
                    }
                }
            }
        }
        // RenderMesh(meshData);
        state.meshData = meshData;
        state.updating = false;
    }

    void AirVoxelFace(Voxel temp, Voxel voxel0, List<Vector2> uv){
        if(temp.sDistF > 0){
            uv.AddRange(voxel0.FaceUVs());
        }
        else{
            uv.AddRange(temp.FaceUVs());
        }
    }


    bool Contunity(float x1, float y1, float z1, float x2, float y2, float z2){
        SurfPt surfPt = null; 
        if(surfPts.TryGetValue(new Vector3(x1,y1,z1), out surfPt) && surfPts.TryGetValue(new Vector3(x2,y2,z2), out surfPt)){
            return true;
        }
        else if(x1 == 0 || x1 == chunkSize || y1 == 0 || y1 == chunkSize || z1 == 0 || z1 == chunkSize){
            return true;
        }
        else{
            return false;
        }
    }

    public static bool SameSignsDistF(float v1, float v2){
        return ((v1 >= 0 && v2 >= 0)  || (v1 < 0 && v2 < 0));
    }

    //Calculates Surface points Dictionary
    void SurfacePoints(){
        SurfPt surfPt = null;
        surfPts = new Dictionary<Vector3, SurfPt>();
        for(int y = 0;y <= chunkVoxels; y++){
            for(int x = 0; x<= chunkVoxels; x++){
                for(int z = 0; z<=chunkVoxels; z++){
                    surfPt = GetVoxel(x,y,z).FindSurfacePoint(this,x,y,z);
                    if (surfPt != null){
                        surfPts.Add(new Vector3(x*voxelSize,y*voxelSize,z*voxelSize), surfPt);
                    }
                }
            }
        }
    }

    //Render Chunk Mesh
    public void RenderMesh(){        
        rendered = true;
        filter.mesh.Clear();
        filter.mesh.vertices = meshData.vertices.ToArray();
        filter.mesh.triangles = meshData.triangles.ToArray();
        filter.mesh.SetUVs(0, meshData.uv);
        if(splatter){
            filter.mesh.SetUVs(1,meshData.uv2);
            filter.mesh.SetUVs(2,meshData.uv3);
            filter.mesh.SetUVs(3,meshData.uv4);
        }
        // filter.mesh.uv = meshData.uv.ToArray();
        // filter.mesh.SetNormals(meshData.normals,0,meshData.vertices.Count);
        filter.mesh.RecalculateNormals();


        // var myMaterial = GetComponent<Renderer>().material;
        // List<Vector4> testing = new List<Vector4>();
        // // Vector4[,,] testing2 = new Vector4[chunkVoxels,chunkVoxels,chunkVoxels];
        // foreach (KeyValuePair<Vector3, SurfPt> entry in surfPts){
        //     testing.Add(new Vector4(x,y,z,0f));
        //     // int xi = Mathf.FloorToInt(x/voxelSize);
        //     // int yi = Mathf.FloorToInt(y/voxelSize);
        //     // int zi = Mathf.FloorToInt(z/voxelSize);
        //     // testing2[xi,yi,zi] = new Vector4(x,y,z,0f);
        // }
        // myMaterial.SetVectorArray("_SurfPts", testing);

        // var test = myMaterial.GetTexture("_MainTex");

        coll.sharedMesh = null;
        Mesh mesh = new Mesh();
        mesh.vertices = meshData.colVertices.ToArray();
        mesh.triangles = meshData.colTriangles.ToArray();
        mesh.RecalculateNormals();

        coll.sharedMesh = mesh;
    }

}
