using System.Collections.Generic;
using System.Linq;
using Daredevil.Components;
using EntityStates;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Daredevil.States
{
	public class JetpackVault : BaseSkillState
	{
		public static float baseDuration = 0.6f;
		public static float earlyExitTime = 0.75f;
		public static float minDuration = 0.2f;
		public static float vaultForce = 2100f;
		public static float coinLaunchMaxDistance = 60f;
		public static float damageCoefficient = 6f;

		public List<GameObject> hitCoinObjects;
		public List<CoinProjectileController> hitCoins;
		public Vector3 bounceVector;
		public DamageType damageType = DamageType.Stun1s;

		public float procCoefficient = 1f;
		public Vector3 bonusForce = new Vector3(0f, 4500f, 0f);
		private float duration;

		public override void OnEnter()
		{
			base.OnEnter();
			float moveSpeedMultiplier = this.moveSpeedStat / base.characterBody.baseMoveSpeed;
			this.duration = baseDuration / moveSpeedMultiplier;
			base.characterBody.SetAimTimer(2f);

			Vector3 direction = GetAimRay().direction;
			Vector3 force = direction * -1f;
			force.y = 1f;
			force *= vaultForce;

			base.characterMotor.ApplyForce(force, true);
			base.PlayAnimation("FullBody, Override", "UtilityBackflip");
			Util.PlaySound(Sounds.jetpackBurst, base.gameObject);

			HitBoxGroup hitBoxGroup = FindHitBoxGroup("VaultHitbox");


			EffectManager.SimpleEffect(Assets.jetpackBurst, base.characterBody.corePosition,
				Util.QuaternionSafeLookRotation(direction), false);

			if (base.isAuthority)
			{
				OverlapAttack overlapAttack = new OverlapAttack();
				overlapAttack.damageType = damageType;
				overlapAttack.attacker = base.gameObject;
				overlapAttack.inflictor = base.gameObject;
				overlapAttack.teamIndex = GetTeam();
				overlapAttack.damage = damageCoefficient * damageStat;
				overlapAttack.procCoefficient = procCoefficient;
				overlapAttack.hitEffectPrefab = null;
				overlapAttack.forceVector = bonusForce;
				overlapAttack.hitBoxGroup = hitBoxGroup;
				overlapAttack.isCrit = RollCrit();
				DamageAPI.AddModdedDamageType(overlapAttack, DaredevilMain.applyStunMark);
				ComboController comboController = GetComponent<ComboController>();
				this.hitCoins = new List<CoinProjectileController>();
				if (overlapAttack.Fire())
				{
					if (comboController)
					{
						comboController.AddCombo(1, "JetpackVault");
					}
					this.hitCoins = CoinProjectileController.CoinMethods.OverlapAttackGetCoins(overlapAttack);
				}
				if (this.hitCoinObjects != null)
				{
					this.hitCoins = this.hitCoins.Concat(from g in hitCoinObjects
														 where g.GetComponent<CoinProjectileController>()
														 select g.GetComponent<CoinProjectileController>()).ToList();
				}
				if (this.hitCoins.Count > 0)
				{
					FireCoins(direction);
				}
			}

		}

		private void FireCoins(Vector3 direction)
		{
			BullseyeSearch search = new BullseyeSearch();
			search.teamMaskFilter = TeamMask.allButNeutral;
			search.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
			search.maxDistanceFilter = coinLaunchMaxDistance;
			search.maxAngleFilter = 90f;
			Ray aimRay = GetAimRay();
			search.searchOrigin = aimRay.origin;
			search.searchDirection = aimRay.direction;
			search.filterByLoS = false;
			search.sortMode = BullseyeSearch.SortMode.Angle;
			search.RefreshCandidates();

			HurtBox[] hurtBoxes = search.GetResults().ToArray();
			for (int i = 0; i < hitCoins.Count; i++)
			{
				CoinProjectileController coin = hitCoins[i];
				Vector3 coinDirection = direction;
				if (hurtBoxes.Length != 0)
				{
					int hurtboxIndex = i % hurtBoxes.Length;
					//Log.LogDebug("OFF OF " + hurtBoxes[hurtboxIndex].healthComponent.name);
					coinDirection = (hurtBoxes[hurtboxIndex].transform.position - base.transform.position).normalized;
				}
				//Log.LogDebug("COIN RELAUNCH " + i + " , " + coinDirection);
				if (coin)
				{
					float multiplier = CoinProjectileController.baseRicochetMultiplier;
					if (coin)
					{
						multiplier *= coin.ricochetMultiplier;
					}
					CoinProjectileController.CoinMethods.CommonCoinBulletAttack(new BulletAttack(), base.gameObject, coin.transform.position, coinDirection, multiplier).Fire();
					EntityState.Destroy(this.hitCoins[i].gameObject);
				}
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
			}
		}

		public override void OnSerialize(NetworkWriter writer)
		{
			writer.Write(this.hitCoinObjects.Count);
			foreach (GameObject hitCoinObject in this.hitCoinObjects)
			{
				writer.Write(hitCoinObject.gameObject);
			}
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			this.hitCoinObjects = new List<GameObject>();
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				this.hitCoinObjects.Add(reader.ReadGameObject());
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return (base.fixedAge >= this.duration * earlyExitTime) ? InterruptPriority.Skill : InterruptPriority.Pain;
		}
	}
}


