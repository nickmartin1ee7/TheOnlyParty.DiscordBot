
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TheOnlyParty.DiscordBot.DbContexts
{
    public class DiscordDbContextFactory : IDesignTimeDbContextFactory<DiscordDbContext>
    {
        public DiscordDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DiscordDbContext>();
            optionsBuilder.UseSqlite("Data Source = ./Data/discord.db");

            return new DiscordDbContext(optionsBuilder.Options);
        }
    }
}
