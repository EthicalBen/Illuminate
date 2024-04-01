namespace Services;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;

using Illuminate;

using Sandbox;

using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

internal static partial class Services {
	internal static void RegisterServices(DiscordClient client) {
		client.GuildAvailable += SetUpForEcolinguist;

		client.MessageCreated += CheckMessage;

		client.MessageCreated += HandleIntroCreated;

		client.MessageUpdated += HandleEditedIntros;

		client.MessageReactionAdded += HandleIntroReactionAdded;
	}

	private static DiscordGuild? Ecolinguist;
	private static DiscordChannel? FakeYtUploads;
	private static DiscordChannel? YouTubeVideos;
	private static DiscordChannel? IlluminatePlayground;
	private static DiscordChannel? CarlPings;
	private static DiscordChannel? Introductions;
	private static DiscordRole? EcolinguistVideoRole;
	private static DiscordRole? GermanicVideoRole;
	private static DiscordRole? SlavicVideoRole;
	private static DiscordRole? RomanceVideoRole;
	private static DiscordRole? PolishVideoRole;
	private static DiscordRole? IntroPingRole;
	private static DiscordRole? MemberRole;
	private static DiscordRole? UnverifiedRole;
	private static DiscordRole? SeniorModeratorRole;
	private static DiscordUser? CarlBot;
	private static DiscordEmoji? Sparkles;
	private static DiscordEmoji? VideoYes;
	private static DiscordEmoji? VideoNo;
	private static DiscordEmoji? StreamYes;
	private static DiscordEmoji? CheckMark;
	private static DiscordEmoji? ExclamationMark;
	private static DiscordEmoji? QuestionMark;
	private static bool setupFinished;

	private static async Task SetUpForEcolinguist(DiscordClient client, GuildCreateEventArgs e) {
		if(e.Guild.Name == "Ecolinguist") {
			//Server
			Ecolinguist = e.Guild;

			//Channels
			FakeYtUploads = Ecolinguist.Channels.FirstOrDefault(guild => guild.Key == (ulong)ChannelIDs.FakeYtUploads).Value;
			YouTubeVideos = Ecolinguist.Channels.FirstOrDefault(guild => guild.Key == (ulong)ChannelIDs.YouTubeVideos).Value;
			IlluminatePlayground = Ecolinguist.Channels.FirstOrDefault(guild => guild.Key == (ulong)ChannelIDs.IlluminatePlayground).Value;
			Introductions = Ecolinguist.Channels.FirstOrDefault(guild => guild.Key == (ulong)ChannelIDs.Introductions).Value;
			CarlPings = Ecolinguist.Channels.FirstOrDefault(guild => guild.Key == (ulong)ChannelIDs.CarlPings).Value;


			//Roles
			EcolinguistVideoRole = Ecolinguist.Roles.FirstOrDefault(guild => guild.Key == (ulong)RoleIDs.EcolinguistVideoRole).Value;
			GermanicVideoRole = Ecolinguist.Roles.FirstOrDefault(guild => guild.Key == (ulong)RoleIDs.GermanicVideoRole).Value;
			SlavicVideoRole = Ecolinguist.Roles.FirstOrDefault(guild => guild.Key == (ulong)RoleIDs.SlavicVideoRole).Value;
			RomanceVideoRole = Ecolinguist.Roles.FirstOrDefault(guild => guild.Key == (ulong)RoleIDs.RomanceVideoRole).Value;
			PolishVideoRole = Ecolinguist.Roles.FirstOrDefault(guild => guild.Key == (ulong)RoleIDs.PolishVideoRole).Value;
			IntroPingRole = Ecolinguist.Roles.FirstOrDefault(guild => guild.Key == (ulong)RoleIDs.IntroPingRole).Value;
			MemberRole = Ecolinguist.Roles.FirstOrDefault(guild => guild.Key == (ulong)RoleIDs.MemberRole).Value;
			UnverifiedRole = Ecolinguist.Roles.FirstOrDefault(guild => guild.Key == (ulong)RoleIDs.UnverifiedRole).Value;
			SeniorModeratorRole = Ecolinguist.Roles.FirstOrDefault(guild => guild.Key == (ulong)RoleIDs.SeniorModeratorRole).Value;

			//Dict
			/*
			messages["slavic"] = $"{SlavicVideoRole.Mention} Norbert uploaded a video! ";
			messages["germanic"] = $"{GermanicVideoRole.Mention} Norbert uploaded a video! ";
			messages["ecolinguist"] = $"{EcolinguistVideoRole.Mention} Norbert uploaded a video! ";
			messages["polish"] = $"{PolishVideoRole.Mention} Norbert uploaded a video! ";
			messages["romance"] = $"{RomanceVideoRole.Mention} Norbert uploaded a video! ";
			*/

			mentions["slavic"] = SlavicVideoRole.Mention;
			mentions["germanic"] = GermanicVideoRole.Mention;
			mentions["ecolinguist"] = EcolinguistVideoRole.Mention;
			mentions["polish"] = PolishVideoRole.Mention;
			mentions["romance"] = RomanceVideoRole.Mention;

			//Users
			CarlBot = await client.GetUserAsync((ulong)UserIDs.CarlBot);

			//Emojis
			Sparkles = DiscordEmoji.FromName(client, ":sparkles:");
			VideoYes = DiscordEmoji.FromName(client, ":ok_hand:");
			VideoNo = DiscordEmoji.FromName(client, ":scream_cat:");
			StreamYes = DiscordEmoji.FromName(client, ":thumbsdown:");
			CheckMark = DiscordEmoji.FromName(client, ":eco_check_mark:");
			ExclamationMark = DiscordEmoji.FromName(client, ":eco_exclamation_mark:");
			QuestionMark = DiscordEmoji.FromName(client, ":eco_question_mark:");


			//done
			setupFinished = true;
		}
	}

	private static readonly HttpClient httpClient = new(new HttpClientHandler() { AllowAutoRedirect = false }, true) { BaseAddress = new("https://www.youtube.com/shorts/") };
	private static readonly Random random = new();
	private static readonly Dictionary<string, string> mentions = new();
	private static readonly List<string> phrases = new() {
		" Norbert uploaded a video! ",
		" A new video just dropped! ",
		" Norbert just uploaded a new video! ",
		" Ding! New video, fresh from the oven! ",
		" Attention, everyone! Norbert has just unleashed a fresh video! ",
		" Calling all fans! Norbert's latest video is now live! ",
		" Hold onto your seats! Norbert has dropped a brand new video! ",
		" Breaking news! Norbert's newest creation is up on YouTube! ",
		" Alert! Alert! Norbert just uploaded an exciting new video! ",
		" Gather 'round, folks! Norbert's latest masterpiece has arrived! ",
		" Drumroll, please! Norbert's newest upload is here! ",
		" It's here! Norbert just released a fresh video for us to enjoy! ",
		" Guess what? Norbert just hit the 'upload' button! ",
		" Big news, everyone! Norbert just unveiled a new video! ",
		" Ready, set, watch! Norbert's newest YouTube upload is live! ",
		" Pop the popcorn and get comfy! Norbert has a new video for us! ",
		" Hold your breath, because Norbert's latest video is out! ",
		" It's a bird, it's a plane, no… it's Norbert's latest video! ",
		" Your wait is over! Norbert just released a new video! ",
	};

	private static async Task CheckMessage(DiscordClient client, MessageCreateEventArgs e) {
		if(!setupFinished || e.Author == client.CurrentUser) return;
		Console.WriteLine("Message received. Not ours, and client is ready.");

		DiscordMessage msg = e.Message;
		if(msg.Channel != FakeYtUploads!) return;
		Console.WriteLine("FakeUploadChannelMessage");

		e.Handled = true;

		//Step 1: Regex Match Link + name from dictionary
		Match msgMatch = messageMatcher().Match(msg.Content);
		if(!msgMatch.Success) return;
		await msg.CreateReactionAsync(Sparkles!);
		string vidId = msgMatch.Groups["VideoID"].Value;
		string channelName = msgMatch.Groups["channelName"].Value;

		//Step 2: HttpClient to check if shorts
		HttpResponseMessage ytResponse = await httpClient.GetAsync(vidId);
		if(ytResponse.IsSuccessStatusCode) {
			await msg.CreateReactionAsync(VideoNo!);
			return;
		}

		await msg.CreateReactionAsync(VideoYes!);

		//Step 2.1: Check for livestream
		VideosResource.ListRequest request = youtubeService.Videos.List("liveStreamingDetails");
		request.Id = vidId;
		request.MaxResults = 50;
		Google.Apis.YouTube.v3.Data.VideoListResponse response = await request.ExecuteAsync();
		if(response.Items[0].LiveStreamingDetails is not null) {
			await msg.CreateReactionAsync(StreamYes!);
			await client.SendMessageAsync(FakeYtUploads!, "itssss a stream!! me no notifyin noone");
			await client.SendMessageAsync(FakeYtUploads!, "((i love how one can clearly see who ~~wrote the code for this~~ put the words into my mouth here XD ehehe >:3 ))");
			return;
		}

		//Step 3: Post Message
		await client.SendMessageAsync(YouTubeVideos!, $"{mentions[channelName]} {phrases[random.Next(phrases.Count)]} https://youtu.be/{vidId}");
	}

	private static readonly YouTubeService youtubeService = new(new BaseClientService.Initializer() {
		ApiKey = Secrets.YtApiKey,
		ApplicationName = nameof(Illuminate)
	});

	[GeneratedRegex("^https://youtu.be/(?<VideoID>[^ ]+) (?<channelName>slavic|germanic|ecolinguist|polish|romance)$", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
	private static partial Regex messageMatcher();

	private static readonly IntroTaskList runningDeletionTasks = new();


	private static async Task HandleIntroCreated(DiscordClient client, MessageCreateEventArgs e) {
		if(!setupFinished || e.Author == client.CurrentUser || e.Message.Channel != IlluminatePlayground!) return; // TODO - CHANGE CHANNEL
		Console.WriteLine("Message received. Not ours, client is ready, correct channel.");
		e.Handled = true;

		//skip this all if a member is already verified (has member role)
		if((await e.Author.ConvertToMember(Ecolinguist!)).Roles.Contains(MemberRole)) return;

		// msg not long enough
		if(e.Message.Content.Length < 150) {
			CancellationTokenSource cancellationTokenSource = new();
			DiscordMessage statusMessage = await CreateStatusMessageAsync(e.Message);
			await e.Message.CreateReactionAsync(ExclamationMark!);
			Task handleIntro = DeleteAfterDelayAsync(client, e.Message, cancellationTokenSource.Token, statusMessage);
			runningDeletionTasks.Add(e.Message, handleIntro, cancellationTokenSource, statusMessage);
		} else {
			await AcceptMessageAsync(client, e.Message);
		}
	}


	private static async Task HandleIntroReactionAdded(DiscordClient client, MessageReactionAddEventArgs e) {
		if(!setupFinished || e.User == client.CurrentUser || e.Channel != IlluminatePlayground!) return; // TODO - CHANGE CHANNEL
		Console.WriteLine("Reaction received. Not ours, client is ready, correct channel.");

		e.Handled = true;

		if(
			e.Emoji != CheckMark! && e.Emoji != QuestionMark!
			|| !await HasIlluminateReactionAsync(client, e.Message, new[] { CheckMark!, QuestionMark! })
			|| !(await e.User.ConvertToMember(Ecolinguist!)).Roles.Contains(SeniorModeratorRole!)
		) return;

		if(runningDeletionTasks.Contains(e.User)) {
			ImmutableList<IntroTaskData> data = runningDeletionTasks[e.User]!;
			await Task.WhenAll(
				data.Select(
					async d => {
						d.TokenSource.Cancel();
						await SafeDeleteAsync(d.StatusMessage);
					}
				)
			);
			runningDeletionTasks.Remove(e.User);
		}

		if(e.Emoji == QuestionMark!) {
			await e.Message.DeleteReactionAsync(QuestionMark!, e.User);
			await e.Message.DeleteReactionAsync(QuestionMark!, client.CurrentUser);
			return;
		}

		if(e.Emoji == CheckMark!) {
			DiscordMember newcomer = await e.Message.Author.ConvertToMember(Ecolinguist!);
			await newcomer.RevokeRoleAsync(UnverifiedRole!);
			await newcomer.GrantRoleAsync(MemberRole!);
			return;
		}
	}

	private static async Task<DiscordMessage> CreateStatusMessageAsync(DiscordMessage message, string? content = null) {
		content ??= $"Hey {message.Author.Username}, sadly your message is too short and it will be deleted shortly. Please copy the contents if you want to edit and re-post it. We require the intros to be at least 150 characters long, to prevent unwanted bots from entering the server.";
		return await message.RespondAsync(content);
	}

	private static async Task UpdateStatusMessageAsync(DiscordMessage statusMessage, string? newContent = null) {
		newContent ??= $"Hey {statusMessage.Author.Username}, sadly your message is still too short and it will be deleted shortly. Please copy the contents if you want to edit and re-post it. We require the intros to be at least 150 characters long, to prevent unwanted bots from entering the server.";
		await statusMessage.ModifyAsync(newContent);
	}

	private static async Task DeleteAfterDelayAsync(DiscordClient client, DiscordMessage message, CancellationToken cancellationToken, DiscordMessage? statusMessage = null) {
		await Task.Delay(3 * 60 * 1000, cancellationToken);
		if(cancellationToken.IsCancellationRequested) return;

		DiscordMessage? msg = await IlluminatePlayground!.TryGetMessageAsync(message.Id);
		if(msg is null) return;

		if(msg.Content.Length >= 150) {
			await AcceptMessageAsync(client, msg);
			return;
		}

		await message.DeleteAsync();
		await SafeDeleteAsync(statusMessage);
	}


	private static async Task SafeDeleteAsync(DiscordMessage? msg) {
		try {
			if(msg is null) return;
			await msg.DeleteAsync();
		} catch { }
	}


	private static async Task AcceptMessageAsync(DiscordClient client, DiscordMessage msg) {
		await msg.CreateReactionAsync(CheckMark!);
		await client.SendMessageAsync(
			CarlPings!,
			$"""
			{IntroPingRole!.Mention}
			New intro by {msg.Author.Mention} is ready for verification!
			{msg.JumpLink}
			"""
		);
	}
	//Notes:
	// TODO - HANDLE EDITED INTROS, REMOVE EXCLAMATION MARK

	// some ideas from members:
	// string responseContent = $"Hey {e.Author.Username}, your introduction is too short! Please write a new, longer one, or edit the old one to be long enough. You have 3 minutes before your intro gets deleted.";
	// string responseContent = $"Hey {e.Author.Username}, your introduction is too short! It will be deleted shortly. Please copy the contents if you want to edit and re-post it. We require the intros to be at least 150 characters long, to prevent unwanted bots from entering the server.";
	// "The message will be deleted shortly, please copy the contents if you don't want to retype it"
	// "Sadly your message is too short and it will have to be deleted. Please, copy the contents of your message if you want to edit and re-post it"

	// TODO - TELL The Author know: THE MSG WILL DIP IF THEY SEND A NEW MSG 
	// wait for 3 min before deleting stuff
	// for NEW intros only (for now)



	private static async Task HandleEditedIntros(DiscordClient client, MessageUpdateEventArgs e) {
		if(!setupFinished || e.Author == client.CurrentUser || e.Channel != IlluminatePlayground!) return; // TODO - CHANGE CHANNEL
		Console.WriteLine("IntroductionsChannelMessage");


		DiscordMember author = await e.Message.Author.ConvertToMember(Ecolinguist!);
		bool isVerified = author.Roles.Contains(MemberRole!);
		bool isIntroMessage = await HasIlluminateReactionAsync(client, e.Message, new[] { CheckMark!, QuestionMark!, ExclamationMark! });
		bool isLongEnough = e.Message.Content.Length >= 150;
		bool wasLongEnough = e.MessageBefore.Content.Length >= 150;
		/*
		// not verified, isNOTintroMsg, short -> long :: msg was short but now is long enough
	✓		- return;
		// not verified, isNOTintroMsg, long -> short :: msg was long enoug but now is short
	✓		- return;
		// not verified, isNOTintroMsg, short -> short :: msg was short and still is short
	✓		- return;
		// not verified, isNOTintroMsg, long -> long :: msg was long and still is long
	✓		- return;
		// verified, isNOTintroMsg, short -> long :: msg was short but now is long enough
	✓		- return;
		// verified, isNOTintroMsg, long -> short :: msg was long enoug but now is short
	✓		- return;
		// verified, isNOTintroMsg, short -> short :: msg was short and still is short
	✓		- return;
		// verified, isNOTintroMsg, long -> long :: msg was long and still is long
	✓		- return;
		*/
		if(!isIntroMessage) return;
		//Is Intro Message

		/*
		// not verified, short -> long :: msg was short but now is long enough
			- kill 3min timer (is always within the timer), delete automsg (msg was too short), pingNewIntro, remove "!", add "check"
		// not verified, long -> short :: msg was long enoug but now is short
			- start 3min timer, send automsg, remove "check", add "!"
		// not verified, short -> short :: msg was short and still is short
			- reset 3min timer (is always within the timer), update automsg (STILL short)
		// not verified, long -> long :: msg was long and still is long
			- return;
		// verified, short -> long :: msg was short but now is long enough
			- return; 
		// verified, long -> short :: msg was long enoug but now is short
			- trigger reverification (ping a role (autoping?) for edited intros w a hyperlink), react w a "?" (mod has to rereact for it to dip - signifies that the intro is ok)
		// verified, short -> short :: msg was short and still is short
			- trigger reverification (ping a role (autoping?) for edited intros w a hyperlink), react w a "?" (mod has to rereact for it to dip - signifies that the intro is ok)
		// verified, long -> long :: msg was long and still is long
			- return;
		*/
		switch((isVerified, wasLongEnough, isLongEnough)) {
			case (false, false, false): {
					//reset timer, update message
					CancellationTokenSource tokenSource = new();
					IntroTaskData? data = runningDeletionTasks[e.Message];
					DiscordMessage statusMessage;
					data?.TokenSource.Cancel();
					if(data?.StatusMessage is not null) {
						statusMessage = data.StatusMessage;
						await UpdateStatusMessageAsync(statusMessage);
					} else {
						statusMessage = await CreateStatusMessageAsync(e.Message);
					}
					Task handleIntro = DeleteAfterDelayAsync(client, e.Message, tokenSource.Token, statusMessage);
					runningDeletionTasks.UpdateTask(e.Message, handleIntro, tokenSource);
					break;
				}
			case (false, false, true): {
					//accept message, change reactions, delete timer, delete status message
					await AcceptMessageAsync(client, e.Message);
					IntroTaskData? data = runningDeletionTasks[e.Message];
					data?.TokenSource.Cancel();
					runningDeletionTasks.Remove(e.Message);
					await SafeDeleteAsync(data?.StatusMessage);
					await e.Message.CreateReactionAsync(CheckMark!);
					await e.Message.DeleteReactionAsync(ExclamationMark!, client.CurrentUser);
					break;
				}
			case (false, true, false): {
					//start timer, change reactions, send status message
					CancellationTokenSource tokenSource = new();
					DiscordMessage statusMessage = await CreateStatusMessageAsync(e.Message);
					Task handleIntro = DeleteAfterDelayAsync(client, e.Message, tokenSource.Token, statusMessage);
					runningDeletionTasks.Add(e.Message, handleIntro, tokenSource, statusMessage);
					await e.Message.CreateReactionAsync(ExclamationMark!);
					await e.Message.DeleteReactionAsync(CheckMark!, client.CurrentUser);
					break;
				}
			case (true, _, false): {
					//add question mark reaction, send ping
					await e.Message.CreateReactionAsync(QuestionMark!);
					await client.SendMessageAsync(
						CarlPings!,
						$"""
						{IntroPingRole!.Mention}
						{e.Author.Mention} has edited their introduction and it needs to be re-verified!
						{e.Message.JumpLink}
						"""
					);
					break;
				}
			case (false, true, true):
			case (true, _, true):
				return;
		}
	}


	private static async Task<bool> HasIlluminateReactionAsync(DiscordClient client, DiscordMessage msg, params DiscordEmoji[] emojis) {
		IEnumerable<Task<IReadOnlyList<DiscordUser>>> emojiTasks = emojis.Select(async emoji => await msg.GetReactionsAsync(emoji));
		await Task.WhenAll(emojiTasks);
		IEnumerable<DiscordUser> users = emojiTasks.SelectMany(task => task.Result);
		return users.Any(user => user == client.CurrentUser);
	}
}

file enum RoleIDs:ulong {
	EcolinguistVideoRole = 1101849321349070858,
	GermanicVideoRole = 1101850559016869968,
	SlavicVideoRole = 1101850758607020052,
	RomanceVideoRole = 1101850296587649114,
	PolishVideoRole = 1101850816056406047,
	IntroPingRole = 1208816932019634237,
	MemberRole = 863414845897179146,
	UnverifiedRole = 1204208658305646623,
	SeniorModeratorRole = 992444492823146567,
}


file enum ChannelIDs:ulong {
	FakeYtUploads = 1065596986847399936,
	YouTubeVideos = 861223379076251658,
	IlluminatePlayground = 875797784354226186,
	CarlPings = 937314762218422273,
	Introductions = 1201538093866549288,
}


file enum UserIDs:ulong {
	CarlBot = 235148962103951360,
}