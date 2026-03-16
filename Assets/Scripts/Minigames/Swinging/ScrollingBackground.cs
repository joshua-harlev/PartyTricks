using System.Collections.Generic;
using UnityEngine;

namespace Minigames.Swinging {
    public class ScrollingBackground : MonoBehaviour {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private int tilesAhead = 3;

        private float tileWidth;
        private readonly List<Transform> tiles = new();
        private int nextTileIndex;
        private Vector3 localPos;

        private void Start() {
            var sr = GetComponent<SpriteRenderer>();
            tileWidth = sr.bounds.size.x;
            localPos = transform.localPosition;
            nextTileIndex = Mathf.FloorToInt(transform.position.x / tileWidth) + 1;

            tiles.Add(transform);
        }

        private void LateUpdate() {
            if (cameraTransform == null) return;

            float camX = cameraTransform.position.x;

            float rightEdge = camX + tileWidth * tilesAhead;
            while (nextTileIndex * tileWidth < rightEdge) {
                SpawnTile(nextTileIndex);
                nextTileIndex++;
            }

            for (int i = tiles.Count - 1; i >= 0; i--) {
                if (tiles[i] == transform) continue; // don't destroy the original
                if (tiles[i].position.x + tileWidth < camX - tileWidth) {
                    Destroy(tiles[i].gameObject);
                    tiles.RemoveAt(i);
                }
            }
        }

        private void SpawnTile(int index) {
            var tile = Instantiate(gameObject, transform.parent).transform;
            Destroy(tile.GetComponent<ScrollingBackground>()); // only one needs the script
            tile.localPosition = new Vector3(index * tileWidth, localPos.y, localPos.z);
            tiles.Add(tile);
        }
    }
}
