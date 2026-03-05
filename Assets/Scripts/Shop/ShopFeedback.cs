using FMODUnity;
using UnityEngine;

namespace Shop {
    public class ShopFeedback : MonoBehaviour {
        [SerializeField] private ShopTimerDisplay timerDisplay;
        [SerializeField] private EventReference lockSound;
        [SerializeField] private EventReference unlockSound;
        [SerializeField] private EventReference allReadySound;
        [SerializeField] private EventReference timeUpSound;
        [SerializeField] private EventReference cannotAffordSound;

        private bool allHumansWereLocked = false;
        private int previousLockedCount = 0;
        private bool timerEnded = false;

        public void UpdateFeedback(bool allPlayersLocked, int lockedCount) {
            if (timerEnded) return;
            if(previousLockedCount > lockedCount) {
                PlaySound(unlockSound);
                if(allHumansWereLocked) timerDisplay.HideAllReady();
            }
            else if (!allHumansWereLocked) {
                if (allPlayersLocked) {
                    PlaySound(allReadySound);
                    timerDisplay.ShowAllReady();
                } else if (lockedCount > previousLockedCount) {
                    PlaySound(lockSound);
                }
            }
            previousLockedCount = lockedCount;
            allHumansWereLocked = allPlayersLocked;
        }

        private void PlaySound(EventReference sound) {
            if (!sound.IsNull) {
                RuntimeManager.PlayOneShot(sound);
            }
        }

        public void Reset() {
            allHumansWereLocked = false;
            timerEnded = false;
            previousLockedCount = 0;
            timerDisplay.HideAllReady();
        }

        public void OnTimerEnd() {
            timerEnded = true;
            PlaySound(timeUpSound);
            timerDisplay.HideAllReady();
        }

        public void PlayCannotAffordSound() {
            PlaySound(cannotAffordSound);
        }
    }
}