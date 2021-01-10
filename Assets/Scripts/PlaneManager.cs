using System.Collections.Generic;
using UnityEngine;

public class PlaneManager : MonoBehaviour {
   public Material material;

   [Range(-100, 100)] public float noiseOffset;
   [Range(0.01f, 100f)] public float noiseScale = .01f;

   private List<GameObject> planes;
   private int updateIndex;

   private void Start() {
      planes = new List<GameObject>();
      int cellWorldSize = 10;
      int n = 3;
      for (int x = 0; x < n; x++) {
         for (int y = 0; y < n; y++) {
            for (int z = 0; z < n; z++) {
               var plane = new GameObject();
               plane.AddComponent<Plane>().material = material;
               plane.GetComponent<Transform>().position = new Vector3(
                  x * cellWorldSize,
                  y * cellWorldSize,
                  z * cellWorldSize
               );
               planes.Add(plane);
            }
         }
      }
   }

   private void Update() {
      planes[updateIndex].GetComponent<Plane>().UpdateMesh(noiseOffset, noiseScale);
      updateIndex = (updateIndex + 1) % planes.Count;
   }
}
