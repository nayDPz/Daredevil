using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace Daredevil.States
{
	public class ShotgunFireBomb : BaseSkillState
	{
		public static GameObject projectilePrefab = Assets.shotgunBombProjectile;
		public static float bloom = 0.5f;
		public static float recoilAmplitude = 7f;
		public static float baseDuration = 2f;
		public static float earlyExitTime = 0.5f;
		public float duration;
		public static float damageCoefficient = 6f;
		public static float force = 3000f;

		public override void OnEnter()
		{
			base.OnEnter();
			StartAimMode();
			duration = baseDuration / attackSpeedStat;
			Fire();
			PlayAnimation("Gesture, Override", "FireBigBig", "fireBigBig.playbackRate", duration);
			Util.PlaySound(Sounds.shotgunShoot, base.gameObject);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}

		private void Fire()
		{
			AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
			base.characterBody.AddSpreadBloom(bloom);
			Ray aimRay = GetAimRay();
			if (base.isAuthority)
			{
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.projectilePrefab = projectilePrefab;
				fireProjectileInfo.position = aimRay.origin;
				fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.damage = damageStat * damageCoefficient;
				fireProjectileInfo.force = force;
				fireProjectileInfo.crit = RollCrit();
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return base.fixedAge >= duration * earlyExitTime ? InterruptPriority.Any : InterruptPriority.PrioritySkill;

		}
	}

}

