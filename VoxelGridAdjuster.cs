using UnityEngine;

namespace PixelCamera
{
    // Allows for full control of a transform while still locking an object to the camera grid.
    [ExecuteInEditMode]
    public class VoxelGridAdjuster : MonoBehaviour
    {
        public Transform FollowedTransform;
        PixelCameraManager pixelCameraManager;

        void OnEnable()
        {
            Initialize();
        }
        void Initialize()
        {
            if (pixelCameraManager == null)
            {
                pixelCameraManager = (PixelCameraManager)FindObjectOfType(typeof(PixelCameraManager));
                if (pixelCameraManager == null)
                {
                    Debug.LogError("pixelCameraManager is null. Set it in editor!");
                }
            }
        }

        void LateUpdate()
        {
            transform.position = pixelCameraManager.PositionToGrid(FollowedTransform.position);
        }
        /*
        void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(FollowedTransform.position, 0.1f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, FollowedTransform.position);
        }*/
    }
}