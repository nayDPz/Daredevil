using Daredevil.Components;
using EntityStates;

namespace Daredevil.States
{
	public class SecondaryMelee : BasicMeleeAttack
	{
		private bool hitEnemy;

		private ComboController comboController;

		private WeaponController weaponController;

		public override void OnEnter()
		{
			// ecs cekasghfiwehfgadfkjasflasef
			baseDuration = 0.75f;
			damageCoefficient = 3.5f;
			procCoefficient = 1f;
			shorthopVelocityFromHit = 12f;
			hitEffectPrefab = Assets.STOLENFROMMERCXDD;
			hitPauseDuration = 0.1f;
			mecanimHitboxActiveParameter = "SecondaryMelee.hitboxActive";
			beginStateSoundString = "SwordPrimary";
			impactSound = Assets.sword2HitSoundEvent;

			if (!base.isGrounded)
			{
				SmallHop(base.characterMotor, 7f);
			}

			this.comboController = GetComponent<ComboController>();
			this.weaponController = GetComponent<WeaponController>();
			this.weaponController.SwordOut2Handed();
			base.OnEnter();
		}

		public override void PlayAnimation()
		{
			string anim = weaponController.swordIsLeft ? "SlashSecondary1" : "SlashSecondary2";
			PlayAnimation("Gesture, Override", anim, "Slash.playbackRate", this.duration);
			this.weaponController.SwingSword();
		}

		public override string GetHitBoxGroupName()
		{
			return "SwordHitbox";
		}

		public override void OnMeleeHitAuthority()
		{
			base.OnMeleeHitAuthority();
			if (!this.hitEnemy)
			{
				if (this.comboController)
				{
					comboController.AddCombo(1, "SecondaryMelee");
				}
				this.hitEnemy = true;
			}
			CoinProjectileController.CoinMethods.OverlapAttackSplitCoin(base.overlapAttack);
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}
}


