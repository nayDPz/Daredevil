using System.Collections.Generic;
using Daredevil.Components;
using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.Huntress;
using R2API;
using RoR2;
using UnityEngine;

namespace Daredevil.States
{
	public class SecondaryLockOn : BaseSkillState // FUCK I DONT WANT TO REWRITE THIS
	{

		public static int maxTargets = 4;
		public static float damageCoefficient = 1.2f;
		public static float procCoefficient = 0.5f;
		public static float maxBaseDuration = 2.1f;
		public static float baseDuration = 0.6f;
		public static float recoil = 3f;

		public class LockOnTarget
		{
			public float timesShot;
			public HurtBox hurtBox;
			public float distanceBetween;
			public Indicator indicator;
			public CharacterMotor motor;
		}

		private StunTracker tracker;
		private WeaponController weaponController;
		private CameraTargetParams.AimRequest aimRequest;
		private List<LockOnTarget> targetList;
		private Vector3 direction;
		private int targetIndex;

		public GameObject tracerEffectPrefab = Assets.coinTracer;

		private float shootDuration;
		private float totalShots;
		private float shotsPerTarget = 5f;
		private float shotsFired;
		private float fireInterval;
		private float fireStopwatch;

		private Vector3 exitDirection;
		private bool crit;

		private Animator animator;
		private ComboController comboCounter;

		private bool addCombo = true;

		private bool windUpStage = true;
		private float windUpTime = 0.166f;
		private float blinkSpeed = 5f;
		private float blinkDistance = 2.4f;
		private float blinkDuration;
		private float exitForce = 80f;

		private Transform modelTransform;
		private CharacterModel characterModel;
		private HurtBoxGroup hurtboxGroup;

		private bool shootStage;
		private bool blinkStage;
		private float windUpStopwatch;
		private float blinkStopwatch;
		private Vector3 blinkVector;

		public override void OnEnter()
		{
			base.OnEnter();
			this.comboCounter = base.GetComponent<ComboController>();
			this.weaponController = base.GetComponent<WeaponController>();
			this.tracker = base.GetComponent<StunTracker>();
			this.targetList = new List<LockOnTarget>();
			this.modelTransform = GetModelTransform();

			this.windUpTime /= this.moveSpeedStat / 4f;
			this.blinkSpeed *= this.moveSpeedStat;
			float msMultiplier = (moveSpeedStat / base.characterBody.baseMoveSpeed - 1f) * 1.5f;
			this.blinkDistance *= 1f + msMultiplier;
			this.blinkDuration = this.blinkDistance / this.blinkSpeed;

			List<HurtBox> targets = tracker.GetTargetList();
			int targetCount = Mathf.Min(targets.Count, maxTargets);
			for (int i = 0; i < targetCount; i++)
			{
				HurtBox hurtBox = targets[i];
				HealthComponent healthComponent;
				if (hurtBox && (healthComponent = hurtBox.healthComponent))
				{
					healthComponent.body.AddTimedBuff(DaredevilContent.Buffs.stunMarkedCooldown, 3f);
					healthComponent.body.ClearTimedBuffs(DaredevilContent.Buffs.stunMarked);
					this.targetList.Add(new LockOnTarget
					{
						hurtBox = hurtBox,
						motor = healthComponent.body.characterMotor
					});
				}
			}

			this.shotsPerTarget *= this.attackSpeedStat;
			this.totalShots = this.shotsPerTarget * this.targetList.Count;

			this.shootDuration = Mathf.Lerp(baseDuration, maxBaseDuration, (targetCount - 1) / (maxTargets - 1));
			this.fireInterval = this.shootDuration / this.totalShots;
			this.animator = GetModelAnimator();

			EffectData effectData = new EffectData
			{
				rotation = Util.QuaternionSafeLookRotation(blinkVector),
				origin = Util.GetCorePosition(base.characterBody)
			};
			EffectManager.SpawnEffect(BlinkState.blinkPrefab, effectData, false);
			Util.PlaySound(Sounds.blink, base.gameObject);

			if (this.modelTransform)
			{
				this.animator = modelTransform.GetComponent<Animator>();
				this.characterModel = modelTransform.GetComponent<CharacterModel>();
				this.hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
			}
			if (base.cameraTargetParams)
			{
				this.aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.OverTheShoulder);
			}

			this.weaponController.GunsOut();
			this.animator.SetBool("lockOn", true);
			string anim = (base.isGrounded ? "LockOnGround" : "LockOnAir");
			base.PlayAnimation("FullBody, Override", anim);
			this.crit = RollCrit();
		}

		public override void OnExit()
		{
			if (this.aimRequest != null)
			{
				this.aimRequest.Dispose();
			}
			this.animator.SetBool("lockOn", false);
			foreach (LockOnTarget target in this.targetList)
			{
				if (target.motor)
				{
					target.motor.useGravity = true;
				}
			}
			if (this.blinkStopwatch <= this.blinkDuration)
			{
				if (this.characterModel)
				{
					this.characterModel.invisibilityCount--;
				}
				if (this.hurtboxGroup)
				{
					HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
					int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
					hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
				}
			}
			base.OnExit();
		}

		private void FireAt(HurtBox fireTarget)
		{

			if (fireTarget.healthComponent.body.characterMotor)
			{
				fireTarget.healthComponent.body.characterMotor.useGravity = false;
			}
			if (this.addCombo)
			{
				this.comboCounter.AddCombo(1, "ShootLockOn");
				this.addCombo = false;
			}

			bool left = shotsFired % 2f == 1f;
			string s = left ? "Left" : "Right";
			Util.PlaySound("QuickShot" + s, base.gameObject);
			
            base.PlayAnimation("Gesture, Additive", "QuickShot" + s);

			string muzzleName = left ? "MuzzlePistol" : "MuzzleRevolver";
			GameObject mf = (shotsFired % 2f == 1f) ? Assets.muzzleFlashPistol : Assets.muzzleFlashRevolver;
			EffectManager.SimpleMuzzleFlash(mf, base.gameObject, muzzleName, true);

			if (base.isAuthority)
			{

				Vector3 rOffset = UnityEngine.Random.insideUnitSphere * 1.5f;

				Vector3 position = fireTarget.transform.position + rOffset;
				Vector3 between = base.transform.position - position;

				
				base.characterDirection.forward = between * -1f;

				this.direction = base.characterDirection.forward;

				shotsFired += 1f;

				GameObject impact = shotsFired % 2 == 1 ? Assets.pistolHit : Assets.revolverHit;
				GameObject tracer = shotsFired % 2 == 1 ? Assets.pistolTracer : Assets.revolverTracer;

				EffectManager.SimpleImpactEffect(impact, position, between, true);

				if (this.tracerEffectPrefab)
				{
					EffectData effectData = new EffectData
					{
						origin = position,
						start = base.transform.position
					};
					EffectManager.SpawnEffect(tracer, effectData, true);
				}

				DamageInfo damageInfo = new DamageInfo
				{
					attacker = base.gameObject,
					damage = damageCoefficient * this.damageStat,
					procCoefficient = procCoefficient,
					position = position,
					damageColorIndex = DamageColorIndex.Default,
					damageType = DamageType.Generic,
					crit = this.crit
				};
				DamageAPI.AddModdedDamageType(damageInfo, DaredevilMain.knockupOnHit);

				fireTarget.healthComponent.TakeDamage(damageInfo);
				GlobalEventManager.instance.OnHitEnemy(damageInfo, fireTarget.healthComponent.gameObject);
				GlobalEventManager.instance.OnHitAll(damageInfo, fireTarget.healthComponent.gameObject);
				ForceFlinch(fireTarget.healthComponent.body);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.windUpStage)
			{
				this.windUpStopwatch += Time.fixedDeltaTime;
				if (this.modelTransform)
				{
					TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
					temporaryOverlay.duration = windUpTime;
					temporaryOverlay.animateShaderAlpha = true;
					temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					temporaryOverlay.destroyComponentOnEnd = true;
					temporaryOverlay.originalMaterial = Assets.matHuntressFlash;
					temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
				}
			}
			if (this.windUpStopwatch >= this.windUpTime)
			{
				if (base.inputBank.moveVector != Vector3.zero)
				{
					this.blinkVector = base.inputBank.moveVector.normalized;
					if (base.inputBank.jump.down)
					{
						this.blinkVector.y = 0.75f;
					}
					else
					{
						this.blinkVector.y = 0.2f;
					}
				}
				this.blinkStage = true;
				this.windUpStage = false;
				this.windUpStopwatch -= windUpTime;
				if (this.characterModel)
				{
					this.characterModel.invisibilityCount = 1;
				}
				if (this.hurtboxGroup)
				{
					this.hurtboxGroup.hurtBoxesDeactivatorCounter = 1;
				}
			}
			if (this.blinkStage)
			{
				this.blinkStopwatch += Time.fixedDeltaTime;
				base.characterMotor.velocity = Vector3.zero;
				base.characterMotor.rootMotion += this.blinkVector * this.blinkSpeed * Time.fixedDeltaTime;
				if (this.blinkStopwatch >= this.blinkDuration)
				{
					this.blinkStopwatch -= this.blinkDuration;
					this.blinkStage = false;
					this.shootStage = true;
					if (this.modelTransform)
					{
						TemporaryOverlay temporaryOverlay2 = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
						temporaryOverlay2.duration = 0.3f;
						temporaryOverlay2.animateShaderAlpha = true;
						temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
						temporaryOverlay2.destroyComponentOnEnd = true;
						temporaryOverlay2.originalMaterial = Resources.Load<Material>("Materials/matHuntressFlashBright");
						temporaryOverlay2.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
					}
					if (this.characterModel && this.characterModel.invisibilityCount > 0)
					{
						this.characterModel.invisibilityCount = 0;
					}
					if (this.hurtboxGroup)
					{
						this.hurtboxGroup.hurtBoxesDeactivatorCounter = 0;
					}
					Util.PlaySound(Sounds.blinkEnd, base.gameObject);
				}
			}
			if (this.shootStage)
			{
				base.characterMotor.velocity = Vector3.zero;
				
				this.fireStopwatch += Time.fixedDeltaTime;
				if (this.fireStopwatch >= this.fireInterval && this.shotsFired < this.totalShots && this.targetList.Count != 0)
				{
					this.fireStopwatch -= this.fireInterval;
					this.targetIndex = Random.Range(0, this.targetList.Count);
					HurtBox hurtBox = this.targetList[targetIndex].hurtBox;
					this.targetList[targetIndex].timesShot++;
					FireAt(hurtBox);
				}

				if(this.shotsFired > 0) //XDD
					base.characterDirection.forward = this.direction;

				if (this.fireStopwatch >= this.fireInterval * 2f)
				{
					this.outer.SetNextStateToMain();
				}
			}
			CleanTargetList();
			if ((base.isAuthority && this.shotsFired >= this.totalShots && this.fireStopwatch >= this.fireInterval)
				|| this.targetList.Count == 0)
			{
				this.exitForce *= this.moveSpeedStat;
				if (base.inputBank.moveVector != Vector3.zero)
				{
					this.exitDirection = base.inputBank.moveVector.normalized;
					this.exitDirection.y = (base.inputBank.jump.down ? 1f : 0.6f);
					base.characterMotor.ApplyForce(this.exitDirection * this.exitForce);
				}
				else
				{
					base.characterMotor.ApplyForce(Vector3.down * 25f);
				}
				this.outer.SetNextStateToMain();
			}
		}

		private void CleanTargetList()
		{
			for (int i = this.targetList.Count - 1; i >= 0; i--)
			{
				LockOnTarget target = this.targetList[i];
				if (!target.hurtBox.healthComponent.alive || target.timesShot >= this.shotsPerTarget)
				{
					targetList.RemoveAt(i);
					if (target.motor)
					{
						target.motor.useGravity = true;
					}
				}
			}
		}

		protected virtual void ForceFlinch(CharacterBody body)
		{
			SetStateOnHurt component = body.healthComponent.GetComponent<SetStateOnHurt>();
			if (component && component.canBeHitStunned)
			{
				component.SetPain();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			if (shootStage)
			{
				return InterruptPriority.PrioritySkill;
			}
			return InterruptPriority.Frozen;
		}
	}
}


