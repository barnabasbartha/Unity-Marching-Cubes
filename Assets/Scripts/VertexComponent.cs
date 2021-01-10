using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct VertexComponent : IComponentData {
   public NativeArray<float> levels;
   public int size;

   public NativeList<Vector3> vertices;
   public NativeList<int> triangles;
   public NativeArray<float> cube;
   public NativeArray<Vector3> edgeVertex;

   public NativeArray<int> vertexOffset;
   public NativeArray<int> edgeConnection;
   public NativeArray<float> edgeDirection;
   public NativeArray<int> cubeEdgeFlags;
   public NativeArray<int> triangleConnectionTable;

   public bool needsUpdate;
}
