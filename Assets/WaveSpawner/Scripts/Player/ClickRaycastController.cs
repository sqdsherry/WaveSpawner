using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using WaveSpawner.Scripts.Enemies;

namespace WaveSpawner.Scripts.Player
{
    /// <summary>
    /// Handles player interaction via physical raycasting upon mouse clicks or touchscreen taps.
    /// Employs Unity's New Input System (<see cref="Pointer"/>) and targets <see cref="IDamageable"/> components.
    /// </summary>
    [DisallowMultipleComponent]
    public class ClickRaycastController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Camera used for raycasting. If null, Camera.main will be automatically cached.")]
        private Camera _camera;

        [SerializeField]
        [Tooltip("LayerMask used to filter raycast hits (defaults to Everything).")]
        private LayerMask _hitLayerMask = ~0;

        [SerializeField]
        [Tooltip("Maximum raycast distance.")]
        private float _maxRaycastDistance = 100f;

        /// <summary>
        /// Camera used for raycasting.
        /// </summary>
        public Camera Camera => _camera;

        /// <summary>
        /// LayerMask used for filtering hit colliders.
        /// </summary>
        public LayerMask HitLayerMask => _hitLayerMask;

        /// <summary>
        /// Maximum raycast range in world units.
        /// </summary>
        public float MaxRaycastDistance => _maxRaycastDistance;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void OnValidate()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void Update()
        {
            // 1. Check for pointer press (mouse left-button, touchscreen tap, or pen)
            if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
            {
                return;
            }

            // 2. Prevent click-through when clicking over UI elements
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // 3. Perform physics raycast
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    return;
                }
            }

            Vector2 screenPosition = Pointer.current.position.ReadValue();
            Ray ray = _camera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, _maxRaycastDistance, _hitLayerMask))
            {
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.Kill();
                }
            }
        }
    }
}
