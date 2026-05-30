using System;
using FMODUnity;
using Minigames.Swinging.Core.PlayerStateMachine;
using UnityEngine;

namespace Minigames.Swinging {
    public class VineSwingingPlayerView : MonoBehaviour {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer magnetOutline;
        [SerializeField] private ParticleSystem boostTrailParticles;
        private ParticleSystem.EmissionModule trailEmission;
        private PlayerContext currentPlayerContext;
        [SerializeField] private EventReference grabEvent;
        [SerializeField] private EventReference launchEvent;
        [SerializeField] private EventReference fallEvent;
        [SerializeField] private EventReference collectCoinEvent;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite releasedSprite;

        private bool isSnapping;
        private bool needsSweep;
        private bool showTrail;
        private float trailOffsetX;
        private bool hasMagnet;
        private bool hasGrabbedOnce;
        private const float SnapSpeed = 80f;
        private RaycastHit2D[] magnetHits;

        public void Initialize(bool showTrail, bool hasMagnet, int coinsPerGap) {
            this.showTrail = showTrail;
            if (showTrail) {
                trailEmission = boostTrailParticles.emission;
                trailEmission.enabled = false;
                boostTrailParticles.Play();
            }
            
            this.hasMagnet = hasMagnet;
            
            if (hasMagnet && magnetOutline != null) {
                magnetOutline.gameObject.SetActive(true);
            } else magnetOutline.gameObject.SetActive(false);
            
            trailOffsetX = boostTrailParticles.transform.localPosition.x;
            magnetHits = new RaycastHit2D[coinsPerGap*2];
        }
        
        public void Pull(PlayerContext playerContext) {
            currentPlayerContext = playerContext;

            if (currentPlayerContext.CurrentStateType == PlayerStateType.Swinging) {
                float swingDirection = Mathf.Cos(currentPlayerContext.SwingPhase);
                spriteRenderer.flipX = swingDirection < 0f;
            }
            else {
                spriteRenderer.flipX = currentPlayerContext.VelocityX < 0f;
            }

            if (showTrail) {
                var localPos = boostTrailParticles.transform.localPosition;
                localPos.x = spriteRenderer.flipX ? -trailOffsetX : trailOffsetX;
                boostTrailParticles.transform.localPosition = localPos;
            }
            
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
                        if (hasGrabbedOnce) { 
                            RuntimeManager.PlayOneShot(grabEvent, transform.position);
                        }
                        hasGrabbedOnce = true;
                        spriteRenderer.sprite = defaultSprite;
                        transform.localPosition =
                            new Vector3(currentPlayerContext.PositionX, currentPlayerContext.PositionY);
                        isSnapping = false;
                        break;
                    case PlayerEvent.Fell:
                        isSnapping = false;
                        needsSweep = false;
                        RuntimeManager.PlayOneShot(fallEvent, transform.position);
                        break;
                    case PlayerEvent.Launched:
                        RuntimeManager.PlayOneShot(launchEvent, transform.position);
                        spriteRenderer.sprite = releasedSprite;
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
            if (hasMagnet && magnetOutline != null) {
                magnetOutline.sprite = spriteRenderer.sprite;
                magnetOutline.flipX = spriteRenderer.flipX;
                magnetOutline.enabled = spriteRenderer.enabled;
            }

            if (showTrail) {
                bool notFalling = playerContext.CurrentStateType != PlayerStateType.Falling;
                trailEmission.enabled = notFalling;
            }
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
            
            int hitCount = Physics2D.CircleCast(origin: fromWorldPosition, radius: sweepRadius, direction: direction.normalized, contactFilter: ContactFilter2D.noFilter, results: magnetHits, distance: distance);
            for (var i = 0; i < hitCount; i++) {
                var hit = magnetHits[i];
                var coin = hit.collider.GetComponent<SwingingCoinView>();
                coin?.ForceCollect(this);
            }
        }
    }
}