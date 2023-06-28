namespace DataLayer {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    internal class EReactionMessage {

        [Key]
        public int DbId { get; private set; }

        public string Tag { get; set; }

        public string Content { get; set; }

        public bool Mutex { get; set; }

        public ulong? MessageId { get; set; }

        public ulong? ChannelId { get; set; }

        public List<EEmojiRolePair> Pairs { get; set; } //Dictionary?

        public EReactionMessage() {
            Pairs = new();
        }
    }
}