using System.Collections.Generic;
using R2API;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace Daredevil.Components
{
	public class CoinRicochetOrb : GenericDamageOrb
	{
		public static GameObject orbPrefab = Assets.coinOrbEffect;
		public static float redDamageCoefficient = 16f;

		public ComboController comboController;
		public float searchRadius = 50f;
		public SphereSearch search;
		public Vector3 coinPosition;

		public float damageCoefficient = 1f;

		public override void Begin()
		{
			this.target = PickNextTarget(this.coinPosition);
			
			this.duration = this.distanceToTarget / this.speed;

			Color color = Color.Lerp(Color.yellow, Color.red, damageCoefficient / redDamageCoefficient);
			float scale = Mathf.Lerp(1, 2f, damageCoefficient / redDamageCoefficient);
			EffectData effectData = new EffectData
			{
				scale = this.scale * scale,
				origin = this.coinPosition,
				genericFloat = this.duration,
				color = color,
			};
			effectData.SetHurtBoxReference(this.target);
			EffectManager.SpawnEffect(Assets.coinOrbEffect, effectData, true);

			//Log.LogInfo(Assets.coinOrbEffect + " to " + this.target + " in " + effectData.genericFloat + "s");
		}


		public override void OnArrival()
		{
			if (this.target)
			{
				HealthComponent healthComponent = target.healthComponent;
				if (healthComponent)
				{
					DamageInfo damageInfo = new DamageInfo();
					damageInfo.damage = damageValue;
					damageInfo.attacker = attacker;
					damageInfo.inflictor = null;
					damageInfo.force = Vector3.zero;
					damageInfo.crit = isCrit;
					damageInfo.procChainMask = procChainMask;
					damageInfo.procCoefficient = procCoefficient;
					damageInfo.position = target.transform.position;
					damageInfo.damageColorIndex = damageColorIndex;
					damageInfo.damageType = DamageType.Stun1s;

					DamageAPI.AddModdedDamageType(damageInfo, DaredevilMain.applyStunMark);
					healthComponent.TakeDamage(damageInfo);

					IShootable shootable = healthComponent.GetComponent<IShootable>();
					if (shootable != null)
					{
						shootable.OnShot(damageInfo);
					}
					else
					{
						GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
						GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
					}

					if (comboController)
					{
						comboController.AddCombo(1, "CoinRicochet");
					}
				}
			}
			
		}

		public HurtBox PickNextTarget(Vector3 position)
		{
			HurtBox target = null;

			this.search = new SphereSearch
			{
				mask = LayerIndex.entityPrecise.mask,
				radius = searchRadius,
				origin = position
			};
			
			TeamMask teamMask = TeamMask.GetUnprotectedTeams(teamIndex);
			HurtBox[] hurtBoxes = search.RefreshCandidates().OrderCandidatesByDistance().GetHurtBoxes();
			RicochetUtils.RicochetPriority prio = RicochetUtils.RicochetPriority.None;

			foreach (HurtBox hurtBox in hurtBoxes)
			{
				if (!hurtBox.healthComponent)
					continue;

				List<IShootable> shootables = new List<IShootable>();
				hurtBox.healthComponent.GetComponents(shootables);
				foreach (IShootable shootable in shootables)
				{
					if (shootable.CanBeShot())
					{
						RicochetUtils.RicochetPriority myPrio = shootable.GetRicochetPriority();
						if (prio < myPrio)
						{
							target = hurtBox;
							prio = myPrio;
						}
					}
				}
				CharacterBody body = hurtBox.healthComponent.body;
				if (teamMask.HasTeam(body.teamComponent.teamIndex))
				{
					if (prio < RicochetUtils.RicochetPriority.Body && body)
					{
						target = hurtBox;
						prio = RicochetUtils.RicochetPriority.Body;
					}
					if (prio < RicochetUtils.RicochetPriority.StunnedBody && body && body.HasBuff(DaredevilContent.Buffs.stunMarked))
					{
						target = hurtBox;
						prio = RicochetUtils.RicochetPriority.StunnedBody;
					}
				}
			}
			//Log.LogWarning("RICHOCHET TO " + target);
			return target;
		}
	}

}

