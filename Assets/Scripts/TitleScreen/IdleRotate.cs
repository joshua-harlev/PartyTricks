using UnityEngine;

  public class IdleRotate : MonoBehaviour
  {
      [SerializeField] private RectTransform[] targets;
      [SerializeField] private float angle = 15f;
      [SerializeField] private float speed = 2f;

      private void Update()
      {
          float z = Mathf.Sin(Time.time * speed) * angle;
          Quaternion rotation = Quaternion.Euler(0, 0, z);
          foreach (var target in targets) {
              target.localRotation = rotation;
          }
      }
  }