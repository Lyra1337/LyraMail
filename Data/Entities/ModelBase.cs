using System.ComponentModel.DataAnnotations;

namespace Lyralabs.TempMailServer.Data
{
    public abstract class ModelBase
    {
        [Key]
        public int Id { get; set; }
    }
}