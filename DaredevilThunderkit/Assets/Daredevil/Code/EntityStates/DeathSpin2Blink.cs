using Daredevil.Components;
using EntityStates;
using EntityStates.Huntress;
using RoR2;
using UnityEngine;

namespace Daredevil.States
{
	public class DeathSpin2Blink : BaseSkillState
	{
		public static float blinkRange = 30f;
		public static float rayRadius = 0.75f;

		private ComboController comboController;
		private Transform modelTransform;
		private CharacterModel characterModel;
		private HurtBoxGroup hurtboxGroup;

		private Vector3 worldBlinkVector;

		private EntityStateMachine weapon;

		
		private float windupDuration = 0.16f;
		private float blinkDuration = 0.09f;
		private float blinkSpeed;
		private float blinkStopwatch;

		public override void OnEnter()
		{
			base.OnEnter();
			this.comboController = GetComponent<ComboController>();
			this.comboController.drain = true;
			this.comboController.displayComboOverride = 6;
			this.comboController.ConsumeCombo();
			this.modelTransform = GetModelTransform();
			if (this.modelTransform)
			{
				this.characterModel = modelTransform.GetComponent<CharacterModel>();
				this.hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
			}
			if (this.characterModel)
			{
				this.characterModel.invisibilityCount = 1;
			}
			if (this.hurtboxGroup)
			{
				this.hurtboxGroup.hurtBoxesDeactivatorCounter = 1;
			}

			this.weapon = EntityStateMachine.FindByCustomName(base.gameObject, "Weapon");
			CreateBlinkEffect(base.transform.position);
			Util.PlaySound(Sounds.blink, base.gameObject);
			Vector3 target = FindBlinkTarget();

			this.worldBlinkVector = target - base.transform.position;
			this.worldBlinkVector.y += 1f;
			this.blinkSpeed = this.worldBlinkVector.magnitude / this.blinkDuration;

			if (this.modelTransform)
			{
				TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = 0.6f;
				temporaryOverlay.animateShaderAlpha = true;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = Assets.matHuntressFlashBright;
				temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
				TemporaryOverlay temporaryOverlay2 = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay2.duration = 0.7f;
				temporaryOverlay2.animateShaderAlpha = true;
				temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay2.destroyComponentOnEnd = true;
				temporaryOverlay2.originalMaterial = Assets.matHuntressFlashExpanded;
				temporaryOverlay2.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
			}
		}

		private Vector3 FindBlinkTarget()
		{
			if (this.comboController && this.comboController.areaIndicatorInstance)
			{
				return this.comboController.areaIndicatorInstance.transform.position;
			}
			Ray aimRay = GetAimRay();
			RaycastHit hitInfo = default(RaycastHit);
			bool flag = false;
			if (rayRadius > 0f && Util.CharacterSpherecast(base.gameObject, aimRay, rayRadius, out hitInfo, blinkRange, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal) && hitInfo.collider.GetComponent<HurtBox>())
			{
				flag = true;
			}
			if (!flag)
			{
				flag = Util.CharacterRaycast(base.gameObject, aimRay, out hitInfo, blinkRange, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
			}

			return flag ? hitInfo.point : aimRay.GetPoint(blinkRange);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.windupDuration)
			{
				this.blinkStopwatch += Time.fixedDeltaTime;
				if (base.characterMotor)
				{
					base.characterMotor.velocity = Vector3.zero;
					base.characterMotor.rootMotion += this.worldBlinkVector.normalized * this.blinkSpeed * Time.fixedDeltaTime;
				}
				if (base.isAuthority && this.blinkStopwatch >= this.blinkDuration)
				{
					this.outer.SetNextStateToMain();
					this.weapon.SetNextState(new DeathSpin2Fire
					{
						activatorSkillSlot = base.activatorSkillSlot
					});
				}
			}
		}

		private void CreateBlinkEffect(Vector3 origin)
		{
			EffectData effect = new EffectData();
			effect.rotation = Util.QuaternionSafeLookRotation(this.worldBlinkVector);
			effect.origin = origin;
			EffectManager.SpawnEffect(BaseBeginArrowBarrage.blinkPrefab, effect, false);
		}

		public override void OnExit()
		{
			Util.PlaySound(Sounds.blinkEnd, base.gameObject);
			if (this.modelTransform)
			{
				TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = 0.6f;
				temporaryOverlay.animateShaderAlpha = true;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = Assets.matHuntressFlashBright;
				temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
				TemporaryOverlay temporaryOverlay2 = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay2.duration = 0.7f;
				temporaryOverlay2.animateShaderAlpha = true;
				temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay2.destroyComponentOnEnd = true;
				temporaryOverlay2.originalMaterial = Assets.matHuntressFlashExpanded;
				temporaryOverlay2.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
			}
			if (this.characterModel)
			{
				this.characterModel.invisibilityCount = 0;
			}
			if (this.hurtboxGroup)
			{
				this.hurtboxGroup.hurtBoxesDeactivatorCounter = 0;
			}
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}


