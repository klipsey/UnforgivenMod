using UnityEngine;
using UnityEngine.UI;
using RoR2;
using RoR2.UI;
using UnforgivenMod.Unforgiven.Components;
using UnforgivenMod.Unforgiven.Content;

namespace UnforgivenMod.Unforgiven.Components
{
    public class PassiveShieldHudController : MonoBehaviour
    {
        public HUD targetHUD;
        public UnforgivenController UnforgivenController;

        public LanguageTextMeshController targetText;
        public GameObject durationDisplay;
        public Image durationBar;
        public Image durationBarColor;

        private void Start()
        {
            this.UnforgivenController = this.targetHUD?.targetBodyObject?.GetComponent<UnforgivenController>();
            this.UnforgivenController.onShieldChange += SetDisplay;

            this.durationDisplay.SetActive(false);
            SetDisplay();
        }

        private void OnDestroy()
        {
            if (this.UnforgivenController) this.UnforgivenController.onShieldChange -= SetDisplay;

            this.targetText.token = string.Empty;
            this.durationDisplay.SetActive(false);
            GameObject.Destroy(this.durationDisplay);
        }

        private void Update()
        {
            if (targetText.token != string.Empty) { targetText.token = string.Empty; }

            if (this.UnforgivenController && this.UnforgivenController.shieldAmount >= 0f)
            {
                float fill;
                fill = Util.Remap(this.UnforgivenController.shieldAmount, 0f, 100f, 0f, 1f);

                if (this.durationBarColor)
                {
                    if (fill >= 1f) this.durationBarColor.fillAmount = 1f;
                    this.durationBarColor.fillAmount = Mathf.Lerp(this.durationBarColor.fillAmount, fill, Time.deltaTime * 2f);
                }

                this.durationBar.fillAmount = fill;
            }
        }

        private void SetDisplay()
        {
            if (this.UnforgivenController)
            {
                this.durationDisplay.SetActive(true);
                this.targetText.token = string.Empty;

                this.durationBar.color = UnforgivenAssets.unforgivenColor;
            }
            else
            {
                this.durationDisplay.SetActive(false);
            }
        }
    }
}
