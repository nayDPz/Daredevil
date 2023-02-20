using Daredevil.Components;
using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using UnityEngine;

namespace Daredevil.States
{
	public class DeathSpin2Fire : BaseSkillState
	{
		private ComboController comboController;
		private WeaponController modelController;

		public static float speedCoefficient = 0.5f;
		public static float blastRadius = 11f;
		public static float damageCoefficient = 2.5f;
		public static float procCoefficient = 0.6f;
		public static float baseDuration = 1f;
		public static float force = 0f;
		public static float recoil = 3f;
		public static float range = 12f;

		public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");
		
		public bool crit;

		private string muzzleString;
		private float maxRange = 12f;
		private float roundDuration = 0.125f;
		private float roundStopwatch;
		private int numRounds = 6;
		private int roundsCompleted;
		private float duration;

		public override void OnEnter()
		{
			base.OnEnter();
			this.modelController = GetComponent<WeaponController>();
			this.modelController.GunsOut();

			this.comboController = GetComponent<ComboController>();
			this.comboController.drain = true;
			this.comboController.displayComboOverride = 6;

			base.characterBody.SetAimTimer(1.5f);
			
			this.roundStopwatch = this.roundDuration;
			float attackSpeed = (attackSpeedStat - 1f) * 0.5f;
			this.roundDuration /= attackSpeed + 1f;
			this.duration = numRounds * this.roundDuration;
			this.crit = RollCrit();
			PlayAnimation("FullBody, Override", "SpinnyShooty", "spinnyShooty.playbackRate", this.duration);
		}

		public override void OnExit()
		{
			base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
			this.comboController.displayComboOverride = 0;
			this.comboController.drain = false;
			base.OnExit();
		}

		private void Fire()
		{
			EffectManager.SimpleMuzzleFlash(FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
			Util.PlaySound((this.roundsCompleted % 2 == 1) ? "QuickShotLeft" : "QuickShotRight", base.gameObject);

			this.comboController.displayComboOverride = numRounds - roundsCompleted - 1;

			EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab, new EffectData
			{
				origin = base.transform.position,
				scale = blastRadius
			}, true);

			if (base.isAuthority)
			{
				BlastAttack blastAttack = new BlastAttack();
				blastAttack.attacker = base.gameObject;
				blastAttack.procChainMask = default(ProcChainMask);
				blastAttack.impactEffect = EffectIndex.Invalid;
				blastAttack.losType = BlastAttack.LoSType.NearestHit;
				blastAttack.damageColorIndex = DamageColorIndex.Default;
				blastAttack.damageType = DamageType.Generic;
				blastAttack.procCoefficient = procCoefficient;
				blastAttack.bonusForce = Vector3.zero;
				blastAttack.baseForce = force;
				blastAttack.baseDamage = damageCoefficient * damageStat;
				blastAttack.falloffModel = BlastAttack.FalloffModel.None;
				blastAttack.radius = maxRange;
				blastAttack.position = base.transform.position;
				blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
				blastAttack.teamIndex = GetTeam();
				blastAttack.inflictor = base.gameObject;
				blastAttack.crit = crit;
				blastAttack.Fire();
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.characterMotor)
			{
				base.characterMotor.velocity.y = 0f;
			}
			base.characterMotor.walkSpeedPenaltyCoefficient = speedCoefficient;
			base.characterDirection.forward = GetAimRay().direction;
			this.roundStopwatch += Time.fixedDeltaTime;
			if (this.roundStopwatch >= this.roundDuration)
			{
				Fire();
				this.roundsCompleted++;
				this.roundStopwatch = 0f;
			}
			if (this.roundsCompleted > this.numRounds || base.fixedAge > this.duration)
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


