using System;

namespace SecurityMicroservice.Shared.Response.Common
{
    public class AuditEntityResponseDto
    {
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
