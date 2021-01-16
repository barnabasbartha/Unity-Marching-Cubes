using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour {
   public ComputeShader shader;
   public Material material;
   public int size = 1;

   private List<GameObject> chunks;

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
      chunkComponent.size = size;
      chunks.Add(chunkObject);
   }
}
