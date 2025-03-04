using UnityEngine;

namespace PixelCamera
{
    // Just a simple camera controller to try out isometric envionments and the features of the 3D Pixel Camera
    [RequireComponent(typeof(PixelCameraManager))]
    public class ExampleSimpleCameraController : MonoBehaviour
    {
        [Header("Moving")]
        [SerializeField] float moveSpeed = 10f;

        [Header("Scrolling")]
        [SerializeField] float viewScrollSpeed = 0.1f;
        [SerializeField] float gameScrollSpeed = 1f;

        [Header("Rotating")]
        [SerializeField] float dragSpeed = 0.5f;
        [SerializeField] float autoRotateSpeed = 30f;
        [SerializeField] float distanceToPivot = 30f;

        Transform target;

        PixelCameraManager pixelCameraManager;

        // Variables for autorotating
        float totalInput = 0;
        float autoRotationLeft = 0;
        float previousMouseX = 0;
        bool autoRotate = false;

        void Start()
        {
            pixelCameraManager = GetComponent<PixelCameraManager>();
            target = pixelCameraManager.FollowedTransform;
        }
        void Update()
        {
            ScrollZoom();
            MoveTarget();
            DragRotate();
        }

        void ScrollZoom()
        {
            float input = Input.mouseScrollDelta.y;
            if (Input.GetKey(KeyCode.LeftControl))
            {
                pixelCameraManager.GameCameraZoom -= input * gameScrollSpeed;
            }
            else
            {
                pixelCameraManager.ViewCameraZoom -= input * viewScrollSpeed;
            }
        }
        void MoveTarget()
        {
            var forward = new Vector3(target.forward.x, 0, target.forward.z).normalized;
            var right = new Vector3(target.right.x, 0, target.right.z).normalized;
            var cameraDirectionInput = Input.GetAxisRaw("Vertical") * forward + Input.GetAxisRaw("Horizontal") * right;
            target.position += moveSpeed * Time.deltaTime * cameraDirectionInput;
        }
        void DragRotate()
        {
            static float GetRotationLeft(float totalRotation, float input)
            {
                float yawGoal;
                if (input > 0)
                    yawGoal = Mathf.Ceil(totalRotation / 45) * 45;
                else
                    yawGoal = Mathf.Floor(totalRotation / 45) * 45;

                return yawGoal - totalRotation;
            }
            static float GetHorizontalMouseMovement(float previousPosition)
            {
                return Input.mousePosition.x - previousPosition;
            }
            if (Input.GetMouseButtonDown(1))
            {   // Stop rotating when new input starts
                autoRotate = false;
            }
            if (Input.GetMouseButton(1))
            {   // Get input and rotate around in respect to it
                var horizontalInput = GetHorizontalMouseMovement(previousMouseX);
                totalInput += horizontalInput * dragSpeed;

                var targetPosition = Input.GetKey(KeyCode.LeftControl) ? target.position + target.forward * distanceToPivot : target.position;
                target.RotateAround(targetPosition, Vector3.up, horizontalInput * dragSpeed);
            }
            if (Input.GetMouseButtonUp(1))
            {   // When input stops, get remaining rotation to the base angle in the direction of the input and set autorotation values
                autoRotationLeft = GetRotationLeft(totalInput - autoRotationLeft, totalInput);
                totalInput = 0;
                autoRotate = true;
            }
            if (autoRotationLeft == 0f)
            {
                autoRotate = false;
            }
            if (autoRotate)
            {   // Rotate automatically after input to a base angle
                var abs = Mathf.Abs(autoRotationLeft);
                var rotateNow = Mathf.Clamp(Time.deltaTime * autoRotateSpeed, 0, abs) * Mathf.Sign(autoRotationLeft);


                var targetPosition = Input.GetKey(KeyCode.LeftControl) ? target.position + target.forward * distanceToPivot : target.position;
                target.RotateAround(targetPosition, Vector3.up, rotateNow);

                autoRotationLeft -= rotateNow;
            }
            previousMouseX = Input.mousePosition.x;
        }
    }
}