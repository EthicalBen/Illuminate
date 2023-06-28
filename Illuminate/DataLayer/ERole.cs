namespace DataLayer {
    using System.ComponentModel.DataAnnotations;

    public class ERole {
        [Key]
        public int DbId { get; private set; }

        public ulong RoleId { get; set; }
    }
}