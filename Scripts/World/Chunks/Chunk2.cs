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

    private MyMesh mesh;
    private MyMesh farMesh;



    // Start is called before the first frame update
    void Start()
    {
        filter = gameObject.GetComponent<MeshFilter>();
        coll = gameObject.GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPos(WorldPos pos){
        chunkPos = pos;
    }

    public void SetColumn(Columns2 col){
        column = col;
    }
}
