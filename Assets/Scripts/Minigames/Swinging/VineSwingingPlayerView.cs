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
        
        public void Pull(PlayerContext playerContext) {
            currentPlayerContext = playerContext;
            transform.localPosition = new Vector3(currentPlayerContext.PositionX, currentPlayerContext.PositionY);
            transform.localRotation = Quaternion.Euler(0f, 0f, currentPlayerContext.SwingAngle * Mathf.Rad2Deg);
            spriteRenderer.enabled = (currentPlayerContext.CurrentStateType != PlayerStateType.Falling);
            foreach (var pendingEvent in currentPlayerContext.PendingEvents) {
                switch (pendingEvent) {
                    case PlayerEvent.GrabbedVine:
                        RuntimeManager.PlayOneShot(grabEvent, transform.position);
                        break;
                    case PlayerEvent.Fell:
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
        }

        public void CollectCoin(int value) {
            if (currentPlayerContext == null) return;
            currentPlayerContext.TotalCoinValue += value;
            currentPlayerContext.PendingEvents.Add(PlayerEvent.CollectedCoin);
        }
    }
}