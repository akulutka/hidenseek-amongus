using System;
using System.Threading.Tasks;
using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Impostor.Plugins.HideNSeek.Handlers;
using Microsoft.Extensions.Logging;

namespace Impostor.Plugins.HideNSeek
{
    [ImpostorPlugin(
        package: "me.shoraii.hidenseek",
        name: "HideNSeek",
        author: "shoraii",
        version: "0.1.0")]
    public class HideNSeekPlugin : PluginBase
    {
        private readonly ILogger<HideNSeekPlugin> _logger;
        private readonly IEventManager _eventManager;
        private IDisposable _unregister;
        public HideNSeekPlugin(ILogger<HideNSeekPlugin> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        public override ValueTask EnableAsync()
        {
            _logger.LogInformation("HideNSeek is being enabled.");
            _unregister = _eventManager.RegisterListener(new GameEventListener(_logger));
            return default;
        }

        public override ValueTask DisableAsync()
        {
            _logger.LogInformation("HideNSeek is being disabled.");
            _unregister.Dispose();
            return default;
        }
    }
}