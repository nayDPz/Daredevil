using EntityStates;

namespace Daredevil.States
{
	public class DaredevilMainState : GenericCharacterMain
	{
		public override void ProcessJump()
		{
			base.ProcessJump();
			if (this.hasCharacterMotor && this.jumpInputReceived && base.characterBody && base.characterMotor.jumpCount < base.characterBody.maxJumpCount)
			{
				DoJumpAnimation("Body, Sword");
				DoJumpAnimation("Body, BigGun");
			}
		}

		private void DoJumpAnimation(string layerName)
		{
			int layerIndex = base.modelAnimator.GetLayerIndex(layerName);
			if (layerIndex >= 0)
			{
				if (base.characterMotor.jumpCount == 0 || base.characterBody.baseJumpCount == 1)
				{
					base.modelAnimator.CrossFadeInFixedTime("Jump", this.smoothingParameters.intoJumpTransitionTime, layerIndex);
				}
				else
				{
					base.modelAnimator.CrossFadeInFixedTime("BonusJump", this.smoothingParameters.intoJumpTransitionTime, layerIndex);
				}
			}
		}
	}
}


