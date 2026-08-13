using UnityEngine;

namespace Minigames.Swinging {
    public class VineSwingingMagnets : MonoBehaviour {
        [SerializeField] private VineSwingingPlayers players;
        
        private const int MaxMagnetHits = 16;
        private readonly Collider2D[] magnetHits = new Collider2D[MaxMagnetHits];
        private readonly ContactFilter2D magnetFilter = new ContactFilter2D
            { useTriggers = true, useLayerMask = false };
        
        public void DoTick() {
            for (int i = 0; i < players.PlayerStateMachines.Length; i++) {
                if (players.PlayerHasMagnet[i]) {
                    Vector2 center = players.PlayerViews[i].transform.position;
                    int hitCount = Physics2D.OverlapCircle(center, players.PlayerMagnetRadii[i], magnetFilter, magnetHits);
                    for (var hitIndex = 0; hitIndex < hitCount; hitIndex++) {
                        var collision = magnetHits[hitIndex];
                        var coin = collision.GetComponent<SwingingCoinView>();
                        coin?.StartPull(players.PlayerViews[i].transform,
                            players.PlayerMagnetPullSpeed[i]);
                    }
                }
            }
        }
    }
}