using System;
using DG.Tweening;
using UnityEngine;

namespace Minigames.DireDodging {
    public static class DireDodgingCameraZoomService {
        private static Camera camera;
        private static float originalSize;
        private static Vector3 originalPosition;

        private static bool shockwaveZoomActive;
        private static float shockwaveTargetSize;
        private static Vector3 shockwaveTargetPosition;

        private static bool deathZoomActive;

        private static bool pendingReverseToOriginal;

        private static float shockwaveZoomDuration;
        private static Vector3 shockwaveInterruptPosition;
        private static float shockwaveInterruptSize;
        private static float shockwaveRemainingDuration;
        private static float shockwaveLerpAmount;
        private static float shockwaveZoomStartTime;
        
        public static bool DeathZoomActive => deathZoomActive;
        public static bool ShockwaveZoomActive => shockwaveZoomActive;
        public static Action<bool> OnShockwaveZoomStatusChange;

        public static void Initialize(Camera sceneCamera) {
            camera = sceneCamera;
            originalSize = camera.orthographicSize;
            originalPosition = camera.transform.position;
            shockwaveZoomActive = false;
            deathZoomActive = false;
            pendingReverseToOriginal = false;
            shockwaveRemainingDuration = 0f;
        }
        
        #region Shockwave

        public static void StartShockwaveZoom(Vector3 playerPosition, float zoomPercentage, float lerpAmount,
            float duration) {
            if (deathZoomActive) return;
            OnShockwaveZoomStatusChange?.Invoke(true);
            shockwaveZoomActive = true;
            shockwaveZoomDuration = duration;
            shockwaveLerpAmount = lerpAmount;
            shockwaveZoomStartTime = Time.time;
            shockwaveTargetSize = originalSize * zoomPercentage;
            shockwaveTargetPosition = Vector3.Lerp(originalPosition,
                new Vector3(playerPosition.x, playerPosition.y, originalPosition.z), lerpAmount);
            
            camera.DOOrthoSize(shockwaveTargetSize, duration).SetEase(Ease.InOutQuad);
        }

        public static void ReverseShockwaveZoom(float durationInSeconds = 0.15f) {
            if (!shockwaveZoomActive) return;
            shockwaveZoomActive = false;

            if (deathZoomActive) {
                pendingReverseToOriginal = true;
                return;
            }

            AnimateToOriginal(durationInSeconds);
        }

        private static void AnimateToOriginal(float durationInSeconds) {
            KillCameraTweens();
            camera.DOOrthoSize(originalSize, durationInSeconds).SetEase(Ease.InOutQuad);
            camera.transform.DOMove(originalPosition, durationInSeconds).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                OnShockwaveZoomStatusChange?.Invoke(false);
            });
        }

        public static void UpdateShockwaveZoomPosition(Vector3 playerPosition) {
            if (!shockwaveZoomActive || deathZoomActive) return;
            
            float elapsed = Time.time - shockwaveZoomStartTime;
            float progress = Mathf.Clamp01(elapsed / shockwaveZoomDuration);
            float easedProgress = DOVirtual.EasedValue(0f, 1f, progress, Ease.InOutQuad);
            float effectiveLerpValue = shockwaveLerpAmount * easedProgress;
            
            Vector3 playerPos = new Vector3(playerPosition.x, playerPosition.y, originalPosition.z);
            shockwaveTargetPosition = Vector3.Lerp(originalPosition, playerPos, effectiveLerpValue);
            camera.transform.position = shockwaveTargetPosition;
        }
        
        #endregion
        
        #region Death

        public static void StartDeathZoom(Vector3 deathPosition, float zoomAmount, float durationInSeconds) {
            deathZoomActive = true;
            KillCameraTweens();

            if (shockwaveZoomActive) {
                shockwaveInterruptPosition = camera.transform.position;
                shockwaveInterruptSize = camera.orthographicSize;
                float progress = 1f;
                if (Mathf.Abs(originalSize - shockwaveTargetSize) > 0.001f) {
                    progress = (originalSize - shockwaveInterruptSize) / (originalSize - shockwaveTargetSize);
                }
                shockwaveRemainingDuration = shockwaveZoomDuration * (1f-Mathf.Clamp01(progress));
            }

            float baseSize = originalSize;
            if(shockwaveZoomActive) baseSize = shockwaveTargetSize;
            Vector3 targetPosition = new Vector3(deathPosition.x, deathPosition.y, originalPosition.z);

            camera.DOOrthoSize(baseSize * zoomAmount, durationInSeconds).SetUpdate(true);
            camera.transform.DOMove(targetPosition, durationInSeconds).SetUpdate(true);
        }

        public static void ReturnFromDeathZoom(float duration = 0.3f) {
            float returnSize = originalSize;
            Vector3 returnPosition = originalPosition;

            if (shockwaveZoomActive) {
                returnSize = shockwaveInterruptSize;
                returnPosition = shockwaveInterruptPosition;
            }
            
            camera.DOOrthoSize(returnSize, duration).SetUpdate(true);
            camera.transform.DOMove(returnPosition, duration).SetUpdate(true).OnComplete(() =>
            {
                deathZoomActive = false;

                if (shockwaveZoomActive && shockwaveRemainingDuration > 0.01f) {
                    camera.DOOrthoSize(shockwaveTargetSize, shockwaveRemainingDuration).SetEase(Ease.InOutQuad);
                }

                if (pendingReverseToOriginal) {
                    pendingReverseToOriginal = false;
                    AnimateToOriginal(0.15f);
                }
            });
        }

        public static void CancelDeathZoom(float returnDurationInSeconds = 0.3f) {
            if (!deathZoomActive) return;
            KillCameraTweens();

            float returnSize;
            Vector3 returnPosition;

            if (pendingReverseToOriginal || !shockwaveZoomActive) {
                returnSize = originalSize;
                returnPosition = originalPosition;
            }
            else {
                returnSize = shockwaveTargetSize;
                returnPosition = shockwaveTargetPosition;
            }

            pendingReverseToOriginal = false;
            
            camera.DOOrthoSize(returnSize, returnDurationInSeconds).SetUpdate(true);
            camera.transform.DOMove(returnPosition, returnDurationInSeconds).SetUpdate(true).OnComplete(() =>
            {
                deathZoomActive = false;
            });
        }
        #endregion

        private static void KillCameraTweens() {
            camera.DOKill();
            camera.transform.DOKill();
        }

        public static void Cleanup() {
            if(camera != null) KillCameraTweens();
            shockwaveZoomActive = false;
            deathZoomActive = false;
            pendingReverseToOriginal = false;
        }
    }
}