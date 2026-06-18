// NpcHealthBar.cs
using UnityEngine;
using UnityEngine.UI;

namespace GCNBC.ViewControllers.UI
{
    // World-space health bar that floats above the NPC, faces the camera,
    // and follows the NPC. Lives as a child of the NPC prefab.
    public class NpcHealthBar : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Slider _slider;
        [SerializeField] private Transform _target;      // NPC root to follow (usually parent)
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 3f, 0f);

        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
            if (_target == null && transform.parent != null)
                _target = transform.parent;

            if (_canvas != null && _canvas.renderMode == RenderMode.WorldSpace)
                _canvas.worldCamera = _camera;
        }

        // Updates the bar value (0..1). Called by the NPC when health changes.
        public void SetHealth(float current, float max)
        {
            float ratio = max > 0f ? current / max : 0f;
            if (_slider != null) _slider.value = ratio;
        }

        private void LateUpdate()
        {
            if (_target != null)
                transform.position = _target.position + _worldOffset;

            // Billboard: face the camera so the bar is always readable.
            if (_camera != null)
                transform.forward = _camera.transform.forward;
        }
    }
}