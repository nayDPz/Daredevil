using EntityStates;
using RoR2;
using UnityEngine;

namespace Daredevil.States
{
	public class NailgunFire : BaseSkillState
	{

		public static int baseBulletCount = 2;
		public static float bulletMinSpread = 0.5f;
		public static float finalBulletMaxSpread = 2f;
		public static float bulletMaxSpread = 6f;
		public static float baseMaxFireInterval = 0.33f;
		public static float baseMinFireInterval = 0.05f;
		public static float baseWindupDuration = 1.25f;
		public static float bulletMaxDistance = 128f;
		public static float maxDamageCoefficientPerSecond = 12.5f;
		public static float maxProcCoefficientPerSecond = 3f;

		public GameObject muzzlePrefab = Assets.muzzleFlashNailgun;
		public GameObject tracerPrefab;
		public GameObject impactPrefab;

		public string muzzle;

		private float bulletSpread;
		private float fireInterval;
		private float windupDuration;
		private float baseBulletsPerSecond;
		private float baseFireRate;
		private float soundStopwatch;
		private float soundInterval;

		private Run.FixedTimeStamp lastCritCheck;
		private Run.FixedTimeStamp critEndTime;
		private float fireTimer;

		public override void OnEnter()
		{
			base.OnEnter();
			this.tracerPrefab = Assets.nailTracer;
			this.windupDuration = baseWindupDuration / this.attackSpeedStat;
			this.baseFireRate = 1f / baseMinFireInterval;
			this.baseBulletsPerSecond = baseBulletCount * this.baseFireRate;
			this.critEndTime = Run.FixedTimeStamp.negativeInfinity;
			this.lastCritCheck = Run.FixedTimeStamp.negativeInfinity;
			this.fireInterval = baseMaxFireInterval;
		}

		public override void OnExit()
		{
			base.OnExit();
			Util.PlaySound(Sounds.nailgunWinddown, base.gameObject);
		}

		private void UpdateCrits()
		{
			if (this.lastCritCheck.timeSince >= 1f)
			{
				this.lastCritCheck = Run.FixedTimeStamp.now;
				if (RollCrit())
				{
					this.critEndTime = Run.FixedTimeStamp.now + 1f;
				}
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			base.characterBody.outOfCombatStopwatch = 0f;

			this.bulletSpread = Mathf.Lerp(bulletMaxSpread, finalBulletMaxSpread, base.fixedAge / baseWindupDuration);
			this.fireInterval = Mathf.Lerp(baseMaxFireInterval, baseMinFireInterval, base.fixedAge / windupDuration);

			StartAimMode();
			this.soundInterval = this.fireInterval / baseBulletCount;
			this.soundStopwatch -= Time.fixedDeltaTime;
			if (this.soundStopwatch <= 0f)
			{
				Util.PlaySound(Sounds.nailgunShoot, base.gameObject);
				this.soundStopwatch += this.soundInterval / this.attackSpeedStat;
			}
			this.fireTimer -= Time.fixedDeltaTime;
			if (this.fireTimer <= 0f)
			{
				this.fireTimer += this.fireInterval / this.attackSpeedStat;
				FireBullets();
			}
			if (base.isAuthority && (!base.inputBank.skill1.down || base.characterBody.isSprinting))
			{
				this.outer.SetNextState(new NailgunSpinDown());
			}
		}

		private void FireBullets()
		{
			PlayCrossfade("Gesture, Override", "FireSmall", "fireSmall.playbackRate", baseMaxFireInterval, 0.025f);
			EffectManager.SimpleMuzzleFlash(muzzlePrefab, base.gameObject, "MuzzleBigGun", false);

			if (base.isAuthority)
			{
				

				UpdateCrits();

				bool crit = !critEndTime.hasPassed;
				float damage = maxDamageCoefficientPerSecond / baseBulletsPerSecond * damageStat;
				float force = 50f;
				float procCoefficient = maxProcCoefficientPerSecond / baseBulletsPerSecond;

				Ray aimRay = GetAimRay();
				BulletAttack bulletAttack = new BulletAttack();
				bulletAttack.bulletCount = (uint)baseBulletCount;
				bulletAttack.aimVector = aimRay.direction;
				bulletAttack.origin = aimRay.origin;
				bulletAttack.damage = damage;
				bulletAttack.damageColorIndex = DamageColorIndex.Default;
				bulletAttack.damageType = DamageType.Generic;
				bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
				bulletAttack.maxDistance = bulletMaxDistance;
				bulletAttack.force = force;
				bulletAttack.hitMask = LayerIndex.CommonMasks.bullet;
				bulletAttack.minSpread = bulletMinSpread;
				bulletAttack.maxSpread = bulletSpread;
				bulletAttack.isCrit = crit;
				bulletAttack.owner = base.gameObject;
				bulletAttack.muzzleName = muzzle;
				bulletAttack.smartCollision = false;
				bulletAttack.procChainMask = default(ProcChainMask);
				bulletAttack.procCoefficient = procCoefficient;
				bulletAttack.radius = 0.1f;
				bulletAttack.sniper = false;
				bulletAttack.stopperMask = LayerIndex.CommonMasks.bullet;
				bulletAttack.weapon = null;
				bulletAttack.tracerEffectPrefab = tracerPrefab;
				bulletAttack.spreadPitchScale = 1f;
				bulletAttack.spreadYawScale = 1f;
				bulletAttack.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
				bulletAttack.hitEffectPrefab = HealthComponent.AssetReferences.crowbarImpactEffectPrefab;
				bulletAttack.Fire();
			}
		}
	}
}


