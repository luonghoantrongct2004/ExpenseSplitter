namespace BE.Common;

public static class CommonData
{
    public enum PaymentMethod
    {
        Cash,
        BankTransfer,
        Momo,
        ZaloPay,
        VNPay,
        Other
    }

    public enum SettlementStatus
    {
        Pending,
        Completed,
        Cancelled
    }

    public enum ActivityAction
    {
        Create,
        Update,
        Delete,
        View,
        Login,
        Logout,
        Invite,
        Join,
        Leave,
        Settle
    }

    public enum GroupRole
    {
        Admin,
        Member
    }

    public enum NotificationType
    {
        ExpenseAdded,
        ExpenseUpdated,
        ExpenseDeleted,
        PaymentReceived,
        PaymentSent,
        PaymentReminder,
        MemberJoined,
        MemberLeft,
        GroupInvite,
        System
    }

    public enum ExpenseCategory
    {
        Food,
        Transport,
        Accommodation,
        Entertainment,
        Shopping,
        Other
    }

    public struct Currency
    {
        public const string VND = "VND";
    }
}