using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;

namespace Daredevil.Components
{
	public class StunIndicator : MonoBehaviour
	{
		private void Update()
		{
			if(Util.HasEffectiveAuthority(this.owner))
				this.UpdateVisualizer();
		}
		public void UpdateVisualizer()
		{
			if(!this.targetBody)
            {
				Destroy(base.gameObject);
				return;
            }

			this.indicatorTransform.position = this.targetBody.corePosition;
			this.indicatorTransform.localScale = Vector3.one * this.targetBody.bestFitRadius * 0.5f;

			Vector3 between = this.indicatorTransform.position - this.owner.transform.position;
			Quaternion rotation = Util.QuaternionSafeLookRotation(between) * Quaternion.Euler(0, 0, 45f);

			this.lineStartTransform.position = this.owner.transform.position;
			this.lineStartTransform.rotation = rotation;

			this.indicatorTransform.rotation = rotation;

			if (this.isTarget != this.indicatorTransform.gameObject.activeSelf) this.indicatorTransform.gameObject.SetActive(isTarget);
		}
		public GameObject owner;
		public CharacterBody targetBody;
		public bool isTarget;

		public Transform indicatorTransform;
		public Transform lineStartTransform;
	}
}
