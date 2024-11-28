using Avalonia.Controls;
using Avalonia;
using ChatApp.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Client.Controls
{
    //聊天气泡控件，用于显示聊天消息
    public class ChatBubble : ContentControl, ISelectable
    {
        public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<ChatBubble, bool>(nameof(IsSelected));

        public static readonly StyledProperty<ChatRoleType> RoleProperty = AvaloniaProperty.Register<ChatBubble, ChatRoleType>(nameof(Role));

        public static readonly StyledProperty<bool> IsReadProperty = AvaloniaProperty.Register<ChatBubble, bool>(nameof(IsRead));

        //表示该气泡是否被选中，选中后会设置 IsRead 为 true
        public bool IsRead
        {
            get => GetValue(IsReadProperty);
            set => SetValue(IsReadProperty, value);
        }

        //指定气泡的角色（发送者或接收者）
        public ChatRoleType Role
        {
            get => GetValue(RoleProperty);
            set => SetValue(RoleProperty, value);
        }

        //表示消息是否已读
        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set
            {
                SetValue(IsSelectedProperty, value);
                IsRead = true;
            }
        }
    }
}
