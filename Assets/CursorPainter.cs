using UnityEngine;

public class CursorPainter : MonoBehaviour {
   public event PaintDelegate OnPaintEvent;

   public delegate void PaintDelegate(Vector3 position, int size, float value);

   public Camera _camera;
   public float distance = 5;
   public int size = 1;
   [Range(-1f, 1f)] public float value = 1;
   private Transform _transform;
   private Transform cameraTransform;

   private void Start() {
      _transform = GetComponent<Transform>();
      cameraTransform = _camera.transform;
   }

   private void Update() {
      UpdatePosition();
      if (Input.GetMouseButton(0)) {
         Paint(value);
      }

      if (Input.GetMouseButton(1)) {
         Paint(-value);
      }
   }

   private void UpdatePosition() {
      _transform.position = cameraTransform.position + cameraTransform.forward * distance;
   }

   private void Paint(float v) {
      OnPaintEvent?.Invoke(_transform.position, size, v);
   }
}
