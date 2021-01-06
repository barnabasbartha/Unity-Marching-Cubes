using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct VertexJob : IJobParallelFor {
   public NativeArray<Vector3> vertices;

   [ReadOnly] public float rand;

   public void Execute(int i) {
      var vertex = vertices[i];
      vertices[i] = new Vector3(vertex.x, i * rand, vertex.z);
   }
}
