using Unity.Collections;
using UnityEngine;

public class Plane : MonoBehaviour {
   public static readonly int SIZE = 10 + 1;

   private Mesh mesh;

   private NativeArray<float> levels;
   public Material material;

   public NativeList<Vector3> vertices;
   public NativeList<int> triangles;
   public NativeHashMap<int, int> vertexIndices;
   public NativeArray<float> cube;
   public NativeArray<Vector3> edgeVertex;

   private Vector3 position;


   private void Start() {
      mesh = new Mesh();
      mesh.MarkDynamic();
      gameObject.AddComponent<MeshFilter>();
      gameObject.GetComponent<MeshFilter>().mesh = mesh;
      gameObject.AddComponent<MeshRenderer>();
      gameObject.GetComponent<MeshRenderer>().material = material;
      position = gameObject.GetComponent<Transform>().position;

      vertices = new NativeList<Vector3>(Allocator.Persistent);
      triangles = new NativeList<int>(Allocator.Persistent);
      cube = new NativeArray<float>(8, Allocator.Persistent);
      edgeVertex = new NativeArray<Vector3>(12, Allocator.Persistent);

      levels = new NativeArray<float>(SIZE * SIZE * SIZE, Allocator.TempJob);
   }

   public void UpdateLevels(float noiseOffset, float noiseScale) {
      float size1 = SIZE - 1.0f;
      float sizep2 = size1 + noiseScale;

      for (int x = 0; x < SIZE; x++) {
         for (int y = 0; y < SIZE; y++) {
            for (int z = 0; z < SIZE; z++) {
               float fx = (x + position.x + noiseOffset) / sizep2;
               float fy = (y + position.y + noiseOffset) / sizep2;
               float fz = (z + position.z + noiseOffset) / sizep2;

               int idx = x + y * SIZE + z * SIZE * SIZE;

               levels[idx] = PerlinNoise3D(fx, fy, fz);
            }
         }
      }
      Clear();
   }

   private void Clear() {
      vertexIndices = new NativeHashMap<int, int>(SIZE, Allocator.TempJob);
      mesh.Clear();
      vertices.Clear();
      triangles.Clear();
   }

   public void FinishUpdateMesh() {
      mesh.SetVertices(vertices.ToArray());
      mesh.SetTriangles(triangles.ToArray(), 0);
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();
      mesh.RecalculateTangents();
      vertexIndices.Dispose();
   }

   private void OnDestroy() {
      levels.Dispose();
      vertices.Dispose();
      triangles.Dispose();
      cube.Dispose();
      edgeVertex.Dispose();
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

   private static float _perlin3DFixed(float a, float b) {
      return Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a, b));
   }
}
