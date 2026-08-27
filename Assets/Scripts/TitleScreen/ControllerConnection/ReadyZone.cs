using UnityEngine;

namespace Input.ControllerConnection {
    [RequireComponent(typeof(SpriteRenderer))]
    public class ReadyZone : MonoBehaviour {
        private SpriteRenderer spriteRenderer;

        private void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public bool Contains(Vector2 point) {
            Bounds zone = spriteRenderer.bounds;
            return point.x >= zone.min.x && point.x <= zone.max.x
                && point.y >= zone.min.y && point.y <= zone.max.y;
        }
    }
}