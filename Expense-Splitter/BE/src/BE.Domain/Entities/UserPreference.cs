using BE.Common;

namespace BE.Domain.Entities
{
    public class UserPreference : BaseEntity
    {
        public Guid UserId { get; set; }
        public string DefaultCurrency { get; set; } = CommonData.Currency.VND;
        public string Language { get; set; } = "vi";
        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
        public string Theme { get; set; } = "light"; // light, dark, auto

        public bool EmailNotifications { get; set; } = true;

        public bool PushNotifications { get; set; } = true;
        public bool ExpenseAddedNotification { get; set; } = true;
        public bool PaymentReceivedNotification { get; set; } = true;
        public bool PaymentReminderNotification { get; set; } = true;
        public int PaymentReminderDays { get; set; } = 7; // Remind after X days

        // Display preferences
        public bool ShowDetailedSplits { get; set; } = true;

        public bool ShowRunningBalance { get; set; } = true;
        public string DateFormat { get; set; } = "dd/MM/yyyy";

        // Navigation property
        public virtual User User { get; set; }
    }
}