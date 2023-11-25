using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Schedule_Mate.Models
{
    public partial class CollectionUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        // [Required, Key, Column("Email", TypeName = "varchar(50)")]
        public string Email { get; set; }
        // [Required, Column("Name", TypeName = "varchar(50)")]
        public string Name { get; set; }
        // [Required, Column("Password", TypeName = "varchar(50)")]
        public string Password { get; set; }
        // [Required, Column("ProfilePic", TypeName = "image")]
        public string ProfilePic { get; set; }
    }
}