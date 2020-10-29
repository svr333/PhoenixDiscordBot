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
    [Group("iam")]
    [Summary("Category that handles commands regarding Self Obtainable Roles.")]
    public class SelfObtainableRolesModule : TopModule
    {
        [Command]
        [Summary("Adds the desired role to the user if possible.")]
        public async Task ObtainSOR([Remainder]SocketRole role)
        {
            var guild = Accounts.GetOrCreateGuildAccount(Context.Guild.Id);
            if (!guild.RoleIdIsInObtainableRolesList(role.Id)) 
                throw new Exception("You cannot obtain this role via the 'iam' command.");
            
            var guildUser =  Context.User as SocketGuildUser;

            // somehow this gives some weirdass side-effects
            //
            //if (guildUser.Roles.Contains(role))
            //    throw new Exception($"**{guildUser.Username}** already has this role.");
            await guildUser.AddRoleAsync(role);
            var message = await ReplyAsync($"You successfully obtained role **{role.Name}**.");
        }

        [Command("not")]
        [Summary("Removes the desired role from the user if possible.")]
        public async Task RemoveSOR([Remainder]SocketRole role)
        {
            var guild = Accounts.GetOrCreateGuildAccount(Context.Guild.Id);
            if (!guild.RoleIdIsInObtainableRolesList(role.Id)) 
                throw new Exception("You cannot remove this role via iam command.");

            var guildUser =  Context.User as SocketGuildUser;

            // somehow this gives some weirdass side-effects
            //
            //if (!guildUser.Roles.Contains(role)) 
            //    throw new Exception($"**{guildUser.Username}** doesn't have this role.");

            await guildUser.RemoveRoleAsync(role);
            var message = await ReplyAsync($"Role **{role.Name}** successfully removed from your account.");
        }

        [Command("add")][RequireCustomPermission(GuildPermission.ManageRoles)]
        [Summary("Adds a role to the list of Self Obtainable Roles.")]
        public async Task AddSORToList([Remainder]SocketRole role)
        {
            var guild = Accounts.GetOrCreateGuildAccount(Context.Guild.Id);
            guild.AddSelfObtainableRole(role.Id);

            Accounts.SaveGuildAccount(guild);
            await ReplyAsync($"Successfully added **{role.Name}** with id **{role.Id}** to the list of obtainable roles.");
        }

        [Command("remove")][RequireCustomPermission(GuildPermission.ManageRoles)]
        [Summary("Removes a role from the list of Self Obtainable Roles.")]
        public async Task RemoveSORFromList([Remainder]SocketRole role)
        {
            var guild = Accounts.GetOrCreateGuildAccount(Context.Guild.Id);
            guild.RemoveSelfObtainableRole(role.Id);

            Accounts.SaveGuildAccount(guild);
            await ReplyAsync($"Successfully removed **{role.Name}** with id **{role.Id}** to the list of obtainable roles.");
        }

        [Command("list")][RequireCustomPermission(GuildPermission.ManageRoles)]
        [Summary("Lists all roles that are currently Self Obtainable.")]
        public async Task ListIAm()
        {
            var guild = Accounts.GetOrCreateGuildAccount(Context.Guild.Id);

            var roles = new List<IRole>();

            for (int i = 0; i < guild.SelfObtainableRoles.Count; i++)
            {
                var roleId = guild.SelfObtainableRoles[i];
                var currentRole = Context.Guild.Roles.First(x => x.Id == roleId);
                if (!(currentRole is null))
                    roles.Add(currentRole);
            }
            if (roles.Count is 0) throw new Exception("This server doesn't have any self obtainable roles.");

            await ReplyAsync($"Self Obtainable Roles for **{Context.Guild.Name}**\n" + 
                            $"▬▬▬▬▬▬▬▬▬▬▬▬\n" +
                            $"`{string.Join("´, ´", roles.Select(x => $"{x.Name}"))}`");
        }
    }
}
