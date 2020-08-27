using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
namespace CosmosFramework4Server
{
    public class PeerManager : Module<PeerManager>
    {
        public override void OnInitialization()
        {
            base.OnInitialization();
            NetworkEventCore.Instance.AddEventListener(0, PeerHandler);
            Utility.Debug.LogInfo("PeerManager OnInitialization"); ;
        }
        void PeerHandler(INetworkMessage netMsg)
        {
            Utility.Debug.LogWarning("PeerManager接收到OperationCode  为 0  的网络广播事件"); ;
        }
    }
}
