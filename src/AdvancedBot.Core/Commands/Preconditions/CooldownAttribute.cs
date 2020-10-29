using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;

namespace AdvancedBot.Core.Commands.Preconditions
{
    public class CooldownAttribute : PreconditionAttribute
    {
        private ConcurrentDictionary<ulong, DateTime> _cooldowns = new ConcurrentDictionary<ulong, DateTime>();
        private uint _cooldownInMs = 0;

        public CooldownAttribute(uint cooldownInMs)
        {
            _cooldownInMs = cooldownInMs;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (_cooldownInMs == 0) { return Task.FromResult(PreconditionResult.FromSuccess()); }

            if (!_cooldowns.ContainsKey(context.Guild.Id))
            {
                _cooldowns.TryAdd(context.Guild.Id, DateTime.Now);
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            _cooldowns.TryGetValue(context.Guild.Id, out DateTime lastExecution);

            if ((DateTime.Now - lastExecution).TotalMilliseconds >= _cooldownInMs)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }


            var lastExecutionTimespan = (DateTime.Now - lastExecution);
            var cooldownTimespan = TimeSpan.FromMilliseconds(_cooldownInMs);

            var timeLeft = cooldownTimespan - lastExecutionTimespan;

            return Task.FromResult(PreconditionResult.FromError(
                $"Command on cooldown. Try again in {timeLeft.Humanize()}."));
        }
    }
}
