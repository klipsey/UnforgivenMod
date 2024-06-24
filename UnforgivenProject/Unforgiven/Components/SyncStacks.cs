using UnityEngine.Networking;
using R2API.Networking;
using R2API.Networking.Interfaces;
using UnityEngine;
using RoR2;

namespace UnforgivenMod.Unforgiven.Components
{
    public class SyncStacks : INetMessage
    {
        private NetworkInstanceId netId;
        private bool empowered;

        public SyncStacks()
        {
        }

        public SyncStacks(NetworkInstanceId netId, bool isNado)
        {
            this.netId = netId;
            this.empowered = isNado;
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

            bodyObject.GetComponent<UnforgivenController>().StackBehaviour(empowered);
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.netId);
            writer.Write(this.empowered);
        }
    }
}