using UnityEngine;
using UnityEngine.EventSystems;
using DCTC.Map;


namespace DCTC.Controllers {
    public class CameraAnimation {
        public float startTime;
        public Vector3 startPosition;
        public Vector3 destinationPosition;
        public float duration;
        public float remaining;
        public Quaternion destinationRotation;
    }


    public class ThreeDCameraController : MonoBehaviour {

        public float CameraLockPadding = 2;
        public Camera mainCamera;
        public float MinZoomDistance = 10;
        public float MaxZoomDistance = 200;
        public float AnimationDuration = 1.25f;
        public float KeyboardScrollSpeed = 200f;

        public delegate void CameraChangedEventHandler();
        public event CameraChangedEventHandler CameraChanged;

        public delegate void ClickEventHandler(Vector3 position);
        public event ClickEventHandler TileClicked;

        public bool SelectionEnabled { get; set; }
        public bool NavigationEnabled { get; set; }
        public bool ZoomEnabled { get; set; }

        Vector3 savedPosition = Vector3.zero;
        Quaternion savedRotation = Quaternion.identity;

        int ScrollMouseButton = 1;
        int SelectMouseButton = 0;


        Vector3 lastMousePosition = Vector3.zero;
        bool isDragging = false;
        bool ignoreMouse = false;
        Vector3 clickCoordinates = Vector3.zero;
        CameraAnimation currentAnimation = null;
        GameController gameController;

        private const int MinXRotation = 60;
        private const int MaxXRotation = 80;

        void Awake() {
            gameController = GameController.Get();
        }

        void Start () {
			if(mainCamera == null) {
				mainCamera = Camera.main;
			}

            SelectionEnabled = true;
            NavigationEnabled = true;
            ZoomEnabled = true;

        }

        void Update () {
            if (gameController.Map == null)
                return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                ignoreMouse = true;
            else 
                ignoreMouse = false;


			UpdateZoom();
			UpdatePosition();
            HandleSelection();
            RunAnimation();
            DrawDebug();
		}

        public void ResetToDefault() {
            Vector2 dimensions = MapDimensions;
            mainCamera.transform.position = new Vector3(0, MaxZoomDistance, 0);
            mainCamera.transform.rotation = Quaternion.Euler(MaxXRotation, 0, 0);
            MoveCamera(new Vector3(dimensions.x / 2, 0, dimensions.y / 2));
        }

        public Vector2 MapDimensions {
            get {
                // Each tile is two world coordinates wide and high
                float mapWidth = gameController.Map.Width * 2;
                float mapHeight = gameController.Map.Height * 2;
                return new Vector2(mapWidth, mapHeight);
            }
        }

        // Returns the world coordinates of the four corners of the visible area of the map
        public Vector3[] CalculateVisibleArea() {
            Vector3[] corners = new Vector3[4];
            corners[0] = ScreenPointToGroundPoint(new Vector3(0, 0));
            corners[1] = ScreenPointToGroundPoint(new Vector3(0, Screen.height));
            corners[2] = ScreenPointToGroundPoint(new Vector3(Screen.width, Screen.height));
            corners[3] = ScreenPointToGroundPoint(new Vector3(Screen.width, 0));
            return corners;
        }

        public void MoveCamera(Vector3 world) {
            Vector3 dest = WorldTargetToCameraLocation(world);
            MoveCameraRelative(dest - mainCamera.transform.position);
            UpdateCameraRotation();
            DispatchCameraChanged();
        }

        public void AnimateCamera(Vector3 world, float duration) {
            AnimateCamera(world, mainCamera.transform.rotation, duration);
        }

        public void AnimateCamera(Vector3 world, Quaternion rotation, float duration) {
            currentAnimation = new CameraAnimation();
            currentAnimation.destinationPosition = WorldTargetToCameraLocation(world);
            currentAnimation.duration = duration;
            currentAnimation.remaining = duration;
            currentAnimation.startPosition = mainCamera.transform.position;
            currentAnimation.startTime = Time.time;
            currentAnimation.destinationRotation = rotation;
        }

        public void AnimateCameraDirect(Vector3 world, Quaternion rotation, float duration) {
            AnimateCamera(world, rotation, duration);
            currentAnimation.destinationPosition = world;
        }

        public void FocusOnPosition(Vector3 world, Direction facing, float distance) {
            float rotateX = RotateXForDistance(distance);
            float radiansX = Mathf.PI / 2 - rotateX * Mathf.Deg2Rad;
            float distanceY = Mathf.Cos(radiansX) * distance;
            float distanceXZ = Mathf.Sin(radiansX) * distance;

            distance = -1 * distance;
            Vector3 dest = Vector3.zero;
            float rotateY = 0;

            switch(facing) {
                case Direction.None:
                case Direction.North:
                    dest = new Vector3(world.x, distanceY, world.z - distanceXZ);
                    rotateY = 0;
                    break;

                case Direction.South:
                    dest = new Vector3(world.x, distanceY, world.z + distanceXZ);
                    rotateY = 180;
                    break;

                case Direction.East:
                    dest = new Vector3(world.x - distanceXZ, distanceY, world.z);
                    rotateY = 90;
                    break;

                case Direction.West:
                    dest = new Vector3(world.x + distanceXZ, distanceY, world.z);
                    rotateY = 270;
                    break;
            }

            Quaternion rotation = Quaternion.Euler(rotateX, rotateY,
                mainCamera.transform.rotation.eulerAngles.z);

            AnimateCameraDirect(dest, rotation, AnimationDuration);
        }

        public void SaveCameraLocation() {
            savedRotation = mainCamera.transform.rotation;
            savedPosition = mainCamera.transform.position;
        }

        public void RestoreCameraLocation() {
            if (savedPosition != Vector3.zero) {
                AnimateCameraDirect(savedPosition, savedRotation, AnimationDuration);
                savedPosition = Vector3.zero;
                savedRotation = Quaternion.identity;
            }
        }

        public Vector3 MouseCursorInWorld() {
            return ScreenPointToGroundPoint(Input.mousePosition);
        }


        void UpdateZoom() {
			float zoom = ignoreMouse ? 0 : Input.GetAxis("Mouse ScrollWheel");
            Vector3 zoomDirection = mainCamera.transform.forward;

            if(zoom > 0) {
                zoomDirection = Camera.main.ScreenPointToRay(Input.mousePosition).direction;
            }

			if(Input.GetKey(KeyCode.PageUp) || Input.GetKey(KeyCode.Q)) {
				zoom += 1 * Time.deltaTime;
			}
			if(Input.GetKey(KeyCode.PageDown) || Input.GetKey(KeyCode.E)) {
				zoom -= 1 * Time.deltaTime;
			}

			if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
				zoom = zoom * 0.1f;
			}

            float distance = CameraDistanceFromGround();

            if (distance < MinZoomDistance && zoom > 0)
                return;

            if (distance > MaxZoomDistance && zoom < 0)
                return;

            if (zoom == 0)
                return;


            Vector3 zoomChange = zoomDirection * zoom * distance;
            mainCamera.transform.position = zoomChange + mainCamera.transform.position;


            UpdateCameraRotation();

            DispatchCameraChanged();

        }

        void UpdateCameraRotation() {
            float distance = CameraDistanceFromGround();
            float rotateX = RotateXForDistance(distance);

            mainCamera.transform.rotation = Quaternion.Euler(rotateX,
                mainCamera.transform.rotation.eulerAngles.y,
                mainCamera.transform.rotation.eulerAngles.z);
        }


        void UpdatePosition() {

            if (Input.GetMouseButtonDown(ScrollMouseButton) && !ignoreMouse && NavigationEnabled) {
                isDragging = true;
                lastMousePosition = ScreenPointToGroundPoint(Input.mousePosition);

                currentAnimation = null;    // cancel current animation 
            }

            if(isDragging) {
                Vector3 current = ScreenPointToGroundPoint(Input.mousePosition);
                MoveCameraRelative(lastMousePosition - current);
                lastMousePosition = ScreenPointToGroundPoint(Input.mousePosition);
            }

            // keyboard shortcuts
            if (isDragging == false) {
                Vector3 keyDiff = new Vector3(0, 0, 0);
                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
                    keyDiff.z += KeyboardScrollSpeed * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
                    keyDiff.z += -KeyboardScrollSpeed * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                    keyDiff.x += -KeyboardScrollSpeed * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                    keyDiff.x += KeyboardScrollSpeed * Time.deltaTime;
                }

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    keyDiff = keyDiff * 0.1f;
                }

                if (keyDiff != Vector3.zero) {
                    MoveCameraRelative(keyDiff);
                }
            }
        }

        void HandleSelection() {
            // releasing the middle mouse stops dragging
            if (Input.GetMouseButtonUp(ScrollMouseButton)) {
                isDragging = false;
                lastMousePosition = Vector3.zero;
            }

            if (Input.GetMouseButtonDown(SelectMouseButton)) {
                clickCoordinates = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(SelectMouseButton) && !ignoreMouse && SelectionEnabled) {
                Vector2 dist = Input.mousePosition - clickCoordinates;

                if (Mathf.Abs(dist.magnitude) < 0.25f ) {
                    if (TileClicked != null) {
                        TileClicked(ScreenPointToGroundPoint(Input.mousePosition));
                    }
                }
                
            }
        }

        void RunAnimation() {
            if(currentAnimation != null) {
                currentAnimation.remaining -= Time.deltaTime;

                if (currentAnimation.remaining <= 0f) {
                    currentAnimation = null;
                } else {
                    Vector3 delta = (currentAnimation.destinationPosition - currentAnimation.startPosition);
                    Vector3 dest = Easing.ExpoEaseOut(Time.time - currentAnimation.startTime, currentAnimation.startPosition,
                        delta, currentAnimation.duration);
                    mainCamera.transform.position = dest;

                    if (mainCamera.transform.rotation != currentAnimation.destinationRotation) {
                        Quaternion destRotation = Quaternion.Lerp(mainCamera.transform.rotation, currentAnimation.destinationRotation,
                            (currentAnimation.duration - currentAnimation.remaining) / currentAnimation.duration);
                        mainCamera.transform.rotation = destRotation;
                    }

                    DispatchCameraChanged();
                }
            }
        }

        void DrawDebug() {
            if (Input.GetKey(KeyCode.Space)) {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(ray.origin, mainCamera.transform.forward * CameraDistanceFromGround(), Color.yellow);

                Vector3[] corners = CalculateVisibleArea();
                Debug.DrawLine(corners[0], corners[1], Color.red);
                Debug.DrawLine(corners[1], corners[2], Color.red);
                Debug.DrawLine(corners[2], corners[3], Color.red);
                Debug.DrawLine(corners[3], corners[0], Color.red);
            }
        }

        void MoveCameraRelative(Vector3 diff) {
            Vector3[] corners = CalculateVisibleArea();
            Vector2 mapDimensions = MapDimensions;

            if (corners[0].x < -1 * CameraLockPadding)
                diff.x = Mathf.Max(0, diff.x);

            if (corners[3].x > mapDimensions.x + CameraLockPadding)
                diff.x = Mathf.Min(0, diff.x);

            if (corners[0].z < -1 * CameraLockPadding)
                diff.z = Mathf.Max(0, diff.z);

            if (corners[1].z > mapDimensions.y + CameraLockPadding)
                diff.z = Mathf.Min(0, diff.z);


            mainCamera.transform.Translate(diff, Space.World);

            DispatchCameraChanged();
        }

        protected void DispatchCameraChanged() {
            if (CameraChanged != null)
                CameraChanged();
        }



        // Returns the DISTANCE from the camera to the XZ plane
        float CameraDistanceFromGround() {
            Plane xz = new Plane(Vector3.up, Vector3.zero);
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            float enter = 0;
            xz.Raycast(ray, out enter);
            return enter;
        }


        // Returns the POINT at which the screen point ray intersects the XZ plane
        Vector3 ScreenPointToGroundPoint(Vector3 screenPoint) {
            Ray mouseRay = Camera.main.ScreenPointToRay(screenPoint);

            if (mouseRay.direction.y >= 0) {
                //Debug.LogError("Why is mouse pointing up?");
                return Vector3.zero;
            }

            return RayToGroundPoint(mouseRay);
        }

        // Returns the POINT at which the viewport point intersects the XZ plane
        Vector3 ViewportPointToGroundPoint(Vector3 viewportPoint) {
            return ScreenPointToGroundPoint(mainCamera.ViewportToScreenPoint(viewportPoint));
        }

        // Returns the POINT at which the camera at the specified position would intersect the XZ plane
        Vector3 WorldPointToGroundPoint(Vector3 point) {
            Ray ray = new Ray(point, mainCamera.transform.forward);
            return RayToGroundPoint(ray);
        }

        // Returns the desired camera location to focus on the specified world position, assuming the camera 
        // is facing north. 
        Vector3 WorldTargetToCameraLocation(Vector3 world) {
            float theta = (Mathf.PI / 2) - (mainCamera.transform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            float distanceZ = Mathf.Sin(theta) * CameraDistanceFromGround();
            return new Vector3(world.x, mainCamera.transform.position.y, world.z - distanceZ);
        }


        // Returns the POINT at which the specified ray intersects the XZ plane
        Vector3 RayToGroundPoint(Ray ray) {
            float rayLength = (ray.origin.y / ray.direction.y);
            return ray.origin - (ray.direction * rayLength);
        }

        float RotateXForDistance(float distance) {
            return Mathf.Lerp(MinXRotation, MaxXRotation, (distance - MinZoomDistance) / (MaxZoomDistance - MinZoomDistance));
        }


    }
}