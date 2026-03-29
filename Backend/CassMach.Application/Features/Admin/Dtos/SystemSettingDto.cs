using System;

namespace CassMach.Application.Features.Admin.Dtos
{
    public class SystemSettingDto
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
