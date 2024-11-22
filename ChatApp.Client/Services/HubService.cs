using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace ChatApp.Client.Services
{
	public interface IHubService
	{
		Task ConnectAsync(string username, string chatroom);
		Task SendMessageAsync(string chatroom, string username, string message);
		event Action<string, string> MessageReceived;
	}

	public class HubService : IHubService
	{
		private HubConnection _connection;

		public event Action<string, string>? MessageReceived;

		public async Task ConnectAsync(string username, string chatroom)
		{
			// ���� HubConnection ʵ����ָ�� SignalR ���� URL
			_connection = new HubConnectionBuilder()
				.WithUrl("http://localhost:5005/chatHub")  // ע����� URL Ϊ��ȷ�ĵ�ַ
				.Build();

			// ���ý�����Ϣ���¼��������
			_connection.On<string, string>("ReceiveMessage", (user, message) =>
			{
				// ���� MessageReceived �¼������ݽ��յ�����Ϣ
				MessageReceived?.Invoke(user, message);
			});

			// ��������
			await _connection.StartAsync();

			// ����ָ��������
			await _connection.InvokeAsync("JoinRoom", username, chatroom);
		}

		// ������Ϣ��ָ��������
		public async Task SendMessageAsync(string chatroom, string username, string message)
		{
			// ���� SignalR ����˵� SendMessage ������������Ϣ
			await _connection.InvokeAsync("SendMessage", chatroom, username, message);
		}

		// �Ͽ��� SignalR ��������ӣ����˳�������
		public async Task DisconnectAsync(string username, string chatroom)
		{
			// ���� SignalR ����˵� LeaveRoom �������˳�������
			await _connection.InvokeAsync("LeaveRoom", username, chatroom);

			// ֹͣ����
			await _connection.StopAsync();
		}
	}
}
