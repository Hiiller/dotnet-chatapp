using Microsoft.AspNetCore.SignalR.Client;
using System;
using Shared.MessageTypes;
using Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Client.Services.Interfaces
{
    public interface IChatService
    {
        IObservable<HubConnectionState> ConnectionState { get; }

        IObservable<string> ParticipantLoggedIn { get; }

        IObservable<string> ParticipantLoggedOut { get; }

        Task<SuccessfulLoginResponse> LoginAsync(string username, string passcode);

        Task<SuccessfulLoginResponse> RegisterAndLogIn(string username, string passcode);

        Task LogoutAsync();

        Task ConnectAsync();

        IObservable<MessagePayload> MessageReceived { get; }

        Task SendMessageAsync(MessagePayload message);
    }
}
