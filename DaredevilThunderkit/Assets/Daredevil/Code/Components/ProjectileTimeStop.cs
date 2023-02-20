using System.Collections.Generic;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace Daredevil.Components
{
	public class ProjectileTimeStop : MonoBehaviour, IOnIncomingDamageServerReceiver
	{
		public ProjectileImpactExplosion projectileImpactExplosion;
		public ProjectileSimple projectileSimple;
		public Rigidbody rigidBody;
		public ProjectileController controller;
		private GameObject owner;

		private bool usedGravity;
		private Vector3 velocityOnFreeze;
		private float forwardSpeedOnFreeze;
		private float lifetimeOnFreeze;
		private bool isFrozen;

		private void Awake()
		{
			if (!rigidBody)
				rigidBody = GetComponent<Rigidbody>();

			if (!projectileSimple)
				projectileSimple = GetComponent<ProjectileSimple>();

			if (!projectileImpactExplosion)
				projectileImpactExplosion = GetComponent<ProjectileImpactExplosion>();

			if (!controller)
				controller = GetComponent<ProjectileController>();

		}

		private void Start()
		{
			this.owner = this.controller.owner;
			if (this.owner)
			{
				ProjectileTimeStopOwnership component = this.owner.GetComponent<ProjectileTimeStopOwnership>();
				if (!component)
				{
					component = this.owner.AddComponent<ProjectileTimeStopOwnership>();
				}
				component.AddProjectile(this);
				if (component.isFrozen)
				{
					FreezeProjectile();
				}
			}
		}

		public void OnIncomingDamageServer(DamageInfo damageInfo)
		{
			damageInfo.rejected = true;
			return;
		}

		public void FreezeProjectile()
		{
			float stopwatch = 0f;
			if (this.rigidBody)
			{
				this.velocityOnFreeze = this.rigidBody.velocity;
				this.usedGravity = this.rigidBody.useGravity;
				this.rigidBody.useGravity = false;
				this.rigidBody.velocity = Vector3.zero;
			}
			if (this.projectileSimple)
			{
				stopwatch = this.projectileSimple.stopwatch;
				this.forwardSpeedOnFreeze = this.projectileSimple.desiredForwardSpeed;
				this.projectileSimple.SetForwardSpeed(0f);
				this.projectileSimple.desiredForwardSpeed = 0f;
			}
			if (this.projectileImpactExplosion)
			{
				stopwatch = this.projectileImpactExplosion.stopwatch;
			}
			this.isFrozen = true;
			this.lifetimeOnFreeze = stopwatch;
		}

		public void ThawProjectile()
		{
			if (this.isFrozen)
			{
				if (this.rigidBody)
				{
					this.rigidBody.useGravity = this.usedGravity;
					this.rigidBody.velocity = this.velocityOnFreeze;
				}
				if (this.projectileSimple)
				{
					this.projectileSimple.SetForwardSpeed(this.forwardSpeedOnFreeze);
					this.projectileSimple.desiredForwardSpeed = this.forwardSpeedOnFreeze;
				}
				this.isFrozen = false;
			}
		}

		private void FixedUpdate()
		{
			if (this.isFrozen)
			{
				if (this.projectileSimple)
				{
					this.projectileSimple.stopwatch = lifetimeOnFreeze;
					this.projectileSimple.SetForwardSpeed(0f);
					this.projectileSimple.desiredForwardSpeed = 0f;
				}
				if (this.rigidBody)
				{
					this.rigidBody.useGravity = false;
					this.rigidBody.velocity = Vector3.zero;
				}
				if (this.projectileImpactExplosion)
				{
					this.projectileImpactExplosion.stopwatch = lifetimeOnFreeze;
				}
			}
		}

		public class ProjectileTimeStopOwnership : MonoBehaviour
		{
			public List<ProjectileTimeStop> projectiles = new List<ProjectileTimeStop>();

			public bool isFrozen;

			private uint soundID;

			public void AddProjectile(ProjectileTimeStop component)
			{
				this.projectiles.Add(component);
			}

			private void FixedUpdate()
			{
				if (projectiles == null)
				{
					Destroy(this);
					return;
				}

				CleanList();
				
			}

			private void CleanList()
			{
				for (int i = projectiles.Count - 1; i >= 0; i--)
				{
					if (!this.projectiles[i])
					{
						this.projectiles.RemoveAt(i);
					}
				}
			}

			public void FreezeAll()
			{
				this.isFrozen = true;
				soundID = Util.PlaySound(Sounds.rocketLauncherTimeStop, base.gameObject);
				foreach (ProjectileTimeStop projectile in projectiles)
				{
					projectile.FreezeProjectile();
				}
			}

			public void ThawAll() // tha wall
			{
				this.isFrozen = false;
				Util.PlaySound(Sounds.rocketLauncherTimeStart, base.gameObject);
				AkSoundEngine.StopPlayingID(soundID);
				foreach (ProjectileTimeStop projectile in projectiles)
				{
					projectile.ThawProjectile();
				}
			}

			private void OnDestroy()
			{
				AkSoundEngine.StopPlayingID(soundID);
				if (this.isFrozen)
				{
					Util.PlaySound(Sounds.rocketLauncherTimeStart, base.gameObject);
				}
			}
		}
	}
}


