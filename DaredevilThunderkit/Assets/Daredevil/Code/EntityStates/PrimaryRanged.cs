using System;
using Daredevil.Components;
using EntityStates;
using RoR2;
using UnityEngine;

namespace Daredevil.States
{
	public class PrimaryRanged : BaseSkillState
	{

		public GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerCaptainShotgun");
		public GameObject muzzleEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/MuzzleFlashes/MuzzleflashHuntress");
		public GameObject hitEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/HitEffect/HitsparkCaptainShotgun");
		public static string abilityKey = "PrimaryRanged";

		public static float damageCoefficient = 1.8f;
		public static float procCoefficient = 1f;
		public static float baseDuration = 0.8625f;
		public static float force = 0f;
		public static float recoil = 1f;
		public static float range = 128f;

		private ComboController comboCounter;
		private WeaponController controller;

		private float duration;
		private float fireTime;
		private bool hasFired;
		private Animator animator;
		private string muzzleString = "MuzzleLeft";

		public override void OnEnter()
		{
			base.OnEnter();
			this.comboCounter = base.GetComponent<ComboController>();
			this.controller = GetComponent<WeaponController>();
			this.controller.GunsOut();
			this.duration = baseDuration / this.attackSpeedStat;
			this.fireTime = 0.15f * this.duration;
			base.characterBody.SetAimTimer(2f);
			this.animator = GetModelAnimator();

			Animator modelAnimator = GetModelAnimator();
			if (modelAnimator)
			{
				string anim = (modelAnimator.GetBool("isMoving") ? "ShootPistolRun" : "ShootPistolIdle");
				base.PlayAnimation("LeftArm, Additive", anim);
			}
		}

		public override void OnExit()
		{
			if (!this.hasFired)
			{
				Fire();
			}
			base.OnExit();
		}

		private void Fire()
		{
			if (this.hasFired)
			{
				return;
			}

			this.hasFired = true;
			base.characterBody.AddSpreadBloom(1.5f);
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
			Util.PlaySound("ShotPrimary", base.gameObject);
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
					hitEffectPrefab = muzzleEffectPrefab,
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
			if (base.fixedAge >= fireTime)
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
			return InterruptPriority.Skill;
		}
	}
}


