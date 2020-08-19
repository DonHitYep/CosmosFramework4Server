using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;

namespace Cosmos
{
    /// <summary>
    /// 建立连接的远程对象管理器
    /// </summary>
    public class RemotePeerManager:ConcurrentSingleton<RemotePeerManager>
    {
        /// <summary>
        /// 建立连接的Peer容器；
        ///  sessionID,Peer
        /// </summary>
        ConcurrentDictionary<int, IRomotePeer> peerDict = new ConcurrentDictionary<int, IRomotePeer>();
        /// <summary>
        /// 添加Peer
        /// </summary>
        /// <param name="peer">peer对象</param>
        /// <returns>是否添加成功</returns>
        public bool TryAdd(IRomotePeer peer)
        {
            return peerDict.TryAdd(peer.SessionID, peer);
        }
        public bool TryAdd(int sessionID, IRomotePeer peer)
        {
            return peerDict.TryAdd(sessionID, peer);
        }
        public bool TryRemove(int sessionID)
        {
            IRomotePeer peer;
            return peerDict.TryRemove(sessionID, out peer);
        }
        /// <summary>
        /// 通过会话ID判断是否存在peer
        /// </summary>
        /// <param name="sessionID">会话ID</param>
        /// <returns>是否存在</returns>
        public bool Contains(int sessionID)
        {
            return peerDict.ContainsKey(sessionID);
        }
        /// <summary>
        /// 添加或者更新
        /// </summary>
        /// <param name="sessionID">会话ID</param>
        /// <param name="peer">peer对象</param>
        /// <returns>是否成功</returns>
        public bool AddOrUpdate(int sessionID, IRomotePeer peer)
        {
            IRomotePeer comparisonPeer;
            peerDict.TryGetValue(sessionID, out comparisonPeer);
            return peerDict.TryUpdate(sessionID, peer, comparisonPeer);
        }
        /// <summary>
        /// 通过会话ID获取peer对象
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="peer"></param>
        /// <returns></returns>
        public bool TryGetValue(int sessionID,out IRomotePeer peer)
        {
            return peerDict.TryGetValue(sessionID, out peer);
        }
    }
}
