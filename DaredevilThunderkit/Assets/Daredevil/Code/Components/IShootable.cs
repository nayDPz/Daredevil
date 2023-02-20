using RoR2;

namespace Daredevil.Components
{
	public interface IShootable
	{
		void OnShot(DamageInfo damageInfo);

		bool CanBeShot();

		RicochetUtils.RicochetPriority GetRicochetPriority();
	}
}


