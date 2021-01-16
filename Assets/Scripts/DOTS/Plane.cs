using Unity.Collections;
using UnityEngine;

public class Plane : MonoBehaviour {
   public static readonly int SIZE = 20;
   public static readonly int SIZE1 = SIZE + 1;
   public static readonly int SIZE3 = SIZE1 * SIZE1 * SIZE1;

   private Mesh mesh;

   public NativeArray<float> levels;
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

      levels = new NativeArray<float>(SIZE1 * SIZE1 * SIZE1, Allocator.TempJob);
   }

   public void UpdateLevels(float noiseOffset, float noiseScale) {
      float size1 = SIZE1 - 1.0f;
      float sizep2 = size1 + noiseScale;

      for (int x = 0; x < SIZE1; x++) {
         for (int y = 0; y < SIZE1; y++) {
            for (int z = 0; z < SIZE1; z++) {
               float fx = (x + position.x + noiseOffset) / sizep2;
               float fy = (y + position.y) / sizep2;
               float fz = (z + position.z) / sizep2;

               int idx = x + y * SIZE1 + z * SIZE1 * SIZE1;

               levels[idx] = PerlinNoise3D(fx, fy, fz);
            }
         }
      }

      Clear();
   }

   private void Clear() {
      vertexIndices = new NativeHashMap<int, int>(SIZE1, Allocator.TempJob);
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
