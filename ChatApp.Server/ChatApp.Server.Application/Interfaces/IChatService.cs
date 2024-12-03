
using ChatApp.Server.Application.DTOs;

namespace ChatApp.Server.Application.Interfaces
{
    public interface IChatService
    {
        /// <summary>
        /// 发送私聊消息。
        /// </summary>
        /// <param name="messageDto">发送者的ID。</param>
        /// <returns>异步操作任务。</returns>
        Task<MessageDto> SaveOnlineMessageAsync(MessageDto messageDto);
        
        Task<MessageDto>SaveOfflineMessageAsync(MessageDto messageDto);
        
        Task <Dictionary<Guid,string>>GetRecentContactsAsync(Guid userId);
        /// <summary>
        /// 获取用户的未读消息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<MessageDto>> GetUnreadMessagesAsync(Guid userId);

        /// <summary>
        /// 获取两个用户之间的所有私聊消息历史。
        /// </summary>
        /// <param name="user1Id">第一个用户的ID。</param>
        /// <param name="user2Id">第二个用户的ID。</param>
        /// <returns>两人之间的所有消息。</returns>
        Task<IEnumerable<MessageDto>> GetPrivateMessagesAsync(Guid user1Id, Guid user2Id);
        /// <summary>
        /// 标记消息为已读
        /// </summary>
        /// <param name="receiverId"></param>
        /// <param name="senderId"></param>
        Task MarkMessagesAsReadAsync(Guid receiverId, Guid senderId);
        
        /// <summary>
        /// 获取用户的已读消息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<MessageDto>> GetReadMessagesAsync(Guid userId);

        /// <summary>
        /// 返回用户的最近对话列表，每个对话只显示最新一条消息，用于构建 UI 的对话列表
        /// </summary>
        /// <param name="userId">用户的ID。</param>
        /// <returns>用户的对话对象列表。</returns>
        Task<IEnumerable<PrivateChatDto>> GetRecentChatsAsync(Guid userId);


        Task SetMessagetoUnread(MessageDto messagesDto);
    }
}