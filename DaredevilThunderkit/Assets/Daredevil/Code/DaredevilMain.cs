using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using Daredevil.Components;
using EntityStates;
using HG;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.Networking;
using RiskOfOptions;

using ModdedDamageType = R2API.DamageAPI.ModdedDamageType;
using System.Runtime.CompilerServices;

namespace Daredevil
{
	[BepInDependency("com.bepis.r2api.content_management")]
	[BepInDependency("com.bepis.r2api")]
	[BepInDependency("com.bepis.r2api.damagetype")]
	[BepInDependency("com.bepis.r2api.language")]
	[BepInDependency("com.bepis.r2api.prefab")]
	[BepInDependency("com.bepis.r2api.unlockable")]

	[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]

	[BepInPlugin("com.nayDPz.Daredevil", "Daredevil", "1.0.0")]
	public class DaredevilMain : BaseUnityPlugin
	{
		

		public const string GUID = "com.nayDPz.Daredevil";
		public const string MODNAME = "Daredevil";
		public const string VERSION = "0.0.1";

		public static GameObject bodyPrefab;
		public static BodyIndex bodyIndex;
		public static ModdedDamageType applyStunMark;
		public static ModdedDamageType knockupOnHit;
		public static ModdedDamageType armorShredOnHit;
		public static R2APISerializableContentPack serializableContentPack;

		internal List<Type> entityStates;

		private static uint _bankID;
		private static uint _bankID2;
		public static DaredevilMain Instance { get; private set; }
		public static PluginInfo PInfo { get; private set; }


		internal static bool RiskOfOptionsInstalled;
		internal static float comboVolume = 100f;
		public static AudioManager.VolumeConVar cvComboVolume = new AudioManager.VolumeConVar("volume_combo", 
			ConVarFlags.Archive | ConVarFlags.Engine, "100", 
			"The sound volume for gaining combo, from 0 to 100.", "Volume_Combo");

		private void Awake()
		{
			Instance = this;
			PInfo = this.Info;
			Log.Init(Logger);

			Languages.Init();
			Daredevil.Config.ReadConfig();

			if (RiskOfOptionsInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
				SetupOptions();
			}
				
			Daredevil.Config.comboVolume.SettingChanged += (object sender, EventArgs args) => { cvComboVolume.AttemptSetString(Daredevil.Config.comboVolume.Value.ToString()); };

			Assets.PopulateAssets();
			DaredevilContent.Initialize();

			bodyPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("DaredevilBody");
			serializableContentPack = Assets.mainAssetBundle.LoadAsset<R2APISerializableContentPack>("ContentPack");
			
			new ContentPacks().Initialize();

			knockupOnHit = DamageAPI.ReserveDamageType();
			applyStunMark = DamageAPI.ReserveDamageType();
			armorShredOnHit = DamageAPI.ReserveDamageType();

			// XDDD
			ClonesToContentPackSHOULDNOTEXIST();
			SetupBody(bodyPrefab);
			Hook();

			ContentManager.onContentPacksAssigned += LateSetup;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void SetupOptions()
        {			
			ModSettingsManager.AddOption(new RiskOfOptions.Options.SliderOption(Daredevil.Config.comboVolume));
        }

		public void Start()
		{
			cvComboVolume.AttemptSetString(Daredevil.Config.comboVolume.Value.ToString());
		}

		[SystemInitializer]
		public static void LoadSoundBank()
		{
			if (Application.isBatchMode)
			{
				return;
			}
			try
			{
				var path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				AkSoundEngine.AddBasePath(path);
				var result = AkSoundEngine.LoadBank("DaredevilSoundBank.bnk", out _bankID);
				if (result != AKRESULT.AK_Success)
					Log.LogError("SoundBank Load Failed: " + result);

				result = AkSoundEngine.LoadBank("UltrakillSoundBank.bnk", out _bankID);
				if (result != AKRESULT.AK_Success)
					Log.LogError("SoundBank Load Failed: " + result);				
			}
			catch (Exception e)
			{
				Log.LogError(e);
			}
		}

		[SystemInitializer(typeof(BodyCatalog))]
		private static void FindBodyIndex()
		{
			bodyIndex = bodyPrefab.GetComponent<CharacterBody>().bodyIndex;
		}

		private void SetupBody(GameObject bodyPrefab)
		{
			bodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandard.asset").WaitForCompletion();
			bodyPrefab.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
			bodyPrefab.GetComponent<ModelLocator>().modelTransform.gameObject.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
			bodyPrefab.GetComponent<CharacterBody>().preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").WaitForCompletion();

		}

		//item displays
		private void LateSetup(ReadOnlyArray<ReadOnlyContentPack> obj)
		{
		}


		/// <summary>
		/// /////////////////////////////////////////////xxxxxxxxxxDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
		/// </summary>
		private void ClonesToContentPackSHOULDNOTEXIST()
		{
			GameObject gameObject = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/MercMonsterMaster"), "DaredevilMonsterMaster", true);
			gameObject.GetComponent<CharacterMaster>().bodyPrefab = bodyPrefab;
			serializableContentPack.masterPrefabs = new GameObject[1] { gameObject };
			GameObject[] array = serializableContentPack.projectilePrefabs;
			ArrayHelper.Append(ref array, Assets.clonedVanillaProjectiles);
			serializableContentPack.projectilePrefabs = array;
		}

		internal static class ArrayHelper
		{
			public static T[] Append<T>(ref T[] array, List<T> list)
			{
				int num = array.Length;
				int count = list.Count;
				Array.Resize(ref array, num + count);
				list.CopyTo(array, num);
				return array;
			}
			public static Func<T[], T[]> AppendDel<T>(List<T> list)
			{
				return (T[] r) => Append(ref r, list);
			}
		}

		private void Hook()
		{
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            On.EntityStates.StunState.OnEnter += StunState_OnEnter;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
		}

		private void StunState_OnEnter(On.EntityStates.StunState.orig_OnEnter orig, StunState self)
		{
			orig(self);
			if(NetworkServer.active)
            {
				//ULTRA MEGA SCUFFEDDDDDDDDDDDDDDDDDD
				// NEED TO ADD INDICATORS ASAP
				foreach(TeamComponent t in TeamComponent.GetTeamMembers(TeamIndex.Player))
					if(t.body.bodyIndex == bodyIndex)
						self.characterBody.AddTimedBuff(DaredevilContent.Buffs.stunMarked, 2f);
			}
				
		}

		private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
			if(self.body)
            {
				if (DamageAPI.HasModdedDamageType(damageInfo, armorShredOnHit))
				{
					self.body.AddTimedBuff(DaredevilContent.Buffs.shredArmor, 6f);
				}
				if (DamageAPI.HasModdedDamageType(damageInfo, knockupOnHit) && self.body)
				{
					self.body.ClearTimedBuffs(DaredevilContent.Buffs.stunMarked);
					if (self.body.characterMotor && !self.body.isBoss)
					{
						float knockup = 1f;
						if (self.body.characterMotor.mass >= 700f)
						{
							knockup = 0f;
						}
						else if (self.body.characterMotor.mass >= 300f)
						{
							knockup = 0.5f;
						}

						self.body.characterMotor.velocity = Vector3.zero;
						self.body.characterMotor.Motor.ForceUnground();

						if (self.body.transform.position.y >= damageInfo.attacker.transform.position.y - 3f && self.body.transform.position.y <= damageInfo.attacker.transform.position.y)
						{
							self.body.characterMotor.AddDisplacement(Vector3.up * knockup);
						}
						if (self.body.transform.position.y > damageInfo.attacker.transform.position.y && self.body.transform.position.y <= damageInfo.attacker.transform.position.y + 6f)
						{
							self.body.characterMotor.AddDisplacement(Vector3.up * knockup * 0.67f);
						}
					}
				}
				if (DamageAPI.HasModdedDamageType(damageInfo, applyStunMark))
				{
					self.body.AddTimedBuff(DaredevilContent.Buffs.stunMarked, 2f);
					self.body.ClearTimedBuffs(DaredevilContent.Buffs.stunMarkedCooldown);
				}

				if (damageInfo.attacker)
				{
					CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
					if (damageInfo.attacker && body && body.bodyIndex == bodyIndex)
					{
						if (self.body.characterMotor && !self.body.isBoss)
						{
							if (self.body.characterMotor.mass >= 300f)
							{
								damageInfo.force *= self.body.characterMotor.mass / 450f;
							}
							else
							{
								damageInfo.force *= self.body.characterMotor.mass / 300f;
							}
						}
						else
						{
							damageInfo.force = damageInfo.force.normalized * 75f;
						}
					}
				}
			}
						
			orig(self, damageInfo);
		}


		private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, HUD self)
        {
			orig(self);
			ComboHUD comboHUD = self.gameObject.AddComponent<ComboHUD>();
			GameObject gameObject = GameObject.Instantiate(Assets.comboHUD, self.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas"));
			RectTransform rect = gameObject.GetComponent<RectTransform>();
			rect.rotation = Quaternion.Euler(0f, 0f, 150f);
			rect.localPosition = Vector3.zero;
			rect.anchoredPosition = new Vector3(-30f, 10f);
			rect.localScale = new Vector3(0.2f, 0.5f, 1f);

			comboHUD.comboGauge = gameObject;
			comboHUD.comboFill = gameObject.transform.Find("GaugeFill").gameObject.GetComponent<Image>();
		}

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
			orig(self);
			self.armor -= self.GetBuffCount(DaredevilContent.Buffs.shredArmor) * 20f;
		}
	}
}
