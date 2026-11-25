using System.Collections.Generic;
using UnityEngine;

public static class LS_MeshGenerator
{
    public static MeshData GenerateTerrainMesh(
    float[,] heightMap,
    float heightMultiplier,
    AnimationCurve _heightCurve,
    int levelOfDetail)
    {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

        int meshSimplificationIncrement =
            (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

        int borderedSize = heightMap.GetLength(0);

        // The visible mesh extends from index 1 to borderedSize - 2.
        int lastMeshIndex = borderedSize - 2;
        int meshWidth = lastMeshIndex - 1;

        float topLeftX = -meshWidth / 2f;
        float topLeftZ = meshWidth / 2f;

 
        List<int> sampleCoordinates = new List<int>();

        sampleCoordinates.Add(0); 
        sampleCoordinates.Add(1); 

        for (int i = 1 + meshSimplificationIncrement;
             i < lastMeshIndex;
             i += meshSimplificationIncrement)
        {
            sampleCoordinates.Add(i);
        }

        if (sampleCoordinates[sampleCoordinates.Count - 1] != lastMeshIndex)
        {
            sampleCoordinates.Add(lastMeshIndex);
        }

        sampleCoordinates.Add(borderedSize - 1); 

        int samplesPerLine = sampleCoordinates.Count;
        int verticesPerLine = samplesPerLine - 2;

        MeshData meshData = new MeshData(verticesPerLine);

        int[,] vertexIndicesMap = new int[samplesPerLine, samplesPerLine];

        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        for (int y = 0; y < samplesPerLine; y++)
        {
            for (int x = 0; x < samplesPerLine; x++)
            {
                bool isBorderVertex =
                    y == 0 ||
                    y == samplesPerLine - 1 ||
                    x == 0 ||
                    x == samplesPerLine - 1;

                if (isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        // Generate vertices and triangles.
        for (int y = 0; y < samplesPerLine; y++)
        {
            for (int x = 0; x < samplesPerLine; x++)
            {
                int sampleX = sampleCoordinates[x];
                int sampleY = sampleCoordinates[y];

                int vertexIndex = vertexIndicesMap[x, y];

                Vector2 percent = new Vector2(
                    (sampleX - 1) / (float)meshWidth,
                    (sampleY - 1) / (float)meshWidth
                );

                float height = heightCurve.Evaluate(
                    heightMap[sampleX, sampleY]
                ) * heightMultiplier;

                Vector3 vertexPosition = new Vector3(
                    topLeftX + (sampleX - 1),
                    height,
                    topLeftZ - (sampleY - 1)
                );

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if (x < samplesPerLine - 1 &&
                    y < samplesPerLine - 1)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x + 1, y];
                    int c = vertexIndicesMap[x, y + 1];
                    int d = vertexIndicesMap[x + 1, y + 1];

                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }
            }
        }
        meshData.BakeNormals();
        return meshData;
    }

}


    public class MeshData
    {
        Vector3[] vertices;
        int[] triangles;
        Vector2[] uvs;
        Vector3[] bakedNormals;

        Vector3[] borderVertices;
        int[] borderTriangles;

        int triangleIndex;
        int borderTriangleIndex;

        public MeshData(int verticesPerLine)
        {
            vertices = new Vector3[verticesPerLine * verticesPerLine];
            uvs = new Vector2[verticesPerLine * verticesPerLine];
            triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

            borderVertices = new Vector3[verticesPerLine * 4 + 4];
            borderTriangles = new int[24 * verticesPerLine];
        }

        public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
        {
            if (vertexIndex < 0) //if bordervertex
            {
                borderVertices[-vertexIndex - 1] = vertexPosition;
            }
            else
            {
                vertices[vertexIndex] = vertexPosition;
                uvs[vertexIndex] = uv;
            }
        }


        public void AddTriangle(int a, int b, int c)
        {
             if (a < 0 || b < 0 || c < 0)
            {
                borderTriangles[borderTriangleIndex] = a;
                borderTriangles[borderTriangleIndex + 1] = b;
                borderTriangles[borderTriangleIndex + 2] = c;
                borderTriangleIndex += 3;
            }
            else
            {
                triangles[triangleIndex] = a;
                triangles[triangleIndex + 1] = b;
                triangles[triangleIndex + 2] = c;
                triangleIndex += 3;
            }
         }

    //we redo the normals/calculate them ourselves because while vertices line up between same-level-of-detail chunk line but, the normals do not. Creating a noticable seem as lighing is calculated differently.
    //The reason for that, is that the vertices in one chunk, dont hve access to the data of normals from the other chunks

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }

        }
        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();   
        }
        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA-1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        //irgendwas mit mathe die ab (a zu b) und ac (a zu c) berechnen für den normal vector oder so :(
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

        public void BakeNormals()
         {
        bakedNormals = CalculateNormals();
         }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.normals = bakedNormals; //old call CalculateNormals(); //old old call -> mesh.RecalculateNormals();
            /*to speed up the game we move calculatenormals, which takes long, into a seperate thread
             * In this case in GenerateTerrainMesh because its already in a seperate thread
             */
            return mesh;
        }
    }

/* unzentrierte map würde normal aufbauen:
 1 2 3
4 5 6
7 8 9...
wir wollen sie aber im tutorial mittig aufbauen, dafür müssen wir:

x  x x
-1 0 1
x x x */