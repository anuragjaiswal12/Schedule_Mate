using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Schedule_Mate.Models
{
    public partial class CollectionSchedule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        // [Required, Column("ScheduleName", TypeName = "varchar(50)")]
        // [Required]
        public string ScheduleName { get; set; }
        // [Required, Column("TaskList", TypeName = "varchar(1000)")]
        // [Required]
        public string TaskList { get; set; }
        // [Required, Column("Email", TypeName = "varchar(50)")]
        public string Email { get; set; }
        // [Key, Required, Column("ScheduleId", TypeName = "varchar(100)")]
        public string ScheduleId { get; set; }
    }
}