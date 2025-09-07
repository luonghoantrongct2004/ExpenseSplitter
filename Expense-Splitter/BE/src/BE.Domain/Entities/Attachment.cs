namespace BE.Domain.Entities
{
    public class Attachment : BaseEntity
    {
        public Guid? ExpenseId { get; set; }
        public Guid? SettlementId { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; } // in bytes
        public string FileType { get; set; } // MIME type
        public string? ThumbnailUrl { get; set; }
        public Guid UploadedById { get; set; }

        // Navigation properties
        public virtual Expense? Expense { get; set; }

        public virtual Settlement? Settlement { get; set; }
        public virtual User UploadedBy { get; set; }
    }
}