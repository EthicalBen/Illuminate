namespace DataLayer {
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using DisCatSharp.Entities;

    using Illuminate.Migrations;

    public class EDiscordMessage {
        [Key]
        public int DbId { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public static explicit operator EDiscordMessage(DiscordMessage message) {
            EDiscordMessage eMessage = new();
            eMessage.CreatedAt = message.CreationTimestamp;
            return eMessage;
        }
    }
}