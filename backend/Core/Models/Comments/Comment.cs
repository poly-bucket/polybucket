using Core.Extensions.Models;
using Core.Models.Users;

namespace Core.Models.Comments
{
    public class Comment : Auditable
    {
        /// <summary>
        /// The content of the comment
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Number of likes on this comment
        /// </summary>
        public int Likes { get; set; }

        /// <summary>
        /// Number of dislikes on this comment
        /// </summary>
        public int Dislikes { get; set; }

        /// <summary>
        /// The model this comment belongs to
        /// </summary>
        public virtual Models.Model Model { get; set; }

        /// <summary>
        /// The user who wrote the comment
        /// </summary>
        public virtual User Author { get; set; }
    }
}