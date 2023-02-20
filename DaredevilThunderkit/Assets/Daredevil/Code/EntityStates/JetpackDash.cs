using System.Collections.Generic;
using System.Linq;
using Daredevil.Components;
using EntityStates;
using EntityStates.Commando;
using RoR2;
using UnityEngine;

namespace Daredevil.States
{
	public class JetpackDash : BaseSkillState
	{
		public static float duration = 0.5f;
		public static float baseHitPauseDuration = 0.3f;
		public static float minHitPauseDuration = 0.1f;
		public static float initialSpeedCoefficient = 11f;
		public static float finalSpeedCoefficient = 0f;

		private bool hasHit;
		private float hitPauseDuration;

		private float stopwatch;
		private OverlapAttack detector;

		private List<GameObject> hitCoinObjects;
		private float hitPauseTimer;
		private float rollSpeed;

		private Animator animator;
		private Vector3 dashDirection;
		private EntityStateMachine weapon;

		public override void OnEnter()
		{
			base.OnEnter();
			this.animator = base.GetModelAnimator();
			this.animator.SetBool("utilityEnd", false);
			base.characterBody.SetAimTimer(2f);
			this.weapon = EntityStateMachine.FindByCustomName(base.gameObject, "Weapon");


			HitBoxGroup hitBoxGroup = FindHitBoxGroup("VaultCollision");


			Util.PlaySound("Play_treeBot_shift_charge", base.gameObject);
			this.detector = new OverlapAttack();
			this.detector.attacker = base.gameObject;
			this.detector.inflictor = base.gameObject;
			this.detector.teamIndex = GetTeam();
			this.detector.damage = 0f;
			this.detector.procCoefficient = 0f;
			this.detector.hitBoxGroup = hitBoxGroup;
			this.dashDirection = GetAimRay().direction;
			this.hitCoinObjects = new List<GameObject>();
			this.hitPauseDuration = Mathf.Max(baseHitPauseDuration / this.attackSpeedStat, minHitPauseDuration);
			base.PlayAnimation("FullBody, Override", "UtilityDash");
			Util.PlaySound(SlideState.soundString, base.gameObject);
		}

		private void RecalculateRollSpeed()
		{
			this.rollSpeed = this.moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, base.fixedAge / duration);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			RecalculateRollSpeed();
			FireAttack();
			this.hitPauseTimer -= Time.fixedDeltaTime;

			if (this.hitPauseTimer <= 0f && this.hasHit)
			{
				this.weapon.SetNextState(new JetpackVault
				{
					hitCoinObjects = this.hitCoinObjects
				});
				this.outer.SetNextStateToMain();
				return;
			}

			if (!hasHit)
			{
				base.characterMotor.rootMotion += this.dashDirection * this.rollSpeed * Time.fixedDeltaTime;
				this.stopwatch += Time.fixedDeltaTime;
			}

			base.characterDirection.forward = dashDirection;
			base.characterMotor.velocity = Vector3.zero;

			if (base.isAuthority && this.stopwatch >= duration)
			{
				this.animator.SetBool("utilityEnd", true);
				this.outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
		}

		private void FireAttack()
		{
			if (base.isAuthority)
			{
				List<HurtBox> hits = new List<HurtBox>();
				if (this.detector.Fire(hits))
				{
					foreach (HurtBox hurtBox in hits)
					{
						if (hurtBox.healthComponent && hurtBox.healthComponent.body)
						{
							ForceFlinch(hurtBox.healthComponent.body);
						}
					}
					OnHitEnemyAuthority();
				}
			}

		}

		protected virtual void OnHitEnemyAuthority()
		{
			this.hitCoinObjects = CoinProjectileController.CoinMethods.OverlapAttackGetCoins(detector).Select(coin => coin.gameObject).ToList();
			if (!this.hasHit)
			{
				base.PlayAnimation("FullBody, Override", "UtilityHit");
				float moveSpeedMultiplier = this.moveSpeedStat / base.characterBody.baseMoveSpeed;
				this.hitPauseTimer = this.hitPauseDuration / moveSpeedMultiplier;
				this.hasHit = true;
			}
		}

		protected virtual void ForceFlinch(CharacterBody body)
		{
			SetStateOnHurt component = body.healthComponent.GetComponent<SetStateOnHurt>();
			if (component == null)
			{
				return;
			}
			if (component.canBeHitStunned)
			{
				component.SetPain();
				if (body.characterMotor)
				{
					body.characterMotor.velocity = Vector3.zero;
				}
			}
			if (body.GetComponent<CoinProjectileController>() && body.rigidbody)
			{
				body.rigidbody.velocity = Vector3.zero;
			}
		}
	}
}


