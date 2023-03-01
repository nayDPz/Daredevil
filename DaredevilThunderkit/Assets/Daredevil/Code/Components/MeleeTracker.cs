using RoR2;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Daredevil.Components
{
	public class MeleeTracker : MonoBehaviour
	{
		private float rayRadius = 1f;
		public float meleeRange = 8f;
		public CharacterBody characterBody;
		public GameObject indicatorPrefab;

		private InputBankTest inputBank;
		private Dictionary<HealthComponent, GameObject> indicators;

		private SphereSearch search = new SphereSearch();

		public float trackerUpdateFrequency = 10f;
		private float trackerUpdateStopwatch;
		private List<HurtBox> targetList = new List<HurtBox>();

		private void Start()
		{
			this.characterBody = GetComponent<CharacterBody>();
			this.inputBank = GetComponent<InputBankTest>();
			this.indicators = new Dictionary<HealthComponent, GameObject>();
		}
		private void FixedUpdate()
        {
			if (!Util.HasEffectiveAuthority(this.characterBody.gameObject))
				return;

			this.trackerUpdateStopwatch += Time.fixedDeltaTime;
			if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
			{
				this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;

				List<HurtBox> oldHurtBoxes = this.targetList;
				this.targetList = Search();

				CleanTargets(oldHurtBoxes);
			}
		}

		private void CleanTargets(List<HurtBox> oldList)
        {
			for (int i = oldList.Count - 1; i >= 0; i--)
			{
				HurtBox hurtBox = oldList[i];
				if (!hurtBox.healthComponent || !hurtBox.healthComponent.alive || !this.targetList.Contains(oldList[i]))
				{
					HurtBox gone = oldList[i];
					if (this.indicators.TryGetValue(gone.healthComponent, out GameObject indicator))
					{
						GameObject.Destroy(indicator.gameObject);
						this.indicators.Remove(gone.healthComponent);
					}
				}
			}
		}
		private List<HurtBox> Search()
        {
			TeamMask mask = TeamMask.GetUnprotectedTeams(this.characterBody.teamComponent.teamIndex);

			this.search.origin = this.characterBody.corePosition;
			this.search.radius = this.meleeRange;
			this.search.mask = LayerIndex.entityPrecise.mask;
			this.search.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
			List<HurtBox> hurtBoxes = this.search.RefreshCandidates().FilterCandidatesByHurtBoxTeam(mask)
				.FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().ToList();

			foreach (HurtBox hurtBox in hurtBoxes)
			{
				AddIndicator(hurtBox.hurtBoxGroup.mainHurtBox);
			}

			return hurtBoxes.Select<HurtBox, HurtBox>(hb => hb.hurtBoxGroup.mainHurtBox).ToList();

		}

		private void AddIndicator(HurtBox hurtBox)
		{
			GameObject indicator;
			if (!this.indicators.TryGetValue(hurtBox.healthComponent, out indicator))
			{
				indicator = GameObject.Instantiate(this.indicatorPrefab);
				indicators.Add(hurtBox.healthComponent, indicator);

				foreach (ParticleSystem p in indicator.GetComponentsInChildren<ParticleSystem>())
				{
					float scale = p.startSize * hurtBox.healthComponent.body.bestFitRadius * 0.35f;
					p.startSize = scale;
				}
			}							
			indicator.transform.parent = hurtBox.transform;
			indicator.transform.localPosition = Vector3.zero;
		}

		public bool HasTarget()
		{
			bool hit = Util.CharacterSpherecast(this.characterBody.gameObject, this.inputBank.GetAimRay(), this.rayRadius, out RaycastHit hitInfo,
				this.meleeRange - this.rayRadius, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);
			hit |= TestRaycastHit(hitInfo);

			return hit;
		}

		public bool TestRaycastHit(RaycastHit raycastHit)
		{
			Collider collider = raycastHit.collider;
			if (collider)
			{
				HurtBox hurtBox = collider.GetComponent<HurtBox>();
				if (hurtBox)
				{
					HealthComponent healthComponent = hurtBox.healthComponent;
					if (healthComponent && healthComponent.alive 
						&& healthComponent.body.teamComponent.teamIndex != this.characterBody.teamComponent.teamIndex)
					{
						return true;
					}
				}
			}

			
			return false;
		}

	}
}


