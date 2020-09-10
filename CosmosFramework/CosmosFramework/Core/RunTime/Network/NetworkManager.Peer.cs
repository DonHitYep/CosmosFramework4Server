using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Network
{
    public sealed partial class NetworkManager : Module<NetworkManager>
    {
        public int PeerCount { get { return clientPeerDict.Count; } }
        ConcurrentDictionary<long, IRemotePeer> clientPeerDict = new ConcurrentDictionary<long, IRemotePeer>();
        Action<IRemotePeer> peerConnectHandler;
        public event Action<IRemotePeer> PeerConnectEvent
        {
            add{peerConnectHandler += value;}
            remove
            {
                try{peerConnectHandler -= value;}
                catch (Exception e)
                {
                    Utility.Debug.LogError(e);
                }
            }
        }
        Action<IRemotePeer> peerDisconnectHandler;
        public event Action<IRemotePeer> PeerDisconnectEvent
        {
            add{peerDisconnectHandler += value;}
            remove
            {
                try{peerDisconnectHandler -= value;}
                catch (Exception e)
                {
                    Utility.Debug.LogError(e);
                }
            }
        }
        public bool TryGetValue(long key, out IRemotePeer value)
        {
            return clientPeerDict.TryGetValue(key, out value);
        }
        public bool ContainsKey(long key)
        {
            return clientPeerDict.ContainsKey(key);
        }
        public bool TryRemove(long key)
        {
            IRemotePeer peer;
            var result= clientPeerDict.TryRemove(key, out peer);
            if (result)
                peerDisconnectHandler?.Invoke(peer);
            return result;
        }
        public bool TryRemove(long key, out IRemotePeer peer)
        {
            return clientPeerDict.TryRemove(key, out peer);
        }
        public bool TryAdd(long key, IRemotePeer value)
        {
            var result =  clientPeerDict.TryAdd(key, value);
            if (result)
                peerConnectHandler?.Invoke(value);
            return result;
        }
        public bool TryUpdate(long key, IRemotePeer newValue, IRemotePeer comparsionValue)
        {
            var result= clientPeerDict.TryUpdate(key, newValue, comparsionValue);
            if (result)
            {
                peerConnectHandler?.Invoke(newValue);
                peerDisconnectHandler?.Invoke(comparsionValue);
            }
            return result;
        }
    }
}
