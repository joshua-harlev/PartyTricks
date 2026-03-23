using System;
using FMODUnity;
using UnityEngine;
using VineSwinging.Core;

namespace Minigames.Swinging {
    public class VineSwingingPlayerView : MonoBehaviour {
        [SerializeField] private SpriteRenderer spriteRenderer;
        private PlayerContext currentPlayerContext;
        [SerializeField] private EventReference grabEvent;
        [SerializeField] private EventReference launchEvent;
        [SerializeField] private EventReference fallEvent;
        [SerializeField] private EventReference collectCoinEvent;

        private bool isSnapping;
        private bool needsSweep;
        private const float SnapSpeed = 80f;
        
        public void Pull(PlayerContext playerContext) {
            currentPlayerContext = playerContext;
            Vector3 targetPosition = new Vector3(playerContext.PositionX, playerContext.PositionY);

            if (currentPlayerContext.PendingEvents.Contains(PlayerEvent.GrabbedVine)) {
                isSnapping = true;
                needsSweep = true;
            }

            if (needsSweep && Vector3.Distance(transform.localPosition, targetPosition) > 1f) {
                SweepCollectCoins(targetPosition);
                needsSweep = false;
            }
            
            foreach (var pendingEvent in currentPlayerContext.PendingEvents) {
                switch (pendingEvent) {
                    case PlayerEvent.GrabbedVine:
                        RuntimeManager.PlayOneShot(grabEvent, transform.position);
                        break;
                    case PlayerEvent.Fell:
                        isSnapping = false;
                        needsSweep = false;
                        RuntimeManager.PlayOneShot(fallEvent, transform.position);
                        break;
                    case PlayerEvent.Launched:
                        RuntimeManager.PlayOneShot(launchEvent, transform.position);
                        break;
                    case PlayerEvent.CollectedCoin:
                        RuntimeManager.PlayOneShot(collectCoinEvent, transform.position);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            currentPlayerContext.ClearEvents();

            if (isSnapping) {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, SnapSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.localPosition, targetPosition) < 0.05f) {
                    isSnapping = false;
                    transform.localPosition = targetPosition;
                }
            }
            else {
                transform.localPosition = targetPosition;
            }
            
            transform.localRotation = Quaternion.Euler(0f, 0f, currentPlayerContext.SwingAngle * Mathf.Rad2Deg);
            spriteRenderer.enabled = (currentPlayerContext.CurrentStateType != PlayerStateType.Falling);
        }

        public void CollectCoin(int value) {
            if (currentPlayerContext == null) return;
            currentPlayerContext.TotalCoinValue += value;
            currentPlayerContext.PendingEvents.Add(PlayerEvent.CollectedCoin);
        }

        public void SweepCollectCoins(Vector3 targetLocalPosition) {
            Vector2 fromWorldPosition = (Vector2)transform.position;
            Vector2 toWorldPosition;
            if (transform.parent != null) {
                toWorldPosition = transform.parent.TransformPoint(targetLocalPosition);
            }
            else {
                toWorldPosition = targetLocalPosition;
            }

            Vector2 direction = toWorldPosition - fromWorldPosition;
            float distance = direction.magnitude;
            if (distance < 0.01f) return;

            float sweepRadius = Mathf.Clamp(distance * 0.25f, 0.1f, 1.5f);
            
            var hits = Physics2D.CircleCastAll(fromWorldPosition, sweepRadius, direction.normalized, distance);
            foreach (var hit in hits) {
                var coin = hit.collider.GetComponent<SwingingCoinView>();
                coin?.ForceCollect(this);
            }
        }
    }
}