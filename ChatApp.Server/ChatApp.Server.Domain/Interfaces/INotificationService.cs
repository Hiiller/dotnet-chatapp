using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Server.Domain.Entities;

namespace ChatApp.Server.Domain.Interfaces
{
    public interface INotificationService
    {
        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="receiverId">接收者的用户ID</param>
        /// <param name="content">通知的内容</param>
        /// <param name="senderId">发送者的用户ID（可选）</param>
        /// <returns>异步操作</returns>
        Task SendNotificationAsync(Guid receiverId, string content, Guid? senderId = null);

        /// <summary>
        /// 获取某个用户的所有通知
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户的通知列表</returns>
        Task<IEnumerable<Notification>> GetNotificationsForUserAsync(Guid userId);

        /// <summary>
        /// 根据ID获取特定通知
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        /// <returns>通知实例</returns>
        Task<Notification?> GetNotificationByIdAsync(Guid notificationId);

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        /// <returns>异步操作</returns>
        Task MarkAsReadAsync(Guid notificationId);

        /// <summary>
        /// 删除通知
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        /// <returns>异步操作</returns>
        Task DeleteNotificationAsync(Guid notificationId);
    }
}