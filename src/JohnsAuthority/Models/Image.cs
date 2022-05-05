using System;
using System.ComponentModel.DataAnnotations;

namespace JohnsAuthority.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string Name { get; set; } // Name of file when uploaded and downloaded.
        public string GeneratedName { get; set; } // Name of file on server. To prevent conflict.
        [Display(Name = "File Type")]
        public string FileType { get; set; }
        public string Path { get; set; }
        public Location Location { get; set; }
        [Display(Name = "Uploaded Date")]
        public DateTime UploadedDate { get; set; }
        public ApplicationUser User { get; set; }

        public Image()
        {
            UploadedDate = DateTime.Now;
        }

        public string GetPath()
        {
            return Path;
        }
    }
}
