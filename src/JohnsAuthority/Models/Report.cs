using System;
using System.ComponentModel.DataAnnotations;

namespace JohnsAuthority.Models
{
    public class Report
    {
        public int Id { get; set; }
        public int TargetId { get; set; }
        public enum Types { Review, User, Image, Other }
        public Types Type { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public ApplicationUser ReportingUser { get; set; }
        public Report()
        {
            Type = Types.Other;
            CreatedDate = DateTime.Now;
        }

        public Report(int id, string type)
        {
            TargetId = id;

            Types t;
            if (Enum.TryParse(type, true, out t))
            {
                Type = t;
            }
            else
            {
                Type = Types.Other;
            }

            CreatedDate = DateTime.Now;
        }
    }
}
