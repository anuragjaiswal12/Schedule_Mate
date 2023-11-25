using Microsoft.EntityFrameworkCore;

namespace Schedule_Mate.Models
{
    public class DbScheduleMateContext : DbContext
    {
        public DbScheduleMateContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<CollectionUser> TblUsers { get; set; }
        public DbSet<CollectionDailyTask> TblDailyTasks { get; set; }
        public DbSet<CollectionSchedule> TblSchedules { get; set; }
    }
}