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
                Id = message.Id,
                Content = message.Content,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                GroupId = message.GroupId,
                Timestamp = message.Timestamp
            };
        }
        
        // Map MessageDto to Message
        public static Message ToEntity(MessageDto messageDto)
        {
            // 通过构造函数创建 Message 实体，并传入 timestamp
            return new Message(
                messageDto.SenderId,     // SenderId
                messageDto.Content,      // Content
                messageDto.ReceiverId,   // ReceiverId
                messageDto.GroupId,      // GroupId
                messageDto.Timestamp     // Timestamp (通过 DTO 传递)
            );
        }

        
    }
}
