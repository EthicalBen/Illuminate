namespace DataLayer {
    using System.ComponentModel.DataAnnotations;

    internal class EEmojiRolePair {

        [Key]
        public int DbId { get; private set; }

        public string EmojiName { get; set; }

        public ulong RoleId { get; set; }
    }
}