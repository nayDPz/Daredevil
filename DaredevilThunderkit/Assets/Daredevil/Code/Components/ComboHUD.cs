using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Daredevil.Components
{
	public class ComboHUD : MonoBehaviour
	{
		public GameObject comboGauge;
		public Image comboFill;
		private ComboController comboController;

		private float maxOffset = 0.07f;
		private float minOffset = 0.05f;
		private float decayRate = 0.225f;
		private float offsetLow;
		private float offsetHigh;

		private HUD hud;

		public float e = 0.14f;
		public float d = 0.29f;
		public float c = 0.435f;
		public float b = 0.577f;
		public float a = 0.698f;
		public float s = 1f;

		private void Awake()
		{
			this.hud = GetComponent<HUD>();
		}

		public void Update()
		{
            if (this.hud != null && this.hud.targetBodyObject != null)
            {
                if (!this.comboController) this.comboController = this.hud.targetBodyObject.GetComponent<ComboController>();
                if (this.comboController)
                {
                    if (this.comboGauge)
                    {
                        float fillAmount;
                        int i = this.comboController.displayCombo;
                        switch (i)
                        {
                            case 1:
                                fillAmount = e;
                                break;
                            case 2:
                                fillAmount = d;
                                break;
                            case 3:
                                fillAmount = c;
                                break;
                            case 4:
                                fillAmount = b;
                                break;
                            case 5:
                                fillAmount = a;
                                break;
                            case 6:
                                fillAmount = s;
                                break;
                            default:
                                fillAmount = 0;
                                break;
                        }



                        this.offsetHigh -= this.decayRate * Time.fixedDeltaTime;
                        if (this.offsetHigh <= this.offsetLow)
                        {
                            this.RecalculateOffset();
                        }

                        if (i == 0 || i == 6 || this.comboController.drain)
                            this.comboFill.fillAmount = fillAmount;
                        else
                            this.comboFill.fillAmount = fillAmount + this.offsetHigh;
                        this.comboGauge.gameObject.SetActive(true);

                    }

                }
                else
                {
                    if (this.comboGauge)
                    {
                        this.comboGauge.gameObject.SetActive(false);
                    }
                }
            }
        }

		private void RecalculateOffset()
		{
			offsetHigh = Random.Range(0f, maxOffset);
			offsetLow = 0f - Random.Range(0f, minOffset);
		}
	}
}


