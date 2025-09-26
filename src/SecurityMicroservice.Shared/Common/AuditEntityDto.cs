namespace SecurityMicroservice.Shared.Common
{
    public class AuditEntityDto
    {
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
