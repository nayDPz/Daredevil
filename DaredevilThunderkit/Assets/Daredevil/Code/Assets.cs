using System.Collections.Generic;
using System.IO;
using System.Linq;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Daredevil
{
	internal static class Assets
	{
		internal static AssetBundle mainAssetBundle;

		internal static GameObject devilBikeObject;
		internal static GameObject swordSwingEffect;
		internal static GameObject swordSwingHeavyEffect;

		internal static GameObject comboHUD;
		internal static GameObject lineVisualiserPrefab;

		internal static Material matTPInOut;
		internal static Material matHuntressFlash;
		internal static Material matHuntressFlashExpanded;
		internal static Material matHuntressFlashBright;

		internal static SkillDef unequipSkill;
		internal static SkillDef specialSkill;
		internal static SkillDef ultraSkill;

		internal static GameObject captainTracer;
		internal static GameObject rocketPREFABBECAUSEIMSTILLNOTUSINGENTITYSTATECONFIGS;
		internal static GameObject STOLENFROMMERCXDD;
		internal static GameObject shotgunBombProjectile;
		internal static GameObject rocketEXPLOSIONEFFECTISTOLEFROMMULT;
		internal static GameObject railgunTazerProjectileREPLACETHISEVENTUALLY;
		internal static GameObject railgunTracer;
		internal static GameObject nailTracer;
		internal static GameObject coinTracer;
		internal static GameObject coinOrbEffect;
		internal static GameObject coinProjectile;

		internal static NetworkSoundEventDef swordHitSoundEvent;
		internal static NetworkSoundEventDef sword2HitSoundEvent;

		public static Material commandoMat;

		
		public static List<GameObject> clonedVanillaProjectiles = new List<GameObject>();
		public static string AssetBundlePath => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(DaredevilMain.PInfo.Location), "daredevilassets");

		public const string bundleName = "daredevilassets";

		internal static void PopulateAssets()
		{
			mainAssetBundle = AssetBundle.LoadFromFile(AssetBundlePath);
			SwapShaders(mainAssetBundle);

			swordHitSoundEvent = mainAssetBundle.LoadAsset<NetworkSoundEventDef>("PrimarySwordHit");
			sword2HitSoundEvent = mainAssetBundle.LoadAsset<NetworkSoundEventDef>("SecondarySwordHit");

			comboHUD = mainAssetBundle.LoadAsset<GameObject>("ComboHUD");
			lineVisualiserPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab").WaitForCompletion();

			ContentAddition.AddEffect(mainAssetBundle.LoadAsset<GameObject>("Explosion"));
			ContentAddition.AddEffect(mainAssetBundle.LoadAsset<GameObject>("BigExplosion"));
			ContentAddition.AddEffect(mainAssetBundle.LoadAsset<GameObject>("GunProjectileImpact"));

			// xDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
			rocketPREFABBECAUSEIMSTILLNOTUSINGENTITYSTATECONFIGS = mainAssetBundle.LoadAsset<GameObject>("RocketProjectile");
			railgunTazerProjectileREPLACETHISEVENTUALLY = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainTazer.prefab").WaitForCompletion(), "DaredevilRailgunTazer");
			
			railgunTazerProjectileREPLACETHISEVENTUALLY.GetComponent<ProjectileDamage>();
			railgunTazerProjectileREPLACETHISEVENTUALLY.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(DaredevilMain.armorShredOnHit);

			clonedVanillaProjectiles.Add(railgunTazerProjectileREPLACETHISEVENTUALLY);

			shotgunBombProjectile = mainAssetBundle.LoadAsset<GameObject>("ShotgunBomb");

			STOLENFROMMERCXDD = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion();

			coinProjectile = mainAssetBundle.LoadAsset<GameObject>("CoinProjectile");

			matTPInOut = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/matTPInOut.mat").WaitForCompletion();
			matHuntressFlash = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressFlash.mat").WaitForCompletion();
			matHuntressFlashBright = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressFlashBright.mat").WaitForCompletion();
			matHuntressFlashExpanded = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressFlashExpanded.mat").WaitForCompletion();

			unequipSkill = mainAssetBundle.LoadAsset<SkillDef>("sdDaredevilUnequip");
			specialSkill = mainAssetBundle.LoadAsset<SkillDef>("sdDaredevilSpecial");
			ultraSkill = mainAssetBundle.LoadAsset<SkillDef>("DaredevilUltraSpecial");
			
			

			captainTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/TracerCaptainShotgun.prefab").WaitForCompletion();
			railgunTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunSuper.prefab").WaitForCompletion();
			nailTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/TracerToolbotNails.prefab").WaitForCompletion();			
			coinTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GoldGat/TracerGoldGat.prefab").WaitForCompletion();
			coinOrbEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/LoaderLightningOrbEffect.prefab").WaitForCompletion();
			
		}

		internal static void SwapShaders(AssetBundle assetBundle)
		{
			Material[] mats = assetBundle.LoadAllAssets<Material>().Where(mat => mat.shader.name.Equals("Standard")).ToArray();//.Equals("Hopoo Games/Deferred/Standard")).ToArray();

			foreach (Material mat in mats)
			{
				Shader shader = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HGStandard.shader").WaitForCompletion();
				if (shader)
				{
					mat.shader = shader;
				}
			}
		}
	}
}


