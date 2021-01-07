using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct VertexJob : IJob {
   [WriteOnly] public NativeList<Vector3> vertices;
   [WriteOnly] public NativeList<int> triangles;

   [ReadOnly] public int size;
   [ReadOnly] public NativeArray<float> levels;
   [ReadOnly] public NativeArray<int> VertexOffset;
   [ReadOnly] public NativeArray<int> EdgeConnection;
   [ReadOnly] public NativeArray<float> EdgeDirection;
   [ReadOnly] public NativeArray<int> CubeEdgeFlags;
   [ReadOnly] public NativeArray<int> TriangleConnectionTable;


   private const float SURFACE = .5f;
   public NativeArray<float> cube;
   public NativeArray<Vector3> edgeVertex;
   public NativeHashMap<int, int> vertexIndices;

   public void Execute() {
      int size1 = size - 1;
      int size2 = size1 * size1;
      int size3 = size2 * size1;
      int nrOfVertices = 0;

      for (int index = 0; index < size3; index++) {
         int x = index % size1;
         int y = (int) Math.Floor(index / (float) size1) % size1;
         int z = (int) Math.Floor(index / (float) size2);
         int flagIndex = 0;

         //Find which vertices are inside of the surface and which are outside
         for (int i = 0; i < 8; i++) {
            int ix = x + VertexOffset[i * 3];
            int iy = y + VertexOffset[i * 3 + 1];
            int iz = z + VertexOffset[i * 3 + 2];
            cube[i] = levels[ix + iy * size + iz * size * size];
            if (cube[i] <= SURFACE)
               flagIndex |= 1 << i;
         }

         //Find which edges are intersected by the surface
         int edgeFlags = CubeEdgeFlags[flagIndex];

         //If the cube is entirely inside or outside of the surface, then there will be no intersections
         if (edgeFlags == 0) continue;

         //Find the point of intersection of the surface with each edge
         for (int i = 0; i < 12; i++) {
            //if there is an intersection on this edge
            if ((edgeFlags & (1 << i)) == 0) continue;

            float offset = GetOffset(cube[EdgeConnection[i * 2]], cube[EdgeConnection[i * 2 + 1]]);

            edgeVertex[i] = new Vector3 {
               x = x + (VertexOffset[EdgeConnection[i * 2] * 3] + offset * EdgeDirection[i * 3]),
               y = y + (VertexOffset[EdgeConnection[i * 2] * 3 + 1] + offset * EdgeDirection[i * 3 + 1]),
               z = z + (VertexOffset[EdgeConnection[i * 2] * 3 + 2] + offset * EdgeDirection[i * 3 + 2])
            };
         }

         //Save the triangles that were found. There can be up to five per cube
         for (int i = 0; i < 5; i++) {
            int triIndex = flagIndex * 16 + 3 * i;
            if (TriangleConnectionTable[triIndex] < 0) break;
            for (int j = 0; j < 3; j++) {
               var vert = TriangleConnectionTable[triIndex + j];
               int hash = edgeVertex[vert].GetHashCode();
               if (!vertexIndices.ContainsKey(hash)) {
                  vertexIndices.Add(hash, nrOfVertices);
                  vertices.Add(edgeVertex[vert]);
                  nrOfVertices++;
               }
               triangles.Add(vertexIndices[hash]);
            }
         }
      }
   }

   private static float GetOffset(float v1, float v2) {
      var delta = v2 - v1;
      return delta == 0.0f ? SURFACE : (SURFACE - v1) / delta;
   }
}
