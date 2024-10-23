using Discord;
using Discord.WebSocket;

namespace ArchiverBot
{
	class Program
	{
		private DiscordSocketClient _client;

		public static async Task Main(string[] args)
		{
			var program = new Program();
			await program.RunBotAsync();
		}

		public async Task RunBotAsync()
		{
			_client = new DiscordSocketClient();

			_client.Log += Log;
			_client.MessageReceived += MessageReceived;

			Console.WriteLine("my sanity is intact");
			var token = File.ReadAllText("/Users/software/Documents/discord_myFirstBot_apikey.txt"); // Replace with your bot token

			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();

			// Block this task until the program is closed.
			await Task.Delay(-1);
		}

		Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		async Task MessageReceived(SocketMessage message)
		{
			if (message.Author.IsBot) return;
			Console.WriteLine(message.Content);
			if (message.Content.ToLower() == "!ping")
			{
				await message.Channel.SendMessageAsync("Pong!");
			}
		}
	}
}