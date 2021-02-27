using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvancedBot.Core.Commands.Preconditions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace AdvancedBot.Core.Commands.Modules
{
    [Group("iwanttoplay")][Alias("iwtp")]
    [Summary("Category that handles the pingable roles commands.")]
    public class PingableRolesModule : TopModule
    {
        [Command][Cooldown(600000)]
        [Summary("Pings the role corresponding with the mentioned trigger.")]
        public async Task IWantToPlay([Remainder]string trigger)
        {
            var guild = Accounts.GetOrCreateGuildAccount(Context.Guild.Id);
            if (!guild.PingableRoles.TryGetValue(trigger, out ulong roleId))
                throw new Exception($"Couldn't find the command for {trigger}.");

            var role = Context.Guild.Roles.First(x => x.Id == roleId);
            var guildUser = Context.User as SocketGuildUser;

            await role.ModifyAsync(x => x.Mentionable = true);

            if (guildUser.Roles.FirstOrDefault(x => x.Id == role.Id) is null)
                throw new Exception($"You need role **{role.Name}** in order to use this command.");

            await ReplyAsync($"Hey <@&{role.Id}>, {Context.User.Mention} wants to play!");

            await Task.Delay(1300).ContinueWith(t => role.ModifyAsync(x => x.Mentionable = false));
        }

        [Command("add")][RequireCustomPermission(GuildPermission.ManageRoles)]
        [Summary("Adds a new role with a certain trigger to the list.")]
        public async Task AddIWantToPlay(SocketRole role, [Remainder]string trigger)
        {
            var guild = Accounts.GetOrCreateGuildAccount(Context.Guild.Id);
            guild.AddPingableRole(trigger, role.Id);

            Accounts.SaveGuildAccount(guild);
            await ReplyAsync($"Successfully added the trigger {trigger} for {role.Name}.");
        }

        [Command("remove")][RequireCustomPermission(GuildPermission.ManageRoles)]
        [Summary("Removes a new role with a certain trigger from the list.")]
        public async Task RemoveIWantToPlay([Remainder]string trigger)
        {
            var guild = Accounts.GetOrCreateGuildAccount(Context.Guild.Id);
            guild.RemovePingableRole(trigger);

            Accounts.SaveGuildAccount(guild);
            await ReplyAsync($"Successfully removed the trigger {trigger}.");
        }

        [Command("list")][RequireCustomPermission(GuildPermission.ManageRoles)]
        [Summary("Lists all current triggers and their roles.")]
        public async Task ListIWantToPlay()
        {
            var guild = Accounts.GetOrCreateGuildAccount(Context.Guild.Id);
            var roles = new List<IRole>();

            for (int i = 0; i < guild.PingableRoles.Count; i++)
            {
                var roleId = guild.PingableRoles.Values.ToArray()[i];
                var currentRole = Context.Guild.Roles.First(x => x.Id == roleId);
                if (!(currentRole is null))
                    roles.Add(currentRole);
            }

            if (roles.Count is 0) throw new Exception("This server doesn't have any pingable roles.");
            
            await ReplyAsync($"IWantToPlayRoles for **{Context.Guild.Name}**\n" + 
                            $"▬▬▬▬▬▬▬▬▬▬▬▬\n" + 
                            $"`{string.Join("`, `", roles.Select(x => $"{x.Name}"))}`");
        }    
    }
}
