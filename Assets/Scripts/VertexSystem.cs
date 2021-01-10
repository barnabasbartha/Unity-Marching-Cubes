using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;

public class VertexSystem : SystemBase {
   protected override void OnUpdate() {
      Entities
         .ForEach((ref VertexComponent entity, ref RenderMesh render) => {
            if (!entity.needsUpdate) return;
            entity.needsUpdate = false;

            var vertexIndices = new NativeHashMap<int, int>(entity.size, Allocator.TempJob);

            entity.vertices.Clear();
            entity.triangles.Clear();
            var jobHandle = new VertexJob {
               levels = entity.levels,
               vertices = entity.vertices,
               triangles = entity.triangles,
               size = entity.size,
               cube = entity.cube,
               edgeVertex = entity.edgeVertex,
               vertexIndices = vertexIndices,
               VertexOffset = entity.vertexOffset,
               EdgeConnection = entity.edgeConnection,
               EdgeDirection = entity.edgeDirection,
               CubeEdgeFlags = entity.cubeEdgeFlags,
               TriangleConnectionTable = entity.triangleConnectionTable
            }.Schedule();
            jobHandle.Complete();
            render.mesh.SetVertices(entity.vertices.ToArray());
            render.mesh.SetTriangles(entity.triangles.ToArray(), 0);
            render.mesh.RecalculateNormals();
            render.mesh.RecalculateBounds();
            vertexIndices.Dispose();
         }).WithoutBurst().Run();
   }
}
