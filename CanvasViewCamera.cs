using UnityEngine;

namespace PixelCamera
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(Camera))]
	public class CanvasViewCamera : MonoBehaviour
    {
        Camera canvasCamera;
        public float Aspect => canvasCamera.aspect;
        [HideInInspector] public float Zoom = -100f; // 1f = full canvas size

        void OnEnable()
		{
            Initialize();
        }

        void Initialize()
        {
            canvasCamera = GetComponent<Camera>();
            if (canvasCamera == null)
            {
                Debug.LogError("Camera is required. Add the component!");
            }
            else if (canvasCamera.orthographic == false)
            {
                Debug.LogWarning("The pixel camera system only works in orthographic camera mode. Changing the view camera to orthographic!");
                canvasCamera.orthographic = true;
            }
        }
        public void AdjustSubPixelPosition(Vector2 targetViewPosition, Vector3 canvasLocalScale)
		{
            var canvasLocalScaleXY = new Vector2(canvasLocalScale.x, canvasLocalScale.y);

            var localPosition = (targetViewPosition - new Vector2(0.5f, 0.5f)) * canvasLocalScaleXY;

            transform.localPosition = new Vector3(localPosition.x, localPosition.y, -1f);
        }
        public float SetZoom(float inputZoom, float halfCanvasHeight)
        {
            float clampedZoom = Mathf.Approximately(inputZoom, 0f) ? 0.01f : Mathf.Clamp(inputZoom, -1, 1f);
            canvasCamera.orthographicSize = clampedZoom * halfCanvasHeight;
            Zoom = clampedZoom;

            return clampedZoom;
        }
        public void SetClipPlanes(float near, float far)
        {
            canvasCamera.nearClipPlane = near;
            canvasCamera.farClipPlane = far;
        }
    }
}