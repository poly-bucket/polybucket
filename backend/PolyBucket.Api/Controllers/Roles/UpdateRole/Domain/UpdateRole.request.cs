using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Roles.UpdateRole.Domain
{
    public class UpdateRole
    {
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }
    }
}