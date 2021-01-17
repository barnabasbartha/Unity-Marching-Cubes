using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour {
   private static float TOLERANCE = 0.0001f;

   public ComputeShader shader;
   public Material material;
   [Range(1, 8)] public int cellSizeMultiplier = 1;
   private int prevSizeMultiplier;
   private int cellSize;
   [Range(.1f, 30f)] public float noiseScale = 1f;
   private float prevNoiseScale;
   [Range(-10f, 10f)] public float noiseOffset;
   private float prevNoiseOffset;

   [Range(1, 6)] public int gridSize = 3;
   [Range(.1f, 10f)] public float gridScale = 2f;
   private List<GameObject> chunks;

   public GameObject cursor;
   private HashSet<Chunk> targetChunks;

   void Start() {
      chunks = new List<GameObject>();
      targetChunks = new HashSet<Chunk>();
      cellSize = cellSizeMultiplier * 8;
      CreateGrid();
      cursor.GetComponent<CursorPainter>().OnPaintEvent += AddLevel;
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
                  x * cellSize,
                  y * cellSize,
                  z * cellSize
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

   /*
    * Paint related methods
    */

   private void AddLevel(Vector3 position, int size, float value) {
      // TODO: Take scale into account
      var x = (int) Mathf.Round(position.x);
      var y = (int) Mathf.Round(position.y);
      var z = (int) Mathf.Round(position.z);
      var cellX = (int) Math.Floor(x / (float) cellSize);
      var cellY = (int) Math.Floor(y / (float) cellSize);
      var cellZ = (int) Math.Floor(z / (float) cellSize);
      var xInChunk = x % cellSize;
      var yInChunk = y % cellSize;
      var zInChunk = z % cellSize;
      AddLevelTo(cellX, cellY, cellZ, xInChunk, yInChunk, zInChunk, value);
      if (xInChunk == 0) AddLevelTo(cellX - 1, cellY, cellZ, cellSize, yInChunk, zInChunk, value);
      if (yInChunk == 0) AddLevelTo(cellX, cellY - 1, cellZ, xInChunk, cellSize, zInChunk, value);
      if (zInChunk == 0) AddLevelTo(cellX, cellY, cellZ - 1, xInChunk, yInChunk, cellSize, value);
      if (xInChunk == 0 && yInChunk == 0) AddLevelTo(cellX - 1, cellY - 1, cellZ, cellSize, cellSize, zInChunk, value);
      if (xInChunk == 0 && zInChunk == 0) AddLevelTo(cellX - 1, cellY, cellZ - 1, cellSize, yInChunk, cellSize, value);
      if (yInChunk == 0 && zInChunk == 0) AddLevelTo(cellX, cellY - 1, cellZ - 1, xInChunk, cellSize, cellSize, value);
      if (xInChunk == 0 && yInChunk == 0 && zInChunk == 0)
         AddLevelTo(cellX - 1, cellY - 1, cellZ - 1, cellSize, cellSize, cellSize, value);
      Debug.Log("Update " + targetChunks.Count);
      foreach (var chunk in targetChunks) {
         chunk.BuildMesh();
      }

      targetChunks.Clear();
   }

   private void AddLevelTo(int cellX, int cellY, int cellZ, int xInChunk, int yInChunk, int zInChunk, float value) {
      var chunkIndex = cellZ + cellY * gridSize + cellX * gridSize * gridSize;
      if (chunks.Count <= chunkIndex || chunkIndex < 0) return;
      var chunk = chunks[chunkIndex].GetComponent<Chunk>();
      var changed = chunk.SetLevel(xInChunk, yInChunk, zInChunk, value);
      if (changed) {
         targetChunks.Add(chunk);
      }
   }
}
