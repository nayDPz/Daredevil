using Daredevil;
using RoR2;

public class DaredevilContent
{
	public static void Initialize()
	{
		Buffs.Initialize();
		WeaponDefs.Initialize();
	}

	public static class Buffs
	{
		public static BuffDef stunMarked;

		public static BuffDef stunMarkedCooldown;

		public static BuffDef shredArmor;

		public static void Initialize()
		{
			stunMarked = Assets.mainAssetBundle.LoadAsset<BuffDef>("bdStunMarked");
			stunMarkedCooldown = Assets.mainAssetBundle.LoadAsset<BuffDef>("bdStunMarkedCooldown");
			shredArmor = Assets.mainAssetBundle.LoadAsset<BuffDef>("bdShredArmor");
		}
	}

	public static class WeaponDefs
	{
		public static DaredevilWeaponDef BaseGuns;

		public static DaredevilWeaponDef Sword;

		public static DaredevilWeaponDef Sword2Handed;

		public static DaredevilWeaponDef Nailgun;

		public static DaredevilWeaponDef Railgun;

		public static DaredevilWeaponDef RocketLauncher;

		public static DaredevilWeaponDef Shotgun;

		public static void Initialize()
		{
			BaseGuns = Assets.mainAssetBundle.LoadAsset<DaredevilWeaponDef>("wdGuns");
			Sword = Assets.mainAssetBundle.LoadAsset<DaredevilWeaponDef>("wdSword");
			Sword2Handed = Assets.mainAssetBundle.LoadAsset<DaredevilWeaponDef>("wdSword2Hand");
			Nailgun = Assets.mainAssetBundle.LoadAsset<DaredevilWeaponDef>("wdNailgun");
			Railgun = Assets.mainAssetBundle.LoadAsset<DaredevilWeaponDef>("wdRailgun");
			RocketLauncher = Assets.mainAssetBundle.LoadAsset<DaredevilWeaponDef>("wdRocketLauncher");
			Shotgun = Assets.mainAssetBundle.LoadAsset<DaredevilWeaponDef>("wdShotgun");
		}
	}

	
}
