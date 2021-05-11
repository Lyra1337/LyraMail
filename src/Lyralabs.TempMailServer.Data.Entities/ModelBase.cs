using System.ComponentModel.DataAnnotations;

namespace Lyralabs.TempMailServer.Data.Entities
{
    public abstract class ModelBase
    {
        [Key]
        public int Id { get; set; }
    }
}