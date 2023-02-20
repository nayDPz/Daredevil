using Daredevil.States;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Daredevil.Components
{
    public class CoinProjectileController : MonoBehaviour, IShootable, IProjectileImpactBehavior, IOnIncomingDamageServerReceiver
	{
		public HealthComponent projectileHealthComponent;
		public ProjectileController controller;
		public ComboController ownerComboController;

		public NetworkSoundEventDef splitSound;
		public NetworkSoundEventDef ricochetSound;

		public static float baseRicochetMultiplier = 2f;
		public bool canRicochet = true;
		private float graceTimer = 0.75f;
		public float ricochetMultiplier = 2f;
		private Vector3 rotationSpeed = new Vector3(2000f, 0f, 0f);
		public static Action<CoinProjectileController> onCoinAwakeGlobal;

		public void OnIncomingDamageServer(DamageInfo damageInfo)
		{
			if (damageInfo.attacker != this.controller.owner)
			{
				damageInfo.rejected = true;
			}
		}

		private void Start()
		{
			float speed = UnityEngine.Random.Range(500f, 2000f);
			this.rotationSpeed = new Vector3(speed, 0f, 0f);

			ownerComboController = controller.owner.GetComponent<ComboController>();
			if (onCoinAwakeGlobal != null)
			{
				onCoinAwakeGlobal(this);
			}
		}

		private void FixedUpdate()
		{
			this.graceTimer -= Time.fixedDeltaTime;
			base.transform.Rotate(this.rotationSpeed * Time.fixedDeltaTime);
		}

		public void OnShot(DamageInfo damageInfo)
		{
			RicochetBullet(damageInfo);
		}

		public bool CanBeShot()
		{
			return this.canRicochet;
		}

		public RicochetUtils.RicochetPriority GetRicochetPriority()
		{
			return RicochetUtils.RicochetPriority.Coin;
		}

		public void RicochetBullet(DamageInfo damageInfo)
		{
			if (damageInfo.attacker)
			{
				this.canRicochet = false;
				TeamComponent teamComponent = damageInfo.attacker.GetComponent<TeamComponent>();
				CoinRicochetOrb orb = new CoinRicochetOrb
				{
					coinPosition = base.transform.position,
					origin = base.transform.position,
					speed = 180f,
					attacker = damageInfo.attacker,
					damageValue = damageInfo.damage * this.ricochetMultiplier,
					damageType = DamageType.Generic,
					teamIndex = teamComponent.teamIndex,
					procCoefficient = 1f,
					isCrit = damageInfo.crit
				};
				OrbManager.instance.AddOrb(orb);
				EffectManager.SimpleSoundEffect(this.ricochetSound.index, base.transform.position, true);
				Destroy(base.gameObject);
			}
		}

		public void Split(GameObject attacker)
		{
			if (this.graceTimer <= 0f)
			{
				this.graceTimer = 0.5f;
				Vector3 between = attacker.transform.position - base.transform.position;
				between.y = 0f;
				Vector3 baseDirection = between.normalized;
				Vector3 origin = base.transform.position;

				Quaternion rotation1 = Quaternion.Euler(0f, 90f, 0f);
				Quaternion rotation2 = Quaternion.Euler(0f, -90f, 0f);
				Vector3 direction1 = rotation1 * baseDirection;
				Vector3 direction2 = rotation2 * baseDirection;

				CoinMethods.SpawnCoin(attacker, origin, direction1, 5f, 6f, ricochetMultiplier);
				CoinMethods.SpawnCoin(attacker, origin, direction2, 5f, 6f, ricochetMultiplier);

				EffectManager.SimpleSoundEffect(this.splitSound.index, base.transform.position, true);

				Destroy(base.gameObject);
			}
		}

		public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
		{
			if (this.graceTimer <= 0f && !impactInfo.collider.GetComponent<HurtBox>() && !impactInfo.collider.GetComponent<CoinProjectileController>())
			{
				Destroy(base.gameObject);
			}
		}

		public struct CoinMethods
		{
			public static BulletAttack CommonCoinBulletAttack(BulletAttack bulletAttack, GameObject attacker, Vector3 origin, Vector3 direction, float ricochetDamageMultiplier)
			{
				bool crit = false;
				CharacterBody body = attacker.GetComponent<CharacterBody>();

				if (body) crit = body.RollCrit();

				bulletAttack.aimVector = direction;
				bulletAttack.origin = origin;
				bulletAttack.owner = attacker;
				bulletAttack.weapon = null;
				bulletAttack.bulletCount = 1;
				bulletAttack.damage = 1f;
				bulletAttack.damageColorIndex = DamageColorIndex.Default;
				bulletAttack.damageType = DamageType.Stun1s;
				bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
				bulletAttack.force = 0f;
				bulletAttack.HitEffectNormal = false;
				bulletAttack.procChainMask = default(ProcChainMask);
				bulletAttack.procCoefficient = 0f;
				bulletAttack.maxDistance = 120f;
				bulletAttack.radius = 1.5f;
				bulletAttack.isCrit = crit;
				bulletAttack.muzzleName = "";
				bulletAttack.minSpread = 0f;
				bulletAttack.maxSpread = 0f;
				bulletAttack.hitEffectPrefab = null;
				bulletAttack.smartCollision = true;
				bulletAttack.sniper = false;
				bulletAttack.spreadPitchScale = 1f;
				bulletAttack.spreadYawScale = 1f;
				bulletAttack.tracerEffectPrefab = Assets.coinTracer;
				bulletAttack.hitCallback = delegate (BulletAttack bullet, ref BulletAttack.BulletHit hitInfo)
				{
					ComboController comboController = bullet.owner.GetComponent<ComboController>();
					TeamComponent team = bullet.owner.GetComponent<TeamComponent>();

					bool result = BulletAttack.defaultHitCallback(bullet, ref hitInfo);

					if (team && hitInfo.hitHurtBox && hitInfo.hitHurtBox.teamIndex != team.teamIndex)
					{
						if (comboController)
						{
							comboController.AddCombo(1, Coin.abilityKey);
						}
						SpawnCoinGeneric(hitInfo.hitHurtBox.transform.position, comboController, ricochetDamageMultiplier);
					}
					return result;
				};
				return bulletAttack;
			}

			private static void SpawnCoinGeneric(Vector3 origin, ComboController comboController, float damageMultiplier)
			{
				Vector3 between = comboController.transform.position - origin;
				between.y = 0f;
				Vector3 direction = between.normalized;
				float angle = UnityEngine.Random.Range(-60f, 60f);
				if (comboController)
				{
					angle = comboController.GetCoinAngle();
				}
				Vector3 coinDirection = Quaternion.Euler(0f, angle, 0f) * direction;
				SpawnCoin(comboController.gameObject, origin, coinDirection, Coin.coinHorizontalSpeed, Coin.coinBounceHeight, damageMultiplier);
			}

			public static void SpawnCoin(GameObject owner, Vector3 origin, Vector3 direction, float horizontalSpeed, float apexHeight, float damageMultiplier)
			{
				Vector3 hVelocity = direction * horizontalSpeed;
				float y = Trajectory.CalculateInitialYSpeedForHeight(apexHeight);
				Vector3 coinVelocity = new Vector3(hVelocity.x, y, hVelocity.z);
				float magnitude = coinVelocity.magnitude;
				Vector3 normalized = coinVelocity.normalized;
				Log.LogInfo("Spawned coin at " + origin + ", direction " + normalized + ", at speed " + magnitude);

				onCoinAwakeGlobal += (coin) => ModifyCoinOnSpawn(coin, damageMultiplier);
				ProjectileManager.instance.FireProjectile(Assets.coinProjectile, origin, Util.QuaternionSafeLookRotation(normalized),
					owner, 0f, 0f, false, DamageColorIndex.Default, null, magnitude);
				onCoinAwakeGlobal -= (coin) => ModifyCoinOnSpawn(coin, damageMultiplier);
			}

			public static void ModifyCoinOnSpawn(CoinProjectileController coin, float damageMultiplier)
			{
				coin.ricochetMultiplier = damageMultiplier;
			}

			public static void OverlapAttackSplitCoin(OverlapAttack attack)
			{
				foreach (CoinProjectileController coin in OverlapAttackGetCoins(attack))
				{
					coin.Split(attack.attacker);
				}
			}

			public static List<CoinProjectileController> OverlapAttackGetCoins(OverlapAttack attack)
			{
				List<CoinProjectileController> coins = new List<CoinProjectileController>();
				foreach (HealthComponent healthComponent in attack.ignoredHealthComponentList)
				{
					if (healthComponent)
					{
						CoinProjectileController coin = healthComponent.GetComponent<CoinProjectileController>();
						if (coin)
						{
							coins.Add(coin);
						}
					}
				}
				return coins;
			}
		}

	}
}

