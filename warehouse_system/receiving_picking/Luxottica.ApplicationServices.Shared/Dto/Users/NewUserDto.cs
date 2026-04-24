using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Shared.Dto.Users
{
    public class NewUserDto
    {
        [Required]
        [StringLength(256)]
        public string UserName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }

        //[EmailAddress]
        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        public string RoleNameAssignment { get; set; }
    }
}
