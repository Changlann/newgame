using UnityEngine;

namespace PixelCamera
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class PixelCameraManager : MonoBehaviour
    {
        [Tooltip(Tooltips.TT_FOLLOWED_TRANSFORM)] public Transform FollowedTransform; // Very useful for camera controllers. Allows for full control of a transform for example while using cinemachine
        [Header("Settings")]
        [Tooltip(Tooltips.TT_GRID_MOVEMENT)] public bool VoxelGridMovement = true;
        [Tooltip(Tooltips.TT_SUB_PIXEL)] public bool SubpixelAdjustments = true;
        [Tooltip(Tooltips.TT_FOLLOW_ROTATION)] public bool FollowRotation = true;

        [Header("Resolution")]
        [Tooltip(Tooltips.TT_RESOLUTION_SYNCHRONIZATION_MODE)] public ResolutionSynchronizationMode resolutionSynchronizationMode = ResolutionSynchronizationMode.SetHeight;
        public Vector2Int GameResolution = new Vector2Int(640, 360);

        [Header("Zoom")]
        [Tooltip(Tooltips.TT_CONTROL_GAME_ZOOM)] public bool ControlGameZoom = true;
        [Tooltip(Tooltips.TT_GAME_ZOOM)] public float GameCameraZoom = 5f;
        [Tooltip(Tooltips.TT_VIEW_ZOOM)][Range(-1f, 1f)] public float ViewCameraZoom = 1f;

        Camera gameCamera;
        CanvasViewCamera viewCamera;
        UpscaledCanvas upscaleCanvas;

        float renderTextureAspect;

        void OnEnable()
        {
            Initialize();
        }
        void Initialize()
        {
            if (gameCamera == null)
            {
                gameCamera = GetComponent<Camera>();
                if (gameCamera == null)
                {
                    Debug.LogError("Camera is null. Attach required component!");
                }
            }
            if (viewCamera == null)
            {
                viewCamera = (CanvasViewCamera)FindObjectOfType(typeof(CanvasViewCamera));
                if (viewCamera == null)
                {
                    Debug.LogError("viewCamera is null. Set it in editor!");
                }
            }
            if (upscaleCanvas == null)
            {
                upscaleCanvas = (UpscaledCanvas)FindObjectOfType(typeof(UpscaledCanvas));
                if(upscaleCanvas == null)
                {
                    Debug.LogError("upscaleCanvas is null. Set it in editor!");
                }
            }
            if (transform.parent == null)
            {
                transform.parent = (new GameObject("PixelCameraSystem")).transform;
            }
            if (transform.parent.childCount > 2)
            {
                Debug.LogWarning("Pixel Camera Managers parent should only have 2 child objects. This and a transform that it follows.");
            }
            if (FollowedTransform == null)
            {
                Debug.LogError("Followed Transform is null. Create a empty sibling object and set in editor.");
            }
            SynchronizeClipPlanes();
        }
        void LateUpdate()
        {
            // On aspect change
            var aspectRatioChanged = renderTextureAspect != viewCamera.Aspect;
            var pixelResolutionChanged = GameResolution != TargetTextureResolution();
            bool resizeCanvas = false;

            if (aspectRatioChanged || pixelResolutionChanged || gameCamera.targetTexture == null)
            {
                // Change fitting aspect ratio
                GameResolution = RenderTextureUtilities.TextureResultion(viewCamera.Aspect, GameResolution, resolutionSynchronizationMode);

                var newRenderTexture = RenderTextureUtilities.CreateRenderTexture(GameResolution, gameCamera.targetTexture);

                SetRenderTexture(viewCamera.Aspect, newRenderTexture, out resizeCanvas);
            }
            else if (Application.isEditor && upscaleCanvas.MaterialHasRenderTexture)
            {
                SetRenderTexture(renderTextureAspect, gameCamera.targetTexture, out resizeCanvas);
            }

            // Zooming
            bool orthographicSizeChanged = gameCamera.orthographicSize != GameCameraZoom;
            if (!ControlGameZoom)
            {
                GameCameraZoom = gameCamera.orthographicSize;
                resizeCanvas = true;
            }
            if (ControlGameZoom && orthographicSizeChanged)
            {
                GameCameraZoom = SetGameZoom(GameCameraZoom, out resizeCanvas);
            }
            if (orthographicSizeChanged || ViewCameraZoom != viewCamera.Zoom)
            {
                ViewCameraZoom = viewCamera.SetZoom(ViewCameraZoom, GameCameraZoom);
            }

            // Resize canvas if aspect changed or camera orthographic size changed
            if (resizeCanvas)
            {
                upscaleCanvas.ResizeCanvas(GameResolution, renderTextureAspect, GameCameraZoom * 2f);
            }

            // Transform updates
            if (FollowRotation)
            {
                transform.rotation = FollowedTransform.rotation;
            }
            if (VoxelGridMovement)
            {
                transform.position = PositionToGrid(FollowedTransform.position);
            }
            else
            {
                transform.position = FollowedTransform.position;
            }
            if (SubpixelAdjustments)
            {
                var targetViewPosition = gameCamera.WorldToViewportPoint(FollowedTransform.position);
                viewCamera.AdjustSubPixelPosition(targetViewPosition, upscaleCanvas.transform.localScale);
            }
        }
        public float PixelWorldSize => 2f * gameCamera.orthographicSize / gameCamera.pixelHeight; // How tall and wide a pixel on the camera is in world units
        public Vector3 PositionToGrid(Vector3 worldPosition)
        {
            var localPosition = transform.InverseTransformDirection(worldPosition);
            var localPositionInPixels = localPosition / PixelWorldSize;
            var integerMovement = (Vector3)Vector3Int.RoundToInt(localPositionInPixels);
            var movement = integerMovement * PixelWorldSize;
            return movement.x * transform.right
                 + movement.y * transform.up
                 + movement.z * transform.forward;
        }
        public float SetGameZoom(float zoom, out bool resizeCanvas) // returns the new size
        {
            // Orthographic cameras can not tolerate size = 0;
            var checkedZoom = Mathf.Approximately(zoom, 0f) ? 0.01f : zoom;
            gameCamera.orthographicSize = checkedZoom;
            resizeCanvas = true;
            return checkedZoom;
        }
        Vector2Int TargetTextureResolution()
        {
            return gameCamera.targetTexture == null ? new Vector2Int(-1, -1) : new Vector2Int(gameCamera.targetTexture.width, gameCamera.targetTexture.height);
        }
        void SetRenderTexture(float aspect, RenderTexture newRenderTexture, out bool resizeCanvas)
        {
            upscaleCanvas.SetCanvasRenderTexture(newRenderTexture);
            resizeCanvas = true;

            gameCamera.targetTexture = newRenderTexture;
            renderTextureAspect = aspect;
        }
        void SynchronizeClipPlanes()
        {
            viewCamera.SetClipPlanes(0f, -1 * viewCamera.transform.localPosition.z + gameCamera.farClipPlane);
        }
    }

    // Tooltips for Unity editor
    public static class Tooltips
    {
        public const string TT_FOLLOWED_TRANSFORM = "Transform that this camera follow while doing pixel perfect corrections. Very useful for camera controllers. Allows for full control of a transform.";

        public const string TT_GRID_MOVEMENT = "Stationary objects in 3D world look stationary without jittering colors or outlines. Camera moves on a voxel grid.";

        public const string TT_SUB_PIXEL = "Subpixel adjustments counter the blocky nature of moving along a grid.";

        public const string TT_FOLLOW_ROTATION = "Should the camera transform follow the followed transforms rotation as well as position.";

        public const string TT_GAME_RESOLUTION = "The resolution of the game render texture. Lower values look more pixelated.";

        public const string TT_RESOLUTION_SYNCHRONIZATION_MODE = "How 'GameResolution' should be calculated. Synchronization ensures consistent pixelation for different device resolutions and artstyles.";

        public const string TT_CONTROL_GAME_ZOOM = "Should this script control the game cameras orthographic size. This can be turned off if another controller is already controlling it to avoid unexpected behaviour.";

        public const string TT_GAME_ZOOM = "The resolution of the game appears to stay the same but everything becomes more detailed while zooming in. The game cameras orhtographic size.";

        public const string TT_VIEW_ZOOM = "Zoom where pixels stay a constant size and the view camera zooms in. The view cameras orhtographic size.";
    }
}
