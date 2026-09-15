using UnityEngine;
using UnityEngine.UI;

namespace UnforgivenMod.Unforgiven.Components
{
    public class PassiveShieldHudController : MonoBehaviour
    {
        public Image shieldBar;
        public Image trailingBar;

        private UnforgivenController source;

        public bool SetSource(UnforgivenController controller)
        {
            if (!controller || !shieldBar || !trailingBar)
            {
                Log.Error("Cannot initialize the shield HUD without its controller and fill images.");
                gameObject.SetActive(false);
                enabled = false;
                return false;
            }

            source = controller;
            RefreshDisplay(true);
            return true;
        }

        private void OnEnable()
        {
            if (source)
            {
                RefreshDisplay(true);
            }
        }

        private void Update()
        {
            if (!source)
            {
                gameObject.SetActive(false);
                return;
            }

            RefreshDisplay(false);
        }

        private void RefreshDisplay(bool immediate)
        {
            float fill = Mathf.Clamp01(source.shieldAmount / UnforgivenController.maxShieldGain);
            shieldBar.fillAmount = fill;
            trailingBar.fillAmount = immediate || fill >= 1f
                ? fill
                : Mathf.Lerp(trailingBar.fillAmount, fill, Time.deltaTime * 2f);
        }
    }
}
