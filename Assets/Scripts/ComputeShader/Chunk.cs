using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer) /*, typeof(MeshCollider)*/)]
public class Chunk : MonoBehaviour {
   public const float SURFACE = .5f;
   public ComputeShader shader;
   public Material material;
   public float noiseOffset;
   public float noiseScale;
   public int sizeMultiplier;
   private int size;
   private int N3;

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
   private int thsize;

   void Start() {
      // meshCollider = GetComponent<MeshCollider>();
      meshFilter = GetComponent<MeshFilter>();
      meshRenderer = GetComponent<MeshRenderer>();
      mesh = new Mesh();
      mesh.MarkDynamic();

      meshFilter.mesh = mesh;
      // meshCollider.sharedMesh = mesh;
      meshRenderer.material = material;

      kernel = shader.FindKernel("cs_main");
      shader.SetFloats("surface", SURFACE);
   }

   private Vector3[] vertices;
   private int[] triangles;

   public void BuildMesh() {
      UpdateVariables();

      levelsBuffer.SetData(levels, 0, 0, levels.Length);
      trianglesBuffer.SetCounterValue(0);
      shader.SetInts("size", size);

      shader.Dispatch(kernel, thsize, thsize, thsize);

      ComputeBuffer.CopyCount(trianglesBuffer, trianglesCountBuffer, 0);
      trianglesCountBuffer.GetData(countBufferArray, 0, 0, 1);

      var nrOfTriangles = countBufferArray[0];
      // Debug.Log(size);
      // Debug.Log(nrOfTriangles);

      if (nrOfTriangles == 0)
         return;

      trianglesBuffer.GetData(trisBufferTempArray, 0, 0, nrOfTriangles);

      //var vertices = new NativeArray<Vector3>(nrOfVertices, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
      //var triangles = new NativeArray<int>(nrOfVertices, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
      if (vertices == null || nrOfTriangles * 3 > vertices.Length) {
         vertices = new Vector3[nrOfTriangles * 3 * 2];
      }
      // TODO: Decrease

      if (triangles == null || nrOfTriangles * 3 != triangles.Length) {
         triangles = new int[nrOfTriangles * 3];
      }

      for (var i = 0; i < nrOfTriangles; i++) {
         var triangle = trisBufferTempArray[i];
         if (maxI < i) {
            vertices[i] = new Vector3();
            vertices[i + 1] = new Vector3();
            vertices[i + 2] = new Vector3();
         }

         vertices[i * 3].x = triangle.a.x;
         vertices[i * 3].y = triangle.a.y;
         vertices[i * 3].z = triangle.a.z;
         vertices[i * 3 + 1].x = triangle.b.x;
         vertices[i * 3 + 1].y = triangle.b.y;
         vertices[i * 3 + 1].z = triangle.b.z;
         vertices[i * 3 + 2].x = triangle.c.x;
         vertices[i * 3 + 2].y = triangle.c.y;
         vertices[i * 3 + 2].z = triangle.c.z;
         triangles[i * 3] = i * 3;
         triangles[i * 3 + 1] = i * 3 + 1;
         triangles[i * 3 + 2] = i * 3 + 2;
      }

      maxI = nrOfTriangles;
      mesh.SetVertices(vertices, 0, nrOfTriangles * 3);
      mesh.SetTriangles(triangles, 0);
      mesh.RecalculateNormals();
      mesh.RecalculateBounds();
   }

   private void UpdateVariables() {
      if (!levels.IsCreated) {
         size = sizeMultiplier * 8;
         N3 = size * size * size;

         shader.GetKernelThreadGroupSizes(kernel, out uint kx, out uint ky, out uint kz);
         thsize = (int) math.ceil(size / (float) kx);

         levels = new NativeArray<float>(N3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
         levelsBuffer = new ComputeBuffer(N3, sizeof(float), ComputeBufferType.Default);
         trianglesBuffer = new ComputeBuffer(N3 * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
         trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
         trisBufferTempArray = new Triangle[N3 * 5];
         shader.SetBuffer(kernel, "levels", levelsBuffer);
         shader.SetBuffer(kernel, "triangles", trianglesBuffer);
      }

      new NoiseJob {
         levels = levels,
         size = size,
         noiseScale = noiseScale,
         noiseOffset = noiseOffset
      }.Schedule(N3, 64).Complete();
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

   struct Triangle {
      public float3 a;
      public float3 b;
      public float3 c;
   }
}
