using System.Net.WebSockets;

namespace std.WebSockets.Core
{
    /// <summary>
    /// WebSocket分组管理接口，提供分组管理和群发功能
    /// </summary>
    public interface IWebSocketGroupManager
    {
        /// <summary>
        /// 将连接添加到指定分组
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="groupName">分组名称</param>
        Task AddToGroupAsync(string connectionId, string groupName);
        
        /// <summary>
        /// 将连接从指定分组移除
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="groupName">分组名称</param>
        Task RemoveFromGroupAsync(string connectionId, string groupName);
        
        /// <summary>
        /// 将连接从所有分组移除
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        Task RemoveFromAllGroupsAsync(string connectionId);
        
        /// <summary>
        /// 向分组发送文本消息
        /// </summary>
        /// <param name="groupName">分组名称</param>
        /// <param name="message">消息文本</param>
        /// <returns>成功发送的连接数</returns>
        Task<int> SendToGroupAsync(string groupName, string message);
        
        /// <summary>
        /// 向分组发送二进制数据
        /// </summary>
        /// <param name="groupName">分组名称</param>
        /// <param name="binaryData">二进制数据</param>
        /// <returns>成功发送的连接数</returns>
        Task<int> SendToGroupAsync(string groupName, ArraySegment<byte> binaryData);
        
        /// <summary>
        /// 向分组发送对象（自动序列化为JSON）
        /// </summary>
        /// <param name="groupName">分组名称</param>
        /// <param name="obj">要发送的对象</param>
        /// <returns>成功发送的连接数</returns>
        Task<int> SendObjectToGroupAsync<T>(string groupName, T obj);
        
        /// <summary>
        /// 获取分组中的所有连接
        /// </summary>
        /// <param name="groupName">分组名称</param>
        /// <returns>连接集合</returns>
        IEnumerable<IWebSocketConnection> GetConnectionsInGroup(string groupName);
        
        /// <summary>
        /// 获取包含指定连接的所有分组
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>分组名称集合</returns>
        IEnumerable<string> GetGroupsForConnection(string connectionId);
        
        /// <summary>
        /// 获取所有分组名称
        /// </summary>
        /// <returns>分组名称集合</returns>
        IEnumerable<string> GetAllGroups();
        
        /// <summary>
        /// 获取指定分组中的连接数
        /// </summary>
        /// <param name="groupName">分组名称</param>
        /// <returns>连接数</returns>
        int GetConnectionCountInGroup(string groupName);
    }
} 