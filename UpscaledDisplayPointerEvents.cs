#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace PixelCamera
{
    [ExecuteInEditMode]
    public class UpscaledDisplayPointerEvents : MonoBehaviour
    {
        PixelCameraManager pixelCameraManager;

        Quaternion defaultRotation = Quaternion.identity;
        Vector3 defaultPosition = new Vector3(0, 0, -1);

        void OnEnable()
        {
            Initialize();
        }

        void Initialize()
        {
#if UNITY_EDITOR
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                // Debug.Log("Not executing in prefab mode!");
                return;
            }
#endif
            if (pixelCameraManager == null)
            {
                pixelCameraManager = (PixelCameraManager)FindObjectOfType(typeof(PixelCameraManager));
                if (pixelCameraManager == null)
                {
                    Debug.LogError("pixelCameraManager is null. Set it in editor!");
                }
            }
            if (transform.parent != pixelCameraManager.transform)
            {
                Debug.LogWarning("The upscaled display parent is not the pixel camera. Changing it!");
                transform.parent = pixelCameraManager.transform;
            }
            if (transform.localPosition != defaultPosition)
            {
                Debug.LogWarning("The upscaled display has incorrect localPosition. Changing it!");
                transform.localPosition = defaultPosition;
            }
            if (transform.localRotation != Quaternion.identity)
            {
                Debug.LogWarning("The upscaled display has incorrect rotation. Changing it!");
                transform.localRotation = defaultRotation;
            }
        }
    }
}
