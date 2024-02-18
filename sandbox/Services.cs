namespace Services;

using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Illuminate;

internal static partial class Services
{
    internal static void RegisterServices(DiscordClient client)
    {
        client.GuildAvailable += SetUpForEcolinguist;

        client.MessageCreated += CheckMessage;

        client.MessageCreated += HandleIntros;
    }

    static DiscordGuild? Ecolinguist;
    static DiscordChannel? FakeYtUploads;
    static DiscordChannel? YouTubeVideos;
    static DiscordChannel? IlluminatePlayground;
    static DiscordChannel? CarlPings;
    static DiscordChannel? Introductions;
    static DiscordRole? EcolinguistVideoRole;
    static DiscordRole? GermanicVideoRole;
    static DiscordRole? SlavicVideoRole;
    static DiscordRole? RomanceVideoRole;
    static DiscordRole? PolishVideoRole;
    static DiscordRole? IntroPingRole;
    static DiscordUser? CarlBot;
    static DiscordEmoji? Sparkles;
    static DiscordEmoji? VideoYes;
    static DiscordEmoji? VideoNo;
    static DiscordEmoji? StreamYes;
    static DiscordEmoji? GreenCheck;

    static bool setupFinished;
    static async Task SetUpForEcolinguist(DiscordClient client, GuildCreateEventArgs e)
    {
        if (e.Guild.Name == "Ecolinguist")
        {
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
            GreenCheck = DiscordEmoji.FromName(client, ":welcome:");


            //done
            setupFinished = true;
        }
    }

    static readonly HttpClient httpClient = new(new HttpClientHandler() { AllowAutoRedirect = false }, true) { BaseAddress = new("https://www.youtube.com/shorts/") };
    static readonly Random random = new();

    static Dictionary<string, string> mentions = new();
    static List<string> phrases = new() {
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

    static async Task CheckMessage(DiscordClient client, MessageCreateEventArgs e)
    {
        if (!setupFinished || e.Author == client.CurrentUser) return;
        Console.WriteLine("Message received. Not ours, and client is ready.");

        DiscordMessage msg = e.Message;
        if (msg.Channel != FakeYtUploads!) return;
        Console.WriteLine("FakeUploadChannelMessage");

        //Step 1: Regex Match Link + name from dictionary
        Match msgMatch = messageMatcher().Match(msg.Content);
        if (!msgMatch.Success) return;
        await msg.CreateReactionAsync(Sparkles!);
        string vidId = msgMatch.Groups["VideoID"].Value;
        string channelName = msgMatch.Groups["channelName"].Value;

        //Step 2: HttpClient to check if shorts
        HttpResponseMessage ytResponse = await httpClient.GetAsync(vidId);
        if (ytResponse.IsSuccessStatusCode)
        {
            await msg.CreateReactionAsync(VideoNo!);
            return;
        }

        await msg.CreateReactionAsync(VideoYes!);

        //Step 2.1: Check for livestream
        var request = youtubeService.Videos.List("liveStreamingDetails");
        request.Id = vidId;
        request.MaxResults = 50;
        var response = await request.ExecuteAsync();
        if (response.Items[0].LiveStreamingDetails is not null)
        {
            await msg.CreateReactionAsync(StreamYes!);
            await client.SendMessageAsync(FakeYtUploads!, "itssss a stream!! me no notifyin noone");
            await client.SendMessageAsync(FakeYtUploads!, "((i love how one can clearly see who ~~wrote the code for this~~ put the words into my mouth here XD ehehe >:3 ))");
            return;
        }

        //Step 3: Post Message
        await client.SendMessageAsync(YouTubeVideos!, $"{mentions[channelName]} {phrases[random.Next(phrases.Count)]} https://youtu.be/{vidId}");
    }

    static YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
    {
        ApiKey = Secrets.YtApiKey,
        ApplicationName = nameof(Illuminate)
    });

    [GeneratedRegex("^https://youtu.be/(?<VideoID>[^ ]+) (?<channelName>slavic|germanic|ecolinguist|polish|romance)$", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    private static partial Regex messageMatcher();

    static async Task HandleIntros(DiscordClient client, MessageCreateEventArgs e)
    {
        if (!setupFinished || e.Author == client.CurrentUser) return;
        Console.WriteLine("Message received. Not ours, and client is ready.");

        DiscordMessage msg = e.Message;
        if (msg.Channel != Introductions!) return;
        Console.WriteLine("IntroductionsChannelMessage");

        //// msg not long enough
        //if (msg.Content.Length <= 130)
        //{
        //    await msg.DeleteAsync();
        //    await msg.RespondAsync();

        //    return;
        //    // delete msg, funeral msg, "msg too short pls prolongue + copypaste og msg"
        //}

        await CarlPings!.SendMessageAsync($"{IntroPingRole!.Mention} new intro");
    }
}

file enum RoleIDs:ulong {
    EcolinguistVideoRole = 1101849321349070858,
    GermanicVideoRole = 1101850559016869968,
    SlavicVideoRole = 1101850758607020052,
    RomanceVideoRole = 1101850296587649114,
    PolishVideoRole = 1101850816056406047,
    IntroPingRole = 1208816932019634237,
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