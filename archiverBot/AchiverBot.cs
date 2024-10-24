using Discord;
using Discord.WebSocket;

namespace ArchiverBot
{
	class AchiverBot
	{
		DiscordSocketClient _client;
		ConfluenceService _confluenceService;

		public static async Task Main(string[] args)
		{
			var program = new AchiverBot();
			await program.RunBotAsync();
		}

		async Task RunBotAsync()
		{
			var config = new DiscordSocketConfig
			{
				GatewayIntents = GatewayIntents.Guilds | 
				                 GatewayIntents.GuildMessages | 
				                 GatewayIntents.MessageContent
			};

			_client = new DiscordSocketClient(config);

			_client.Log += Log;
			_client.MessageReceived += MessageReceived;

			const string PAGE_ID = "29786120";
			const string SPACE_KEY = "~712020dd497edf2bfd4f298aa0bb26724fb49e";
			const string BASE_URL = "https://koud-fi.atlassian.net/";
			_confluenceService = new ConfluenceService(
				baseUrl: BASE_URL,
				spaceKey: SPACE_KEY,
				pageId: PAGE_ID,
				apiToken: File.ReadAllText("/Users/software/Documents/confluence_api_token.txt"),
				email: File.ReadAllText("/Users/software/Documents/email.txt")
			);

			var token = File.ReadAllText("/Users/software/Documents/discord_myFirstBot_apikey.txt"); 

			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();

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
			if (message.Content.Contains("hello confluence"))
			{
				await _confluenceService.PostToConfluence("hello confluence", "hello hello");
			}
			if (message.Content.ToLower() == "!ping")
			{
				await message.Channel.SendMessageAsync("Pong!");
			}
		}
	}
}