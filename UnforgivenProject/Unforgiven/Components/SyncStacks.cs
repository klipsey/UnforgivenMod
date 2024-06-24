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

        public SyncStacks()
        {
        }

        public SyncStacks(NetworkInstanceId netId)
        {
            this.netId = netId;
        }

        public void Deserialize(NetworkReader reader)
        {
            this.netId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            GameObject bodyObject = Util.FindNetworkObject(this.netId);
            if (!bodyObject) return;

            bodyObject.GetComponent<UnforgivenController>().StackBehaviour();
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.netId);
        }
    }
}