using UnityEngine.Networking;
using R2API.Networking;
using R2API.Networking.Interfaces;
using UnityEngine;
using RoR2;

namespace UnforgivenMod.Unforgiven.Components
{
    public class SyncWindExplosion : INetMessage
    {
        private NetworkInstanceId netId;
        private GameObject target;
        private bool monsoon;

        public SyncWindExplosion()
        {
        }

        public SyncWindExplosion(NetworkInstanceId netId, GameObject target, bool monsoon)
        {
            this.netId = netId;
            this.target = target;
            this.monsoon = monsoon;
        }

        public void Deserialize(NetworkReader reader)
        {
            this.netId = reader.ReadNetworkId();
            this.target = reader.ReadGameObject();
            this.monsoon = reader.ReadBoolean();
        }

        public void OnReceived()
        {
            GameObject bodyObject = Util.FindNetworkObject(this.netId);
            if (!bodyObject) return;

            GameObject effect = monsoon
                ? Content.UnforgivenSkinVFX.GetMonsoonEffect(Content.UnforgivenAssets.windExplosionEffect)
                : Content.UnforgivenAssets.windExplosionEffect;
            EffectManager.SpawnEffect(effect, new EffectData
            {
                origin = bodyObject.transform.position,
                rotation = Quaternion.identity,
                scale = 1f
            }, false);
            Util.PlaySound("sfx_unforgiven_nado_impact", bodyObject);
            bodyObject.AddComponent<WindExplosion>();
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.netId);
            writer.Write(this.target);
            writer.Write(this.monsoon);
        }
    }
}