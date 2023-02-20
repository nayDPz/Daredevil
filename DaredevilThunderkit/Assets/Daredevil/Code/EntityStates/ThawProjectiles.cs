using Daredevil.Components;
using EntityStates;

namespace Daredevil.States
{
	public class ThawProjectiles : BaseSkillState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			ProjectileTimeStop.ProjectileTimeStopOwnership component = GetComponent<ProjectileTimeStop.ProjectileTimeStopOwnership>();
			if (!component)
			{
				component = base.gameObject.AddComponent<ProjectileTimeStop.ProjectileTimeStopOwnership>();
			}
			component.ThawAll();
			this.outer.SetNextStateToMain();
		}
	}
}


