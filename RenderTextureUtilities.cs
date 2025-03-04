using UnityEngine;

namespace PixelCamera
{
    // How the game resolution is synchronized for different aspect ratios.
    public enum ResolutionSynchronizationMode
    {
        SetHeight,
        SetWidth,
        SetBoth
    }
    public static class RenderTextureUtilities
    {
        public static Vector2Int TextureResultion(float aspect, Vector2Int resolution, ResolutionSynchronizationMode resolutionSynchronizationMode)
        {
            switch (resolutionSynchronizationMode)
            {
                case ResolutionSynchronizationMode.SetHeight:
                    return new Vector2Int(Mathf.RoundToInt(resolution.y * aspect), resolution.y);
                case ResolutionSynchronizationMode.SetWidth:
                    return new Vector2Int(resolution.x, Mathf.RoundToInt(resolution.x / aspect));
                case ResolutionSynchronizationMode.SetBoth:
                    return new Vector2Int(resolution.x, resolution.y);
                default:
                    Debug.LogError("This case is not implemented.");
                    return Vector2Int.one;
            }
        }
        public static RenderTexture CreateRenderTexture(Vector2Int textureSize, RenderTexture toBeReleasedTexture = null)
        {
            if (toBeReleasedTexture != null)
            {
                toBeReleasedTexture.Release();
            }

            var newTexture = new RenderTexture(textureSize.x, textureSize.y, 32, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Point
            };

            newTexture.Create();

            return newTexture;
        }
    }
}
