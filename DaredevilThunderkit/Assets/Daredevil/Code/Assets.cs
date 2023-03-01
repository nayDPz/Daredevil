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
		internal static GameObject rocketProjectilePrefab;
		internal static GameObject STOLENFROMMERCXDD;
		internal static GameObject shotgunBombProjectile;
		internal static GameObject rocketEXPLOSIONEFFECTISTOLEFROMMULT;
		internal static GameObject railgunTazerProjectileREPLACETHISEVENTUALLY;
		internal static GameObject railgunTracer;
		internal static GameObject nailTracer;

		internal static GameObject muzzleFlashNailgun;
		internal static GameObject muzzleFlashRailgun;
		internal static GameObject muzzleFlashRocket;
		internal static GameObject muzzleFlashShotgun;
		internal static GameObject muzzleFlashPistol;
		internal static GameObject muzzleFlashRevolver;

		internal static GameObject pistolHit;
		internal static GameObject revolverHit;

		internal static GameObject stunIndicator;

		internal static GameObject gunImpactEffect;

		internal static GameObject pistolTracer;
		internal static GameObject revolverTracer;
		internal static GameObject coinTracer;
		internal static GameObject coinOrbEffect;
		internal static GameObject coinProjectile;


		internal static GameObject jetpackBurst;
		internal static GameObject jetpackDashEffect;
		internal static GameObject jetpackChargeEffect;

		internal static NetworkSoundEventDef swordHitSoundEvent;
		internal static NetworkSoundEventDef sword2HitSoundEvent;

		public static Material commandoMat;

		public static Dictionary<string, string> ShaderSwap = new Dictionary<string, string>()
		{
			{"stubbed hopoo games/deferred/standard",  "HGStandard.shader"},
			{"stubbed hopoo games/fx/cloud remap",  "HGCloudRemap.shader"},
			{"stubbed hopoo games/fx/distortion",  "HGDistortion.shader"}
		};

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
			rocketProjectilePrefab = mainAssetBundle.LoadAsset<GameObject>("RocketProjectile");
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

			muzzleFlashNailgun = mainAssetBundle.LoadAsset<GameObject>("MuzzleFlashNailgun");
			muzzleFlashRailgun = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/MuzzleflashRailgun.prefab").WaitForCompletion();
			muzzleFlashShotgun = mainAssetBundle.LoadAsset<GameObject>("MuzzleFlashShotgun");
			muzzleFlashRocket = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/MuzzleflashSmokeRing.prefab").WaitForCompletion();

			gunImpactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ImpactToolbotDash.prefab").WaitForCompletion();

			muzzleFlashPistol = mainAssetBundle.LoadAsset<GameObject>("MuzzleFlashPistol");
			muzzleFlashRevolver = mainAssetBundle.LoadAsset<GameObject>("MuzzleFlashRevolver");

			jetpackBurst = mainAssetBundle.LoadAsset<GameObject>("JetpackBurstEffect");
			jetpackChargeEffect = mainAssetBundle.LoadAsset<GameObject>("JetpackChargeEffect");
			jetpackDashEffect = mainAssetBundle.LoadAsset<GameObject>("JetpackBoostEffect");

			railgunTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunSuper.prefab").WaitForCompletion();
			captainTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/TracerCaptainShotgun.prefab").WaitForCompletion();
			railgunTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunSuper.prefab").WaitForCompletion();
			nailTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/TracerToolbotNails.prefab").WaitForCompletion();

			pistolHit = mainAssetBundle.LoadAsset<GameObject>("BulletImpact");
			revolverHit = mainAssetBundle.LoadAsset<GameObject>("BulletImpactBig");

			pistolTracer = mainAssetBundle.LoadAsset<GameObject>("BulletTracer");
			revolverTracer = mainAssetBundle.LoadAsset<GameObject>("BulletTracerBig");
			coinTracer = mainAssetBundle.LoadAsset<GameObject>("CoinTracer"); //Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GoldGat/TracerGoldGat.prefab").WaitForCompletion();
			coinOrbEffect = mainAssetBundle.LoadAsset<GameObject>("CoinOrbEffect"); //Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/LoaderLightningOrbEffect.prefab").WaitForCompletion();
			
		}

		internal static void SwapShaders(AssetBundle assetBundle)
		{
			Material[] mats = assetBundle.LoadAllAssets<Material>();

			foreach (Material mat in mats)
			{
				//Log.LogDebug(mat.name + "||" + mat.shader.name + " ----------------------------- ");
				if (!mat.shader.name.StartsWith("Stubbed")) continue;

				if (ShaderSwap.TryGetValue(mat.shader.name.ToLower(), out string shaderKey))
                {
					Shader shader = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/" + shaderKey).WaitForCompletion();
					if (shader)
					{
						mat.shader = shader;
						//Log.LogInfo("Swapped shader for " + mat.name);
					}
				}

				
			}
		}
	}
}


