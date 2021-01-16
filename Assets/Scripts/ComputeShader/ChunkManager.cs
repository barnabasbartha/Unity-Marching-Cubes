using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour {
   public ComputeShader shader;
   public Material material;
   [Range(1, 8)] public int sizeMultiplier = 1;
   private int prevSizeMultiplier;
   [Range(.1f, 30f)] public float noiseScale = 1f;
   private float prevNoiseScale;
   [Range(-10f, 10f)] public float noiseOffset;
   private float prevNoiseOffset;

   private List<GameObject> chunks;

   public static float TOLERANCE = 0.0001f;

   void Start() {
      chunks = new List<GameObject>();

      var chunkObject = new GameObject("Chunk",
         typeof(MeshFilter),
         typeof(MeshRenderer),
         // typeof(MeshCollider),
         typeof(Chunk)
      );
      var chunkComponent = chunkObject.GetComponent<Chunk>();
      chunkComponent.shader = shader;
      chunkComponent.material = material;
      chunkComponent.sizeMultiplier = sizeMultiplier;
      chunks.Add(chunkObject);
   }

   private void Update() {
      if (sizeMultiplier != prevSizeMultiplier || Math.Abs(noiseOffset - prevNoiseOffset) > TOLERANCE ||
          Math.Abs(noiseScale - prevNoiseScale) > TOLERANCE) {
         prevSizeMultiplier = sizeMultiplier;
         prevNoiseOffset = noiseOffset;
         prevNoiseScale = noiseScale;
         UpdateChunk();
      }
   }

   private void UpdateChunk() {
      var chunk = chunks[0].GetComponent<Chunk>();
      chunk.sizeMultiplier = sizeMultiplier;
      chunk.noiseScale = noiseScale;
      chunk.noiseOffset = noiseOffset;
      chunk.BuildMesh();
   }
}
