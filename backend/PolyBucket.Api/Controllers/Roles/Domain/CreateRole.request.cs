using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Roles.Domain
{
    public class CreatRoleRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }
    }
}