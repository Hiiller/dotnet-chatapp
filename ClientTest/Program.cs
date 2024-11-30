using System;
using System.Threading.Tasks;
using ChatApp.Client.Services;
using ChatApp.Server.Application.DTOs;
using Shared.Models;
using System.Net.Http;

class Program
{
    static async Task Main(string[] args)
    {
        // 配置 HttpClient 和 ChatService
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5005") };
        var chatService = new ChatService(httpClient);

        while (true)
        {
            Console.WriteLine("\n=== Chat Service Test Menu ===");
            Console.WriteLine("1. Register User");
            Console.WriteLine("2. Login User");
            Console.WriteLine("3. Get Recent Contacts");
            Console.WriteLine("4. Exit");
            Console.Write("Select an option: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await RegisterUser(chatService);
                    break;
                case "2":
                    await LoginUser(chatService);
                    break;
                case "3":
                    await GetRecentContacts(chatService);
                    break;
                case "4":
                    Console.WriteLine("Exiting...");
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private static async Task RegisterUser(ChatService chatService)
    {
        Console.WriteLine("\n=== Register User ===");
        Console.Write("Enter username: ");
        var username = Console.ReadLine();
        Console.Write("Enter password: ");
        var password = Console.ReadLine();

        var registerDto = new RegisterUserDto
        {
            Username = username,
            Password = password
        };

        var response = await chatService.RegisterUser(registerDto);

        if (response.connectionStatus)
        {
            Console.WriteLine($"Registration successful! User ID: {response.currentUserId}");
        }
        else
        {
            Console.WriteLine($"Registration failed. Error Code: {response.ErrorCode}");
        }
    }

    private static async Task LoginUser(ChatService chatService)
    {
        Console.WriteLine("\n=== Login User ===");
        Console.Write("Enter username: ");
        var username = Console.ReadLine();
        Console.Write("Enter password: ");
        var password = Console.ReadLine();

        var loginDto = new LoginUserDto
        {
            Username = username,
            Password = password
        };

        var response = await chatService.LoginUser(loginDto);

        if (response.connectionStatus)
        {
            Console.WriteLine($"Login successful! Welcome, {response.currentUsername}. User ID: {response.currentUserId}");
        }
        else
        {
            Console.WriteLine($"Login failed. Error Code: {response.ErrorCode}");
        }
    }

    private static async Task GetRecentContacts(ChatService chatService)
    {
        Console.WriteLine("\n=== Get Recent Contacts ===");
        Console.Write("Enter your user ID: ");
        if (!Guid.TryParse(Console.ReadLine(), out var userId))
        {
            Console.WriteLine("Invalid User ID format.");
            return;
        }

        var response = await chatService.GetRecentContacts(userId);

        if (response != null && response.Contacts.Count > 0)
        {
            Console.WriteLine("Recent Contacts:");
            foreach (var contact in response.Contacts)
            {
                Console.WriteLine($"Contact ID: {contact.Key}, Name: {contact.Value}");
            }
        }
        else
        {
            Console.WriteLine("No recent contacts found.");
        }
    }
}
