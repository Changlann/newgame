using UnityEngine;

namespace PixelCamera
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(MeshRenderer))]
    public class UpscaledCanvas : MonoBehaviour
    {
        public string materialVariableName = "_LowResTexture";
        Material canvasMaterial;

        void OnEnable()
        {
            Initialize();
        }

        void Initialize()
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                Debug.LogError("MeshRenderer is required. Add the component!");
            }
            else
            {
                canvasMaterial = meshRenderer.sharedMaterial;
                if (canvasMaterial == null)
                {
                    Debug.LogError("canvasMaterial is null. Set it in MeshRenderer!");
                }
            }

            if (transform.parent.localScale != Vector3.one)
            {
                Debug.LogWarning("The Upscale Display localScale should be Vector3.one. Changing the local scale!");
                transform.parent.localScale = Vector3.one;
            }
        }
        public bool MaterialHasRenderTexture => canvasMaterial.HasProperty(materialVariableName);
        public void ResizeCanvas(Vector2Int targetTextureResolution, float aspectRatio, float canvasHeight)
        {
            Vector2 pixelViewSize = Vector2.one / targetTextureResolution;
            transform.localScale = new Vector3(canvasHeight * aspectRatio * (1f + pixelViewSize.x * 2f), canvasHeight * (1f + pixelViewSize.y * 2f), 1f);
        }
        public void SetCanvasRenderTexture(RenderTexture renderTexture)
        {
            canvasMaterial.SetTexture(materialVariableName, renderTexture);
        }
    }
}
