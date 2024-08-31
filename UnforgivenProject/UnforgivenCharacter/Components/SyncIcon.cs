using UnityEngine.Networking;
using R2API.Networking;
using R2API.Networking.Interfaces;
using UnityEngine;
using RoR2;
using UnforgivenMod.Unforgiven.Content;

namespace UnforgivenMod.Unforgiven.Components
{
    public class SyncIcon : INetMessage
    {
        private NetworkInstanceId netId;
        private bool empowered;

        public SyncIcon()
        {
        }

        public SyncIcon(NetworkInstanceId netId, bool empowered)
        {
            this.netId = netId;
            this.empowered = empowered;
        }

        public void Deserialize(NetworkReader reader)
        {
            this.netId = reader.ReadNetworkId();
            this.empowered = reader.ReadBoolean();
        }

        public void OnReceived()
        {
            GameObject bodyObject = Util.FindNetworkObject(this.netId);
            if (!bodyObject) return;

            CharacterBody characterBody = bodyObject.GetComponent<CharacterBody>();
            if(characterBody)
            {
                if (empowered)
                {
                    characterBody.skillLocator.secondary.skillDef.icon = UnforgivenAssets.secondaryEmpoweredIcon;
                }
                else
                {
                    characterBody.skillLocator.secondary.skillDef.icon = UnforgivenAssets.secondaryIcon;
                }
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.netId);
            writer.Write(this.empowered);
        }
    }
}