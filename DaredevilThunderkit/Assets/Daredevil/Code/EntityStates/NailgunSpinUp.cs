using EntityStates;
using RoR2;

namespace Daredevil.States
{
	public class NailgunSpinUp : BaseSkillState
	{
		public static float baseDuration;

		public DaredevilWeaponDef weaponDef;
		public float duration = 0.75f;
		private bool cancelled = true;

		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = baseDuration / this.attackSpeedStat;
			Util.PlaySound(Sounds.nailgunWindup, base.gameObject);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.cancelled = false;
				this.outer.SetNextState(new NailgunFire());
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			if (this.cancelled)
			{
				Util.PlaySound(Sounds.nailgunWinddown, base.gameObject);
			}
		}
	}
}


