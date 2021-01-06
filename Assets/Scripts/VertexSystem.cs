using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class VertexSystem : SystemBase {
   protected override void OnUpdate() {
      /*
      Entities.ForEach((ref Translation entity) => {
         var position = entity.Value;
         position = new float3 {
            x = position.x,
            y = position.y, // + 0.05f,
            z = position.z
         };
         entity.Value = position;
      }).Schedule();
      */
   }
}
