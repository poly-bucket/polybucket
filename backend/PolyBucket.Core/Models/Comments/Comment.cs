using Core.Entities;
using Core.Models.Users;

namespace Core.Models.Comments
{
    public class Comment : Auditable
    {
        /// <summary>
        /// The content of the comment
        /// </summary>
        public string Content { get; set; } = null!;
    }
}