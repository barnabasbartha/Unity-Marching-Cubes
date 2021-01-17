using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour {
   private static float TOLERANCE = 0.0001f;

   public ComputeShader shader;
   public Material material;
   [Range(1, 8)] public int cellSizeMultiplier = 1;
   private int prevSizeMultiplier;
   [Range(.1f, 30f)] public float noiseScale = 1f;
   private float prevNoiseScale;
   [Range(-10f, 10f)] public float noiseOffset;
   private float prevNoiseOffset;

   [Range(1, 6)] public int gridSize = 3;
   [Range(.1f, 10f)] public float gridScale = 2f;
   private List<GameObject> chunks;


   void Start() {
      chunks = new List<GameObject>();
      CreateGrid();
   }

   private void CreateGrid() {
      for (int x = 0; x < gridSize; x++) {
         for (int y = 0; y < gridSize; y++) {
            for (int z = 0; z < gridSize; z++) {
               var chunkObject = new GameObject("Chunk",
                  typeof(MeshFilter),
                  typeof(MeshRenderer),
                  // typeof(MeshCollider),
                  typeof(Chunk)
               );
               var position = new Vector3(
                  x * cellSizeMultiplier * 8,
                  y * cellSizeMultiplier * 8,
                  z * cellSizeMultiplier * 8
               );
               var scaledPosition = position * gridScale;
               var chunkComponent = chunkObject.GetComponent<Chunk>();
               chunkComponent.offset = position;
               chunkComponent.shader = shader;
               chunkComponent.material = material;
               chunkComponent.sizeMultiplier = cellSizeMultiplier;
               var transformComponent = chunkObject.GetComponent<Transform>();
               transformComponent.position = scaledPosition;
               transformComponent.localScale = new Vector3(gridScale, gridScale, gridScale);
               chunks.Add(chunkObject);
            }
         }
      }
   }

   private void Update() {
      if (cellSizeMultiplier != prevSizeMultiplier ||
          Math.Abs(noiseOffset - prevNoiseOffset) > TOLERANCE ||
          Math.Abs(noiseScale - prevNoiseScale) > TOLERANCE) {
         prevSizeMultiplier = cellSizeMultiplier;
         prevNoiseOffset = noiseOffset;
         prevNoiseScale = noiseScale;
         foreach (var o in chunks) {
            UpdateChunk(o.GetComponent<Chunk>());
         }
      }
   }

   private void UpdateChunk(Chunk chunk) {
      chunk.sizeMultiplier = cellSizeMultiplier;
      chunk.noiseScale = noiseScale;
      chunk.noiseOffset = noiseOffset;
      chunk.ResetLevels();
      chunk.BuildMesh();
   }
}
