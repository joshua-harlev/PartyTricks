using System.Collections.Generic;
using UnityEngine;

namespace Minigames.Swinging.Background {
    public class ParallaxPropSpawner : MonoBehaviour {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float minGap = 10f;
        [SerializeField] private float maxGap = 25f;
        [SerializeField] private float spawnAheadDistance = 20f;
        [SerializeField] private float cleanupBehindDistance = 10f;

        private float nextSpawnX;
        private readonly List<Transform> props = new();
        
        private void LateUpdate() {
            if (cameraTransform == null) return;

            float localCamX = cameraTransform.position.x - transform.parent.position.x;

            while (nextSpawnX < localCamX + spawnAheadDistance) {
                var prop = Instantiate(gameObject, transform.parent);
                Destroy(prop.GetComponent<ParallaxPropSpawner>());
                prop.transform.localPosition = new Vector3(nextSpawnX, transform.localPosition.y, transform.localPosition.z);
                props.Add(prop.transform);
                nextSpawnX += Random.Range(minGap, maxGap);
            }

            for (int i = props.Count - 1; i >= 0; i--) {
                if (props[i].localPosition.x < localCamX - cleanupBehindDistance) {
                    Destroy(props[i].gameObject);
                    props.RemoveAt(i);
                }
            }
        }
    }
}
