using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Server.Domain.Entities;
using ChatApp.Server.Application.DTOs;

namespace ChatApp.Server.Application.Mappers
{
    public class MessageMapper
    {
        // Map Message to MessageDto
        public static MessageDto ToDto(Message message)
        {
            return new MessageDto
            {
                id = message.Id,
                content = message.Content,
                attachmentUrl = message.AttachmentUrl,
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                groupId = message.GroupId,
                timestamp = message.Timestamp,
            };
        }
        
        // Map MessageDto to Message
        public static Message ToEntity(MessageDto messageDto)
        {
            // 规范化：将 Guid.Empty 视为 null，避免群聊/私聊字段同时赋值
            Guid? normalizedReceiverId = (messageDto.receiverId == null || messageDto.receiverId == Guid.Empty)
                ? null
                : messageDto.receiverId;
            Guid? normalizedGroupId = (messageDto.groupId == null || messageDto.groupId == Guid.Empty)
                ? null
                : messageDto.groupId;

            // 如果是群聊（有 groupId），强制 receiverId 为 null；如果是私聊（有 receiverId），强制 groupId 为 null
            if (normalizedGroupId != null)
            {
                normalizedReceiverId = null;
            }
            else if (normalizedReceiverId != null)
            {
                normalizedGroupId = null;
            }

            // 通过构造函数创建 Message 实体，并传入 timestamp，还会自动创建一个 Id
            return new Message(
                messageDto.senderId,           // SenderId
                messageDto.content,            // Content
                normalizedReceiverId,          // ReceiverId
                normalizedGroupId,             // GroupId
                messageDto.timestamp,          // Timestamp (通过 DTO 传递)
                false,
                messageDto.attachmentUrl       // 附件URL
            );
        }

        
    }
}
