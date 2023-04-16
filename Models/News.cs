using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab4v2.Models
{
    public class News
    {
        public Guid Id { get; set; }

        public string FileName { get; set; }

        public string Url { get; set; }

        public string NewsBoardID { get; set; }
        public NewsBoard NewsBoard { get; set; }

        [DataType(DataType.Upload)]
        [NotMapped]
        public IFormFile imageFile { get; set; }
    }
}
