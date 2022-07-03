
using Microsoft.EntityFrameworkCore;

using TheOnlyParty.DiscordBot.Models;

namespace TheOnlyParty.DiscordBot.DbContexts;

public class DiscordDbContext : DbContext
{
    public DbSet<UserReport> UserReports { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public DiscordDbContext(DbContextOptions options) : base(options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }
}
