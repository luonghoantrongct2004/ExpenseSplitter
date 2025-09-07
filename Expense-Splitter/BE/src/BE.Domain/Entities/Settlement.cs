using BE.Common;

namespace BE.Domain.Entities
{
    public class Settlement : BaseEntity
    {
        public Guid GroupId { get; set; }
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = CommonData.Currency.VND;
        public CommonData.PaymentMethod? PaymentMethod { get; set; }
        public string? ReferenceCode { get; set; }
        public string? Note { get; set; }
        public CommonData.SettlementStatus Status { get; set; } = CommonData.SettlementStatus.Pending;
        public DateTime? SettledAt { get; set; }

        // Navigation properties
        public virtual Group Group { get; set; }

        public virtual User FromUser { get; set; }
        public virtual User ToUser { get; set; }
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}