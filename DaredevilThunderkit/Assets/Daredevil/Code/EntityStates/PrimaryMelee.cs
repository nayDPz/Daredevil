using Daredevil.Components;
using EntityStates;

namespace Daredevil.States
{
	public class PrimaryMelee : BasicMeleeAttack
	{
		private bool hitEnemy;
		private ComboController comboController;
		private WeaponController weaponController;

		public override void OnEnter()
		{

			// FUCKING ESC MANNNNNNNNNNNNNNNNNNNNNN
			baseDuration = 0.75f;
			damageCoefficient = 2.25f;
			procCoefficient = 1f;
			shorthopVelocityFromHit = 6f;
			hitEffectPrefab = Assets.STOLENFROMMERCXDD;
			hitPauseDuration = 0.08f;
			scaleHitPauseDurationAndVelocityWithAttackSpeed = true;
			mecanimHitboxActiveParameter = "PrimaryMelee.hitboxActive";
			beginSwingSoundString = "SwordPrimary";
			impactSound = Assets.swordHitSoundEvent;

			this.comboController = GetComponent<ComboController>();
			this.weaponController = GetComponent<WeaponController>();
			this.weaponController.SwordOut();
			base.OnEnter();
		}

		public override string GetHitBoxGroupName()
		{
			return "SwordHitbox";
		}

		public override void PlayAnimation()
		{
			string animationStateName = ((!this.weaponController.swordIsLeft) ? "SlashPrimary1" : "SlashPrimary2");
			PlayAnimation("Gesture, Override", animationStateName, "Slash.playbackRate", this.duration * 0.75f);
			this.weaponController.SwingSword();
		}

		public override void OnMeleeHitAuthority()
		{
			base.OnMeleeHitAuthority();
			if (!this.hitEnemy)
			{
				if (this.comboController)
				{
					this.comboController.AddCombo(1, "PrimaryMelee");
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


