namespace DataLayer {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;

    internal class IlluminateContext:DbContext{
        internal DbSet<EDiscordMember> Members { get; private init; }
        internal DbSet<EString> SwearWords { get; private init; }
        internal DbSet<EReactionMessage> ReactionMessages { get; private init; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source=Illuminate.db");
    }
}
