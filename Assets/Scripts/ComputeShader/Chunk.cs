using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer) /*, typeof(MeshCollider)*/)]
public class Chunk : MonoBehaviour {
   public const int MAX_VERTEX_PER_BLOCK = 12;
   public const int MAX_TRIANGLE_PER_BLOCK = 5;
   public const float ISO_LEVEL = .5f;
   public ComputeShader shader;
   public Material material;
   public int size;
   private int size1;
   private int N3;

   // private MeshCollider meshCollider;
   private MeshRenderer meshRenderer;
   private MeshFilter meshFilter;
   private Mesh mesh;

   // Used to store the noise data
   // private ComputeBuffer gridBuffer;

   // Used to store triangle vertices
   private ComputeBuffer trisBuffer;

   // Used to store the amount of triangle in trisBuffer
   private ComputeBuffer trisLengthBuffer;

   // Used to store the value of countBuffer
   private int[] countBufferArray = {0};

   // Used to temporary store data from trisBuffer
   private float3[] trisBufferTempArray;

   // Used to store grid data
   private NativeArray<float4> gridData;

   void Start() {
      size1 = size + 1;
      N3 = size1 * size1 * size1;

      // meshCollider = GetComponent<MeshCollider>();
      meshFilter = GetComponent<MeshFilter>();
      meshRenderer = GetComponent<MeshRenderer>();
      mesh = new Mesh();
      mesh.MarkDynamic();

      meshFilter.mesh = mesh;
      // meshCollider.sharedMesh = mesh;
      meshRenderer.material = material;

      // gridBuffer = new ComputeBuffer(N3, sizeof(float) * 4, ComputeBufferType.Default);
      trisBuffer = new ComputeBuffer(N3 * MAX_TRIANGLE_PER_BLOCK * 3, sizeof(float) * 3, ComputeBufferType.Append);
      trisLengthBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
      trisBufferTempArray = new float3[N3 * MAX_TRIANGLE_PER_BLOCK * 3];

      BuildMesh();
   }

   public void BuildMesh() {
      var kernel = shader.FindKernel("CSMain");

      // gridBuffer.SetData(gridData, 0, 0, gridData.Length);
      trisBuffer.SetCounterValue(0);

      //shader.SetInts("m_Size", size1, size1, size1);
      //shader.SetFloats("m_Isolevel", ISO_LEVEL);
      //shader.SetBuffer(k, "m_GridPointBuffer", gridBuffer);
      shader.SetBuffer(kernel, "m_TriBuffer", trisBuffer);

      shader.GetKernelThreadGroupSizes(kernel, out uint kx, out uint ky, out uint kz);

      var thgx = (int) math.ceil(1 / (float) kx);
      var thgy = (int) math.ceil(1 / (float) ky);
      var thgz = (int) math.ceil(1 / (float) kz);

      shader.Dispatch(kernel, thgx, thgy, thgz);

      ComputeBuffer.CopyCount(trisBuffer, trisLengthBuffer, 0);
      trisLengthBuffer.GetData(countBufferArray, 0, 0, 1);

      var trisBufferLength = countBufferArray[0];

      if (trisBufferLength == 0)
         return;

      trisBuffer.GetData(trisBufferTempArray, 0, 0, trisBufferLength);

      var vertices = new NativeArray<Vector3>(trisBufferLength, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
      var triangles = new NativeArray<int>(trisBufferLength, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
      for (var i = 0; i < trisBufferLength; i++) {
         var vertex = trisBufferTempArray[i];
         vertices[i] = new Vector3(vertex.x, vertex.y, vertex.z);
         triangles[i] = i;
      }

      mesh.SetVertices(vertices);
      mesh.SetTriangles(triangles.ToArray(), 0);
      mesh.RecalculateNormals();
      mesh.RecalculateBounds();
   }

   private void OnDestroy() {
      // gridBuffer.Dispose();
      trisBuffer.Dispose();
      trisLengthBuffer.Dispose();
   }
}
