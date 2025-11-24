using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyPlant.Library.Models
{
    public class Plant
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        
        [Column("seasonSowing")]
        public string SeasonSowing { get; set; }
        
        [Column("seasonBloom")]
        public string SeasonBloom { get; set; }
        
        public string Zone{get;set;}
        public string Water{get;set;}
        public string Description { get; set; }
        public string Image { get; set; }
        public string Category { get; set; }
    }
}
