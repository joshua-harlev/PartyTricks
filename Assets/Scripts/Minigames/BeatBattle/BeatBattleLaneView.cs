using System.Collections.Generic;
using Minigames.BeatBattle.Core;
using UnityEngine;

namespace Minigames.BeatBattle {
    public class BeatBattleLaneView : MonoBehaviour {
        [Header("Timings")] 
        [SerializeField] private float leadTimeInSeconds = 2f;
        
        [Header("Lanes")] 
        [SerializeField] private RectTransform laneA;
        [SerializeField] private RectTransform laneB;
        [SerializeField] private RectTransform hitLine;
        
        [Header("Note Prefab")]
        [SerializeField] private BeatBattleNoteView notePrefab;

        [Header("Colors")] 
        [SerializeField] private Color colorA = Color.red;
        [SerializeField] private Color colorB = Color.blue;

        private readonly List<NoteViewData> activeNotes = new();
        private float laneHeight;
        private float hitLineY;
        private float scrollSpeed;
        private bool scrolling;

        private struct NoteViewData {
            public BeatBattleNoteView View;
            public float TargetTimeInSeconds; // when to reach hit line
            public int NoteIndex;
        }

        private void Awake() {
            Canvas.ForceUpdateCanvases();
            laneHeight = laneA.rect.height;
            Vector3 hitLineWorldPosition = hitLine.position;
            hitLineY = laneA.InverseTransformPoint(hitLineWorldPosition).y;
        }

        public BeatBattleNoteView SpawnCreationNote(NoteType type, int gridSlotIndex, float gridSlotDuration, float creationDurationInSeconds) {
            var lane = (type == NoteType.A) ? laneA : laneB;
            var note = Instantiate(notePrefab, lane);
            note.SetColor((type == NoteType.A) ? colorA : colorB);

            float noteTime = gridSlotIndex * gridSlotDuration;
            float speed = laneHeight / creationDurationInSeconds;
            float y = hitLineY + (noteTime * speed);
            note.SetAnchoredY(y);
            
            activeNotes.Add(new NoteViewData
            {
                View = note,
                TargetTimeInSeconds = noteTime,
                NoteIndex = activeNotes.Count
            });
            
            return note;
        }

        public void BeginPlayback(BeatBattleChart chart, float playbackDurationInMs, float gridSlotDuration) {
            ClearNotes();
            scrollSpeed = laneHeight / leadTimeInSeconds;

            for (int i = 0; i < chart.Notes.Count; i++) {
                var chartNote = chart.Notes[i];
                var lane = (chartNote.Type == NoteType.A) ? laneA : laneB;
                var note = Instantiate(notePrefab, lane);
                note.SetColor((chartNote.Type == NoteType.A) ? colorA : colorB);

                float targetTimeInSeconds = chartNote.GridSlot * gridSlotDuration;
                
                float startY = hitLineY + (targetTimeInSeconds * scrollSpeed);
                note.SetAnchoredY(startY);
                
                activeNotes.Add(new NoteViewData
                {
                    View = note,
                    TargetTimeInSeconds = targetTimeInSeconds,
                    NoteIndex = i
                });
            }

            scrolling = true;
        }

        public void UpdateScroll(float elapsedTimeInSeconds) {
            if (!scrolling) return;
            for (int i = 0; i < activeNotes.Count; i++) {
                var note = activeNotes[i];
                if (note.View == null) continue;

                float y = hitLineY + (note.TargetTimeInSeconds - elapsedTimeInSeconds) * scrollSpeed;
                note.View.SetAnchoredY(y);
            }
        }

        public void OnNoteHit(int noteIndex) {
            for (int i = 0; i < activeNotes.Count; i++) {
                if (activeNotes[i].NoteIndex == noteIndex && activeNotes[i].View != null) {
                    activeNotes[i].View.PlayHitFeedback();
                    return;
                }
            }
        }

        public void OnNoteMissed(int noteIndex) {
            for (int i = 0; i < activeNotes.Count; i++) {
                if (activeNotes[i].NoteIndex == noteIndex && activeNotes[i].View != null) {
                    activeNotes[i].View.PlayMissFeedback();
                    return;
                }
            }
        }

        public void ClearNotes() {
            foreach (var note in activeNotes) {
                if (note.View != null) {
                    Destroy(note.View.gameObject);
                }
            }
            activeNotes.Clear();
            scrolling = false;
        }
    }
}