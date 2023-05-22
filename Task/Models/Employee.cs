using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Task.Data
{
    public partial class Employee
    {
        [Key]
        public int Id { get; set; }


        public string? Name { get; set; } = "anonymous";

        [Required]
        public string Email { get; set; } = null!;

        public string Role { get; set; } = "non-admin";

        [Required]
        [MinLength(5)]
        public string Password { get; set; } = null!;
    }


}

