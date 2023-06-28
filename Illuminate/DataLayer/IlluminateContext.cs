namespace DataLayer {

    using Microsoft.EntityFrameworkCore;

    internal class IlluminateContext:DbContext{
        internal DbSet<EDiscordMember> Members { get; private init; }
        internal DbSet<EString> SwearWords { get; private init; }
        internal DbSet<EReactionMessage> ReactionMessages { get; private init; }
        internal DbSet<EDiscordGuild> Guilds { get; private init; }
        protected override void OnConfiguring(DbContextOptionsBuilder options) {
            options.UseSqlite($"Data Source=Illuminate.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.ApplyConfiguration(new EDiscordGuild.Configuration());
        }
    }
}
