using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Minigames.DireDodging {
    public class DireDodgingProjectilePool : MonoBehaviour {
        [SerializeField] private Color PlayerColor;
        [SerializeField] private GameObject ProjectilePrefab;
        [SerializeField] private Transform PoolParent;
        
        private ObjectPool<DireDodgingProjectile>[] pools;
        private List<DireDodgingProjectile> activeProjectiles = new();

        public void Initialize() {
            pools = new ObjectPool<DireDodgingProjectile>[2];
            pools[0] = new ObjectPool<DireDodgingProjectile>(
                () => CreateProjectile(pools[0]),
                OnGetProjectile,
                OnReleaseProjectile,
                OnDestroyProjectile
            );
            pools[1] = new ObjectPool<DireDodgingProjectile>(
                () => CreateProjectile(pools[1]),
                OnGetProjectile,
                OnReleaseProjectile,
                OnDestroyProjectile
            );
        }

        public DireDodgingProjectile GetNormal() => pools[0].Get();
        public DireDodgingProjectile GetCharged() => pools[1].Get();

        public void ReturnAllToPool() {
            foreach (var projectile in activeProjectiles.ToArray()) {
                projectile.ReturnToPool();
            }
        }
        
        public void DestroyAllVisible() {
            var projectilesToDestroy = new List<DireDodgingProjectile>(activeProjectiles);
            foreach (var projectile in projectilesToDestroy) {
                Destroy(projectile.gameObject);
            }
        }
        
        private DireDodgingProjectile CreateProjectile(IObjectPool<DireDodgingProjectile> projectilePool) {
            GameObject projectileObject = Instantiate(ProjectilePrefab, PoolParent);
            projectileObject.SetActive(false);
            DireDodgingProjectile projectile = projectileObject.GetComponent<DireDodgingProjectile>();
            projectile.SetPool(projectilePool);
            projectile.SetColor(PlayerColor);
            return projectile;
        }
        
        private void OnGetProjectile(DireDodgingProjectile projectile) {
            projectile.gameObject.SetActive(true);
            activeProjectiles.Add(projectile);
        }

        private void OnReleaseProjectile(DireDodgingProjectile projectile) {
            projectile.gameObject.SetActive(false);
            activeProjectiles.Remove(projectile);
        }

        private void OnDestroyProjectile(DireDodgingProjectile projectile) {
            Destroy(projectile.gameObject);
        }
    }
}