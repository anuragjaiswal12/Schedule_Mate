using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Schedule_Mate.Models
{
    public partial class CollectionDailyTask
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        // [Required, Column("Email", TypeName = "varchar(50)")]
        public string Email { get; set; }
        // [Required, Column("TaskDate", TypeName = "date")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime TaskDate { get; set; }
        // [Required, Column("ScheduleId", TypeName = "varchar(100)")]
        public string ScheduleId { get; set; }
        public string ScheduleName { get; set; }
        // [Required, Column("TaskList", TypeName = "varchar(1000)")]
        public string TaskList { get; set; }
    }
}