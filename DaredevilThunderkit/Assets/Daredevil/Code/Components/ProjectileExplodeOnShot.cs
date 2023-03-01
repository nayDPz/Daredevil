using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace Daredevil.Components
{
	public class ProjectileExplodeOnShot : MonoBehaviour, IShootable, IOnIncomingDamageServerReceiver
	{
		public ProjectileImpactExplosion projectileImpactExplosion;
		public RicochetUtils.RicochetPriority priority = RicochetUtils.RicochetPriority.Explosive;
		public GameObject bigExplosionPrefab;


		// fuuuuucking lol
		public void OnIncomingDamageServer(DamageInfo damageInfo)
		{
			damageInfo.rejected = true;
		}

		private void Awake()
		{
			if (!this.projectileImpactExplosion) this.projectileImpactExplosion = base.GetComponent<ProjectileImpactExplosion>();

		}

		public bool CanBeShot()
		{
			return true;
		}

		public RicochetUtils.RicochetPriority GetRicochetPriority()
		{
			return this.priority;
		}

		public void OnShot(DamageInfo damageInfo)
		{
			float damageCoefficient = 1f;
			if (damageInfo.attacker)
			{
				CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
				if (body)
				{
					damageCoefficient = damageInfo.damage / body.damage;
				}
			}
			if (this.projectileImpactExplosion)
			{
				this.projectileImpactExplosion.blastRadius *= 1.5f;
				this.projectileImpactExplosion.falloffModel = BlastAttack.FalloffModel.None; ///
				this.projectileImpactExplosion.blastDamageCoefficient *= damageCoefficient;
				this.projectileImpactExplosion.impactEffect = this.bigExplosionPrefab;
				this.projectileImpactExplosion.SetAlive(false);
			}
		}

        
    }

}

