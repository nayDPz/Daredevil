using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace Daredevil.States
{
	public class RailgunFireTazer : BaseSkillState
	{
		public static GameObject projectilePrefab = Assets.railgunTazerProjectileREPLACETHISEVENTUALLY;
		public static string soundString = "Play_captain_m2_tazer_shoot";
		public static float bloom = 0.3f;
		public static float recoilAmplitude = 4f;
		public static float baseDuration = 1.5f;
		public static float earlyExitTime = 0.5f;

		public static float damageCoefficient = 2f;
		public static float force = 600f;

		public float duration;
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = baseDuration / this.attackSpeedStat;
			FireTazer();
			base.PlayAnimation("Gesture, Override", "FireBig");
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
			}
		}

		private void FireTazer()
		{
			Util.PlaySound(soundString, base.gameObject);
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
			return (base.fixedAge >= this.duration * earlyExitTime) ? InterruptPriority.Any : InterruptPriority.PrioritySkill;
		}
	}
}


