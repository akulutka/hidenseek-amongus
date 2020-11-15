using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth.Customization;
using Microsoft.Extensions.Logging;
using System.Timers;

namespace Impostor.Plugins.HideNSeek.Handlers
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<HideNSeekPlugin> _logger;

        static Timer impostorTimer = new Timer();
        static Timer hideTimer = new Timer();
        static Api.Games.IGame game;

        public GameEventListener(ILogger<HideNSeekPlugin> logger)
        {
            _logger = logger;
            impostorTimer.Interval = 4 * 60 * 1000;
            hideTimer.Interval = 20 * 1000;
            impostorTimer.Elapsed += new ElapsedEventHandler(ImpostorElapsed);
            hideTimer.Elapsed += new ElapsedEventHandler(HideElapsed);
        }

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            _logger.LogInformation($"Game is starting.");
            game = e.Game;
            foreach (var player in e.Game.Players)
            {
                var info = player.Character.PlayerInfo;
                var isImpostor = info.IsImpostor;
                player.Character.SetHatAsync(HatType.NoHat);
                player.Character.SetPetAsync(PetType.NoPet);
                player.Character.SetSkinAsync(SkinType.None);
                if (isImpostor)
                {
                    player.Character.SetColorAsync(ColorType.Orange);
                    player.Character.SetNameAsync("Seeker");
                }
                else
                {
                    player.Character.SetColorAsync(ColorType.White);
                    player.Character.SetNameAsync("Hider");
                }
            }
            impostorTimer.Start();
            hideTimer.Start();
        }

        [EventListener]
        public void OnGameEnded(IGameEndedEvent e)
        {
            impostorTimer.Stop();
            hideTimer.Stop();
            _logger.LogInformation($"Game has ended.");
        }

        [EventListener]
        public void OnPlayerChat(IPlayerChatEvent e)
        {
            _logger.LogInformation($"{e.PlayerControl.PlayerInfo.PlayerName} said {e.Message}");
            if (e.Message == "killimpostor")
            {
                foreach (var player in e.Game.Players)
                {
                    var info = player.Character.PlayerInfo;
                    if (info.IsImpostor)
                    {
                        player.Character.SetMurderedAsync();
                    }
                }
            }
            if (e.Message.StartsWith("setround"))
            {
                if (e.Message.Split().Length > 1)
                {
                    string arg = e.Message.Split()[1];
                    if (int.TryParse(arg, out int seconds))
                    {
                        if (seconds > 0)
                        {
                            impostorTimer.Interval = seconds * 1000;
                        }
                    }
                }
            }
            if (e.Message.StartsWith("sethide"))
            {
                if (e.Message.Split().Length > 1)
                {
                    string arg = e.Message.Split()[1];
                    if (int.TryParse(arg, out int seconds))
                    {
                        if (seconds > 0)
                        {
                            hideTimer.Interval = seconds * 1000;
                        }
                    }
                }
            }
            if (e.Message.StartsWith("getround"))
            {
                e.ClientPlayer.Character.SendChatAsync((impostorTimer.Interval / 1000).ToString());
            }
            if (e.Message.StartsWith("gethide"))
            {
                e.ClientPlayer.Character.SendChatAsync((hideTimer.Interval / 1000).ToString());
            }
            if (e.Message.StartsWith("amonguscheats"))
            {
                e.ClientPlayer.Character.SetHatAsync(HatType.TopHat);
                e.ClientPlayer.Character.SetNameAsync("i am not cheating");
                e.ClientPlayer.Character.SetPetAsync(PetType.NoPet);
                e.ClientPlayer.Character.SetSkinAsync(SkinType.SuitB);
            }
            if (e.Message.StartsWith("Who is the impostor?"))
            {
                foreach (var player in e.Game.Players)
                {
                    if (player != e.ClientPlayer)
                    {
                        player.Character.SendChatAsync("I AM THE IMPOSTOR!");
                    }
                }
            }
        }

        [EventListener]
        public void OnPlayerMurder(IPlayerMurderEvent e)
        {
            int aliveCrewmates = 0;
            int aliveImpostors = 0;
            foreach (var player in e.Game.Players)
            {
                var info = player.Character.PlayerInfo;
                if (!info.IsImpostor && !info.IsDead)
                {
                    aliveCrewmates++;
                }
                if (info.IsImpostor && !info.IsDead)
                {
                    aliveImpostors++;
                }
            }
            if (aliveCrewmates == aliveImpostors + 1)
            {
                foreach (var player in e.Game.Players)
                {
                    var info = player.Character.PlayerInfo;
                    if (!info.IsImpostor)
                    {
                        player.Character.SetColorAsync(ColorType.Black);
                    }
                }
            }
        }

        public void ImpostorElapsed(object sender, ElapsedEventArgs e)
        {
            _logger.LogInformation("HIDERS WIN");
            foreach (var player in game.Players)
            {
                var info = player.Character.PlayerInfo;
                if (info.IsImpostor)
                {
                    player.Character.SetMurderedAsync();
                }
            }
            impostorTimer.Stop();
        }

        public void HideElapsed(object sender, ElapsedEventArgs e)
        {
            _logger.LogInformation("SEEKERS CAN MOVE NOW");
            foreach (var player in game.Players)
            {
                var info = player.Character.PlayerInfo;
                if (info.IsImpostor)
                {
                    player.Character.SetColorAsync(ColorType.Red);
                }
            }
            hideTimer.Stop();
        }
    }
}