using System;

namespace CassMach.Domain.Entities
{
    public class SystemSetting
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public User UpdatedByUser { get; set; }
    }
}
