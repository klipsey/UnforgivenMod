using UnityEngine;
using RoR2;

namespace UnforgivenMod.Unforgiven.Components
{
    public class WindExplosion : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (this.transform) this.transform.localScale = Vector3.zero;
        }
    }
}