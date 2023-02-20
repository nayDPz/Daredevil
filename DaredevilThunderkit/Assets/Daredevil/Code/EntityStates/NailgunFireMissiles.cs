using EntityStates;
using RoR2;
using UnityEngine;

namespace Daredevil.States
{
	public class NailgunFireMissiles : BaseSkillState
	{
		public static float baseDuration = 0.5f;
		public static float exitDuration = 0.5f;
		private float fireTimer;
		private float fireInterval;
		private bool crit;
		private int missiles;
		public static int baseMissiles = 3;
		public static float missileDamageCoefficient = 1.5f;

		public override void OnEnter()
		{
			base.OnEnter();
			GetModelChildLocator();
			this.missiles = Mathf.FloorToInt(baseMissiles * this.attackSpeedStat);
			this.fireInterval = baseDuration / this.missiles;
			this.crit = RollCrit();
		}

		private void FireMissile()
		{
			if (base.isAuthority)
			{
				MissileUtils.FireMissile(base.transform.position + Vector3.up * 2f, base.characterBody, default(ProcChainMask), null,
					missileDamageCoefficient * this.damageStat, this.crit, GlobalEventManager.CommonAssets.missilePrefab,
					DamageColorIndex.Default, true);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.fireTimer -= Time.fixedDeltaTime;
			if (this.fireTimer <= 0f && base.fixedAge <= baseDuration)
			{
				this.fireTimer += this.fireInterval;
				FireMissile();
			}
			if (base.fixedAge >= baseDuration + exitDuration / this.attackSpeedStat && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}


