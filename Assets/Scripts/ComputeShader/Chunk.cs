using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer) /*, typeof(MeshCollider)*/)]
public class Chunk : MonoBehaviour {
   private const float SURFACE = .5f;
   public ComputeShader shader;
   public Material material;
   public Vector3 offset;
   public float noiseOffset;
   public float noiseScale;
   public int sizeMultiplier;
   private int size;
   private int size1;
   private int N3;
   private int N13;

   // private MeshCollider meshCollider;
   private MeshRenderer meshRenderer;
   private MeshFilter meshFilter;
   private Mesh mesh;

   private ComputeBuffer levelsBuffer;
   private ComputeBuffer trianglesBuffer;
   private ComputeBuffer trianglesCountBuffer;
   private int[] countBufferArray = {0};
   private Triangle[] trisBufferTempArray;
   private NativeArray<float> levels;

   private int maxI;
   private int kernel;
   private int thsizex;
   private int thsizey;
   private int thsizez;

   private Vector3[] vertices;
   private int[] triangles;
   private Dictionary<int, int> vertexCache;

   private void Start() {
      // meshCollider = GetComponent<MeshCollider>();
      meshFilter = GetComponent<MeshFilter>();
      meshRenderer = GetComponent<MeshRenderer>();
      mesh = new Mesh();
      mesh.MarkDynamic();

      meshFilter.mesh = mesh;
      // meshCollider.sharedMesh = mesh;
      meshRenderer.material = material;

      size = sizeMultiplier * 8 + 1;
      size1 = sizeMultiplier * 8;
      N3 = size * size * size;
      N13 = size1 * size1 * size1;

      kernel = shader.FindKernel("cs_main");
      shader.SetFloats("surface", SURFACE);
      shader.GetKernelThreadGroupSizes(kernel, out uint kx, out uint ky, out uint kz);
      thsizex = (int) math.ceil(size1 / (float) kx);
      thsizey = (int) math.ceil(size1 / (float) ky);
      thsizez = (int) math.ceil(size1 / (float) kz);

      levels = new NativeArray<float>(N3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
      levelsBuffer = new ComputeBuffer(N3, sizeof(float), ComputeBufferType.Default);
      trianglesBuffer = new ComputeBuffer(N13 * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
      trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
      trisBufferTempArray = new Triangle[N13 * 5];
      vertexCache = new Dictionary<int, int>();
   }

   public void ResetLevels() {
      new NoiseJob {
         levels = levels,
         size = size,
         noiseScale = noiseScale,
         noiseOffset = noiseOffset,
         offset = offset
      }.Schedule(N3, 64).Complete();
   }

   public void BuildMesh() {
      levelsBuffer.SetData(levels, 0, 0, levels.Length);
      trianglesBuffer.SetCounterValue(0);
      shader.SetInts("size", size);
      shader.SetBuffer(kernel, "levels", levelsBuffer);
      shader.SetBuffer(kernel, "triangles", trianglesBuffer);

      shader.Dispatch(kernel, thsizex, thsizey, thsizez);

      ComputeBuffer.CopyCount(trianglesBuffer, trianglesCountBuffer, 0);
      trianglesCountBuffer.GetData(countBufferArray, 0, 0, 1);

      var nrOfTriangles = countBufferArray[0];
      if (nrOfTriangles == 0)
         return;
      var nrOfTriangles3 = nrOfTriangles * 3;

      trianglesBuffer.GetData(trisBufferTempArray, 0, 0, nrOfTriangles);

      if (vertices == null || nrOfTriangles3 > vertices.Length) {
         vertices = new Vector3[nrOfTriangles3];
      }

      // TODO: Decrease

      if (triangles == null || nrOfTriangles3 != triangles.Length) {
         triangles = new int[nrOfTriangles3];
      }

      vertexCache.Clear();

      for (var i = 0; i < nrOfTriangles; i++) {
         var triangle = trisBufferTempArray[i];
         AddTriangle(i, 0, triangle.a);
         AddTriangle(i, 1, triangle.b);
         AddTriangle(i, 2, triangle.c);
      }

      maxI = nrOfTriangles;
      mesh.Clear();
      mesh.SetVertices(vertices);
      mesh.SetTriangles(triangles, 0);
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();
      mesh.RecalculateTangents();
   }

   private void AddTriangle(int i, int o, float3 vertex) {
      var j = i * 3 + o;
      if (maxI < i) {
         vertices[j] = new Vector3();
      }

      var hash = vertex.GetHashCode();
      if (!vertexCache.ContainsKey(hash)) {
         vertexCache.Add(hash, j);
         vertices[j].x = vertex.x;
         vertices[j].y = vertex.y;
         vertices[j].z = vertex.z;
      }

      triangles[j] = vertexCache[hash];
   }

   private void DisposeVariables() {
      if (levelsBuffer == null) return;
      levelsBuffer.Dispose();
      trianglesBuffer.Dispose();
      trianglesCountBuffer.Dispose();

      levels.Dispose();
   }

   private void OnDestroy() {
      DisposeVariables();
   }

   private struct Triangle {
      public float3 a;
      public float3 b;
      public float3 c;
   }

   public void SetLevel(int x, int y, int z, float value) {
      var i = x + y * size + z * size * size;
      if (levels.Length <= i || i < 0) return;
      levels[i] = Mathf.Clamp(levels[i] + value, 0f, 1f);
   }
}
