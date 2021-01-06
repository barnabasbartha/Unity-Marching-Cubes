using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class PlaneManager : MonoBehaviour {
   public Material material;

   private int prevSize;
   [Range(3, 75)] public int size;

   private Mesh mesh;
   // private NativeArray<Vector3> vertices;

   private void Start() {
      mesh = new Mesh();
      //mesh.RecalculateNormals();
      //mesh.RecalculateBounds();

      EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
      EntityArchetype archetype = entityManager.CreateArchetype(
         typeof(Translation),
         typeof(RenderMesh),
         typeof(LocalToWorld),
         typeof(RenderBounds)
         // typeof(VertexComponent)
      );
      Entity entity = entityManager.CreateEntity(archetype);
      //entityManager.SetComponentData(entity, new VertexComponent {
      //   vertices = new Vector3[new Vector3()],
      //   triangles = new NativeArray<int>()
      //});
      entityManager.SetSharedComponentData(entity, new RenderMesh {
         mesh = mesh,
         material = material
      });

      //mesh.vertices[1].y = 4;
      //mesh.RecalculateNormals();
      //mesh.RecalculateBounds();

      // vertices = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);
      // var modifiedVertices = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);
      //var job = new VertexJob {
      //   vertices = vertices
      //};
      //job.Schedule().Complete();
      // job.vertices.CopyTo(modifiedVertices);
      // mesh.vertices = job.vertices.ToArray();

      /*
      var entityArray = new NativeArray<Entity>(1, Allocator.Temp);
      entityManager.CreateEntity(archetype, entityArray);
      foreach (var entity in entityArray) {
          // entityManager.SetComponentData(entity, new MyComponent { myValue = 10 });
          // entityManager.SetComponentData(entity, new MoveSpeedComponent { moveSpeed = Random.Range(1f, 2f});
          entityManager.SetSharedComponentData(entity, new RenderMesh
          {
              mesh = mesh,
              material = material
          });
      }
      entityArray.Dispose();
      */
   }

   private static float PerlinNoise3D(float x, float y, float z) {
      y += 1;
      z += 2;
      float xy = _perlin3DFixed(x, y);
      float xz = _perlin3DFixed(x, z);
      float yz = _perlin3DFixed(y, z);
      float yx = _perlin3DFixed(y, x);
      float zx = _perlin3DFixed(z, x);
      float zy = _perlin3DFixed(z, y);
      return xy * xz * yz * yx * zx * zy;
   }

   static float _perlin3DFixed(float a, float b) {
      return Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a, b));
   }


   private void Update() {
      if (size == prevSize) return;
      prevSize = size;
      GenerateMesh();

      /*
      var job = new VertexJob {
         vertices = vertices,
         rand = Random.Range(-1f, 1f)
      };
      var jobHandle = job.Schedule(vertices.Length, 1);
      jobHandle.Complete();
      mesh.SetVertices(job.vertices);
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();
      */
   }

   private void GenerateMesh() {
      var voxels = new float[size * size * size];
      var vertices = new List<Vector3>();
      var triangles = new List<int>();

      for (int x = 0; x < size; x++) {
         for (int y = 0; y < size; y++) {
            for (int z = 0; z < size; z++) {
               float fx = x / (size - 1.0f);
               float fy = y / (size - 1.0f);
               float fz = z / (size - 1.0f);

               int idx = x + y * size + z * size * size;

               voxels[idx] = PerlinNoise3D(fx, fy, fz);
            }
         }
      }

      var marching = new MarchingCubes();
      var time = Time.realtimeSinceStartup;
      marching.Generate(voxels, size, vertices, triangles);
      Debug.Log((Time.realtimeSinceStartup - time) * 1000 + "ms, vertices: " + vertices.Count);
      mesh.SetVertices(vertices);
      mesh.SetTriangles(triangles.ToArray(), 0);
      mesh.RecalculateNormals();
      mesh.RecalculateBounds();
   }

   private void OnDestroy() {
      // vertices.Dispose();
   }
}
