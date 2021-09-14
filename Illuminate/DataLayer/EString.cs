namespace DataLayer {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DisCatSharp.Entities;

    using Microsoft.EntityFrameworkCore;

    internal class EString{
        [Key]
        public int DbId { get; private set; }
        public string String { get; private set; }
        public EString(string @string) => String = @string;
    }
}