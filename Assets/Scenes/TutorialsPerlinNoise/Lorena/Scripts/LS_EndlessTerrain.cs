using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LS_EndlessTerrain : MonoBehaviour
{
    public const float maxViewDst = 450;
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    static LS_MapGenerator mapGenerator;

    int chunkSize;
    int chunkVisibleInViewDst;

    //irgendwas von system.collection i actually dont knour
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    //wir müssen natürlich speichern was geladen wurde um sie wieder später auszumachen
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        mapGenerator = FindAnyObjectByType<LS_MapGenerator>();
        chunkSize = LS_MapGenerator.mapChunkSize - 1;
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }


    void UpdateVisibleChunks()
    {

        for (int i =0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                } else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
                }
            }
        }
    }
       
    //Estentiate a plane for visual placeholder until we implement with mapgenerator
    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MapData mapData;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;



        //konstrukor
        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material) //mit transform parent können wir die Planes in ein Objekt in Inspektor stecken
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;


            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;

            SetVisible(false);

            mapGenerator.RequestMapData(OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);

        }

        void OnMeshDataReceived(LS.MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }


        public void UpdateTerrainChunk()
        {
            //find point on perimiter that is closet to viewerposition and find the distance between that and if distance less than max view distance then make mesh object enabled
           float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
           bool visible = viewerDstFromNearestEdge <= maxViewDst;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }
    }


}

/*ein terrain chunk passt in ein block/grid. wir finden herraus was um den spieler ist. 
(-1,1) (0,1) (1.1)
(-1, 0) (0,0) (1, 0)
(-1, -1) (0, -1) (1, -1)
*/