using EntityStates;

namespace Daredevil.States
{
	public class NailgunSpinDown : BaseSkillState
	{
		public DaredevilWeaponDef weaponDef;
		public float duration = 0.5f;
		public static float baseDuration;

		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = baseDuration / this.attackSpeedStat;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
			}
		}
	}
}

