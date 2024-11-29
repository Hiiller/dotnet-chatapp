using Microsoft.AspNetCore.SignalR.Client;
using Shared.MessageTypes;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ChatApp.Client.Services.Interfaces;
using System.Threading.Tasks;
using System.Reactive.Subjects;

namespace ChatApp.Client.Services
{
    //业务逻辑层，与 SignalR 服务端进行通信
    public class ChatService : IChatService
    {
        private const string CHAT_HUB_NAME = "chat";

        public IObservable<HubConnectionState> ConnectionState { get; private set; }

        public IObservable<string> ParticipantLoggedIn { get; private set; }

        public IObservable<string> ParticipantLoggedOut { get; private set; }

        public IObservable<MessagePayload> MessageReceived { get; private set; }


        public ObservableCollection<MessagePayload> Messages { get; private set; } = new ObservableCollection<MessagePayload>();


        //使用 HubConnection 与服务器通信，支持用户登录、注销和消息发送/接收等功能
        public ChatService(string serverUrl)
        {
            //connection = new HubConnectionBuilder().WithUrl($"{serverUrl}{CHAT_HUB_NAME}").Build();
            //connection.On<string>("UserLoggedIn", (userName) => participantLoggedInSubject.OnNext(userName));
            //connection.On<string>("UserLoggedOut", (userName) => participantLoggedOutSubject.OnNext(userName));

            //connection.On<MessagePayload>("NewMessage", (message) => ProcessNewMessage(message));


            //ParticipantLoggedIn = participantLoggedInSubject.AsObservable();
            //ParticipantLoggedOut = participantLoggedOutSubject.AsObservable();
            //MessageReceived = newMessageReceivedSubject.AsObservable();
            //ConnectionState = Observable.Interval(TimeSpan.FromMilliseconds(500))
            //                            .Select(x => connection.State)
            //                            .DistinctUntilChanged();
        }

        private void ProcessNewMessage(MessagePayload message)
        {
            Console.WriteLine($"{message.Type} Message Received");
            Messages.Add(message);
            newMessageReceivedSubject.OnNext(message);
        }

        //异步方法，启动与 SignalR 服务器的连接
        public async Task ConnectAsync()
        {
            await connection.StartAsync();
        }

        /*
         * 异步方法，用于通过 SignalR 调用服务器的 Login 方法，传入用户名和密码进行登录。
         * 登录后会调用 ProcessLogInResponse 方法处理登录响应。
         */
        public async Task<SuccessfulLoginResponse> LoginAsync(string username, string passcode)
        {
            var result = await connection.InvokeAsync<SuccessfulLoginResponse>("Login", username, passcode);
            ProcessLogInResponse(result);
            return result;
        }

        /*
        * 异步方法，用于注册并登录用户。
        * 先通过 RegisterAndLogIn 调用服务器方法，传入用户名和密码，然后处理登录响应。
        */
        public async Task<SuccessfulLoginResponse> RegisterAndLogIn(string username, string passcode)
        {
            var result = await connection.InvokeAsync<SuccessfulLoginResponse>("RegisterAndLogIn", username, passcode);
            ProcessLogInResponse(result);
            return result;
        }

        /*
         * 异步方法，用于将消息发送到 SignalR 服务器。
         * 如果消息对象中没有指定 AuthorUsername，则使用当前用户的用户名。然后将消息通过 InvokeAsync 方法发送到服务器。
         */
        public async Task SendMessageAsync(MessagePayload message)
        {
            if (string.IsNullOrEmpty(message.AuthorUsername))
                message.AuthorUsername = CurrentUser.UserName;

            await connection.InvokeAsync("SendMessage", message);
        }

        /*
         * 异步方法，用于注销用户，并清空本地存储的消息列表。
         */
        public async Task LogoutAsync()
        {
            await connection.InvokeAsync("Logout");
            Messages.Clear();
        }


        /*
         * 用于处理登录响应的私有方法。
         * 如果登录成功，返回一个 SuccessfulLoginResponse 对象，该对象包含用户信息和之前的消息。
         * 方法将用户信息保存到 currentUser 属性中，并将之前的消息添加到 Messages 列表中。
         */
        private void ProcessLogInResponse(SuccessfulLoginResponse slr)
        {
            currentUser = slr.User;
            if (slr.PreviousMessages != null)
            {
                foreach (var chat in slr.PreviousMessages) Messages.Add(chat);
            }
        }


        //事件流处理：
        internal User CurrentUser => currentUser;

        //Fields 
        private User currentUser;
        private HubConnection connection;
        //向订阅者发出关于消息接收、用户登录和用户注销的通知：
        private Subject<MessagePayload> newMessageReceivedSubject = new Subject<MessagePayload>();
        private Subject<string> participantLoggedOutSubject = new Subject<string>();
        private Subject<string> participantLoggedInSubject = new Subject<string>();
    }
}
