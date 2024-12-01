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
                content = message.Content,
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                groupId = message.GroupId,
                timestamp = message.Timestamp
            };
        }
        
        // Map MessageDto to Message
        public static Message ToEntity(MessageDto messageDto)
        {
            // 通过构造函数创建 Message 实体，并传入 timestamp
            return new Message(
                messageDto.senderId,     // SenderId
                messageDto.content,      // Content
                messageDto.receiverId,   // ReceiverId
                messageDto.groupId,      // GroupId
                messageDto.timestamp     // Timestamp (通过 DTO 传递)
            );
        }

        
    }
}
