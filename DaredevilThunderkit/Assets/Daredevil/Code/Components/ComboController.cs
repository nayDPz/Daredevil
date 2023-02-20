using Daredevil.States;
using EntityStates.Huntress;
using RoR2;
using UnityEngine;

namespace Daredevil.Components
{
	public class ComboController : MonoBehaviour
	{
		private CharacterBody characterBody;
		private InputBankTest inputBank;

		public bool drain;
		public int displayComboOverride;
		public int comboCount;
		public int displayCombo;
		public int maxCombo = 6;
		private int comboDecayAmount;
		private float comboDecayStopwatch;
		private float comboDecayTime = 1f;
		private string lastAbilityHitKey;

		private bool displayIndicator;

		private int coinIndex;
		private float maxCoinAngle = 60f;
		private int coinsInPattern = 4;

		public GameObject areaIndicatorInstance;
		private LineRenderer arcVisualizerLineRenderer;
		private GameObject arcVisualizerPrefab = Assets.lineVisualiserPrefab;

		private void Start()
		{
			this.characterBody = GetComponent<CharacterBody>();
			this.inputBank = GetComponent<InputBankTest>();

			//XDD
			if (this.characterBody.skillLocator.special.skillDef == Assets.specialSkill)
			{
				if (ArrowRain.areaIndicatorPrefab)
				{
					areaIndicatorInstance = GameObject.Instantiate(ArrowRain.areaIndicatorPrefab);
					areaIndicatorInstance.transform.localScale = new Vector3(0f, 0f, 0f);
				}
				arcVisualizerLineRenderer = GameObject.Instantiate(arcVisualizerPrefab, base.transform.position, Quaternion.identity).GetComponent<LineRenderer>();
				arcVisualizerLineRenderer.positionCount = 2;
			}
		}

		private void FixedUpdate()
		{
			this.displayCombo = (this.drain ? this.displayComboOverride : this.comboCount);
			this.displayIndicator = false;
			if (this.comboCount >= this.maxCombo)
			{
				this.displayIndicator = true;
				this.comboDecayTime = 4f;
			}
			else
			{
				this.comboDecayTime = 3f;
			}
			UpdateAreaIndicator();
			ComboDecay();
		}

		public float GetCoinAngle()
		{
			float angle = ((coinIndex <= 3) ? (60 + -40 * coinIndex) : (-60 + 40 * (coinIndex - 3)));

			coinIndex++; // dont care shut up
			if (coinIndex >= 5)
			{
				coinIndex = 0;
			}
			return angle;
		}

		public void AddCombo(int combo, string abilityKey)
		{
			if (!abilityKey.Equals(lastAbilityHitKey))
			{
				this.comboCount += combo;
				Util.PlaySound("Combo" + this.comboCount, this.characterBody.gameObject);
			}
			if (this.comboCount + combo > this.maxCombo)
			{
				this.comboCount = maxCombo;
			}
			this.lastAbilityHitKey = abilityKey;
			this.comboDecayAmount = 1;
			this.comboDecayStopwatch = 0f;
		}

		public void ConsumeCombo()
		{
			this.lastAbilityHitKey = "";
			this.comboCount = 0;
		}

		public void RemoveCombo(int combo)
		{
			this.comboCount -= combo;
			if (this.comboCount < 0)
			{
				this.comboCount = 0;
			}
			if (this.comboCount == 0)
			{
				lastAbilityHitKey = "";
			}
		}

		private void UpdateAreaIndicator()
		{
			if (this.areaIndicatorInstance)
			{
				if (!this.characterBody.healthComponent.alive)
				{
					Destroy(this.arcVisualizerLineRenderer);
					Destroy(this.areaIndicatorInstance);
					return;
				}

				float scale = (displayIndicator ? (DeathSpin2Fire.range - 2f) : 0f);
				if (this.areaIndicatorInstance)
				{
					this.areaIndicatorInstance.transform.localScale = Vector3.one * scale;
				}
				float range = DeathSpin2Blink.blinkRange;
				Ray aimRay = this.inputBank.GetAimRay();
				RaycastHit hitInfo = default(RaycastHit);
				bool flag = false;
				if (DeathSpin2Blink.rayRadius > 0f && Util.CharacterSpherecast(base.gameObject, aimRay, DeathSpin2Blink.rayRadius, out hitInfo, range, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal) && hitInfo.collider.GetComponent<HurtBox>())
				{
					flag = true;
				}
				if (!flag)
				{
					flag = Util.CharacterRaycast(base.gameObject, aimRay, out hitInfo, range, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
				}
				Vector3 position = ((!flag) ? aimRay.GetPoint(range) : hitInfo.point);
				this.areaIndicatorInstance.transform.position = position;
				if (arcVisualizerLineRenderer)
				{
					if (this.displayIndicator)
					{
						this.arcVisualizerLineRenderer.SetPosition(0, characterBody.transform.position);
						this.arcVisualizerLineRenderer.SetPosition(1, position);
					}
					else
					{
						this.arcVisualizerLineRenderer.SetPosition(0, Vector3.zero);
						this.arcVisualizerLineRenderer.SetPosition(1, Vector3.zero);
					}
				}
			}
			
		}

		private void ComboDecay()
		{
			if (this.comboCount >= 1)
			{
				this.comboDecayStopwatch += Time.fixedDeltaTime;
			}
			if (this.comboDecayStopwatch >= this.comboDecayTime)
			{
				RemoveCombo(this.comboDecayAmount);
				this.comboDecayAmount++;
				this.comboDecayStopwatch -= comboDecayTime;
			}
		}

		
	}
}


