using Daredevil.Components;
using EntityStates;

namespace Daredevil.States
{
	public class FreezeProjectiles : BaseSkillState
	{
		public static float maxFreezeDuration = 8f;
		private bool cancelled = true;

		public override void OnEnter()
		{
			base.OnEnter();
			ProjectileTimeStop.ProjectileTimeStopOwnership component = GetComponent<ProjectileTimeStop.ProjectileTimeStopOwnership>();
			if (!component)
			{
				component = base.gameObject.AddComponent<ProjectileTimeStop.ProjectileTimeStopOwnership>();
			}
			component.FreezeAll();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= maxFreezeDuration)
			{
				this.outer.SetNextState(new ThawProjectiles());
			}
		}

		public override void ModifyNextState(EntityState nextState)
		{
			this.cancelled = nextState.GetType() != typeof(ThawProjectiles);
			base.ModifyNextState(nextState);
		}

		public override void OnExit()
		{
			base.OnExit();
			if (this.cancelled)
			{
				ProjectileTimeStop.ProjectileTimeStopOwnership component = GetComponent<ProjectileTimeStop.ProjectileTimeStopOwnership>();
				if (component)
				{
					component.ThawAll();
				}
			}
		}
	}
}


