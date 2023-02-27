using System;
using Daredevil.Components;
using EntityStates;
using EntityStates.EngiTurret.EngiTurretWeapon;
using RoR2;
using UnityEngine;

namespace Daredevil.States
{
	public class SecondaryRanged : BaseSkillState
	{
		public static GameObject tracerEffectPrefab = Assets.revolverTracer;
		public static GameObject muzzleEffectPrefab = Assets.muzzleFlashRevolver;
		public static GameObject hitEffectPrefab = Assets.revolverHit;
		public static string abilityKey = "SecondaryRanged";

		public static float damageCoefficient = 2.7f;
		public static float procCoefficient = 1f;
		public static float baseDuration = 1.3f;
		public static float earlyExitTime = 0.7f;
		public static float force = 100f;
		public static float recoil = 3f;
		public static float range = 256f;

		private float duration;
		private float fireTime;
		private bool hasFired;
		private Animator animator;
		private string muzzleString = "MuzzleRevolver";
		private ComboController comboCounter;
		private WeaponController weaponController;

		public override void OnEnter()
		{
			base.OnEnter();
			this.comboCounter = GetComponent<ComboController>();
			this.weaponController = GetComponent<WeaponController>();
			this.weaponController.GunsOut();
			this.duration = baseDuration / this.attackSpeedStat;
			this.fireTime = 0.205f * this.duration;
			base.characterBody.SetAimTimer(2f);
			this.animator = GetModelAnimator();
			PlayAnimation("Gesture, Override", "ShootRevolver", "shootRevolver.playbackRate", duration);
		}

		public override void OnExit()
		{
			if (base.fixedAge < this.duration)
			{
				PlayCrossfade("Gesture, Override", "BufferEmpty", "shootRevolver.playbackRate", 0.2f, 0.2f);
			}
			base.OnExit();
		}

		private void Fire()
		{
			if (this.hasFired)
			{
				return;
			}
			hasFired = true;

			Util.PlayAttackSpeedSound("ShotPrimary", base.gameObject, attackSpeedStat);
			base.characterBody.AddSpreadBloom(1.5f);
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, muzzleString, false);
			if (base.isAuthority)
			{
				Ray aimRay = GetAimRay();
				AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);
				BulletAttack bulletAttack = new BulletAttack
				{
					bulletCount = 1,
					aimVector = aimRay.direction,
					origin = aimRay.origin,
					damage = damageCoefficient * damageStat,
					damageColorIndex = DamageColorIndex.Default,
					damageType = DamageType.Generic,
					falloffModel = BulletAttack.FalloffModel.DefaultBullet,
					maxDistance = range,
					force = force,
					hitMask = LayerIndex.CommonMasks.bullet,
					minSpread = 0f,
					maxSpread = 0f,
					isCrit = RollCrit(),
					owner = base.gameObject,
					muzzleName = muzzleString,
					smartCollision = true,
					procChainMask = default(ProcChainMask),
					procCoefficient = procCoefficient,
					radius = 0.75f,
					sniper = false,
					stopperMask = LayerIndex.CommonMasks.bullet,
					weapon = null,
					tracerEffectPrefab = tracerEffectPrefab,
					spreadPitchScale = 0f,
					spreadYawScale = 0f,
					queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
					hitEffectPrefab = hitEffectPrefab,
					hitCallback = delegate (BulletAttack bullet, ref BulletAttack.BulletHit hitInfo)
					{
						bool result = BulletAttack.defaultHitCallback(bullet, ref hitInfo);
						if (hitInfo.hitHurtBox && hitInfo.hitHurtBox.teamIndex != base.teamComponent.teamIndex)
						{
							this.comboCounter.AddCombo(1, abilityKey);
						}
						return result;
					}
				};
				bulletAttack.modifyOutgoingDamageCallback += RicochetUtils.BulletAttackShootableDamageCallback;
				bulletAttack.Fire();
			}


		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.fireTime)
			{
				Fire();
			}
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return (base.fixedAge <= duration * earlyExitTime) ? InterruptPriority.PrioritySkill : InterruptPriority.Any;
		}
	}
}


