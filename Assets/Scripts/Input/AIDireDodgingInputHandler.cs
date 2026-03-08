using System.Collections.Generic;
using PlasticGui.Diff;
using Unity.Properties;
using UnityEngine;

namespace Input {
    public class AIDireDodgingInputHandler : AIInputHandlerBase {
        private Transform playerTransform;
        private Camera mainCamera;

        private const float EdgeThreshold = 0.08f;
        private const float FarEdgeThreshold = 0.92f;
        private const float WallRaycastDistance = 1f;

        private static readonly Vector2[] CardinalDirections =
            { Vector2.left, Vector2.right, Vector2.down, Vector2.up };

        public void SetPlayerContext(Transform player, Camera camera) {
            playerTransform = player;   
            mainCamera = camera;
        }
        
        protected override Vector2 GetRandomNavigationVector() {
            if (playerTransform == null || mainCamera == null) {
                return Vector2.zero;
            }

            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(playerTransform.position);
            Vector2 playerPosition = playerTransform.position;

            var validDirections = new List<Vector2>(4);

            for (int i = 0; i < CardinalDirections.Length; i++) {
                Vector2 direction = CardinalDirections[i];

                if (direction.x < 0 && viewportPosition.x < EdgeThreshold) continue;
                if (direction.x > 0 && viewportPosition.x > FarEdgeThreshold) continue;
                if (direction.y < 0 && viewportPosition.y < EdgeThreshold) continue;
                if (direction.y > 0 && viewportPosition.y > FarEdgeThreshold) continue;

                RaycastHit2D hit = Physics2D.Raycast(playerPosition, direction, WallRaycastDistance);
                if (hit.collider != null && hit.collider.CompareTag("Wall")) continue;
                
                validDirections.Add(direction);
            }

            if (validDirections.Count == 0) return Vector2.zero;
            
            return validDirections[Random.Range(0, validDirections.Count)];
        }
    }
}