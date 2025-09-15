namespace BE.Common;

public static class Messages
{
    public const string Created = "Tạo {0} thành công rồi nè! 🎉";
    public const string Updated = "Cập nhật {0} thành công! ✨";
    public const string Deleted = "Xóa {0} thành công! 🗑️";
    public const string Retrieved = "Lấy {0} thành công! 📊";
    public const string Added = "Thêm {0} thành công! ➕";
    public const string Removed = "Xóa {0} thành công! ➖";
    public const string Joined = "Tham gia {0} thành công! 🎊";
    public const string Left = "Đã rời khỏi {0}! 👋";
    public const string Generated = "Tạo {0} thành công! 🎟️";
    public const string Archived = "Lưu trữ {0} thành công! 📦";

    // Generic messages - dùng với string.Format
    public const string NotFound = "Ủa, {0} đâu mất tiêu rồi? 🔍";
    public const string Invalid = "{0} không hợp lệ rồi bạn ơi 😅";
    public const string Success = "{0} thành công rồi nè! 🎉";
    public const string Failed = "Ối dồi ôi, {0} bị lỗi rồi 😢";
    public const string AlreadyExists = "{0} có rồi mà bạn, tạo chi nữa 🤔";
    public const string Unauthorized = "Ê ê, không phải chỗ của bạn đâu nhé! 🚫";
    public const string OnlyAdminCan = "Chỉ admin mới được {0} thôi nha 👮‍♂️";
    public const string CannotDelete = "Không xóa được {0} đâu bạn ơi 🙅‍♂️";
    public const string CannotUpdate = "Không sửa được {0} rồi 🤷‍♂️";

    // Specific cases
    public const string InvalidCredentials = "Email hoặc mật khẩu sai rồi bạn êi 🤦‍♂️";
    public const string SessionExpired = "Phiên làm việc hết hạn rồi, đăng nhập lại nha 🔄";
    public const string ValidationFailed = "Điền thông tin chưa đúng kìa bạn 📝";
    public const string SomethingWentWrong = "Ui, có gì đó sai sai. Thử lại nha! 🔧";

    // Fun specific messages
    public const string UserNotInGroup = "Ê, bạn không phải thành viên nhóm này, xin phép đi chỗ khác chơi nhé! 🚪";
    public const string EmptyWallet = "Ví trống rỗng, không có gì để chia 💸";
    public const string NoMoney = "Hết tiền rồi sao mà chia 😭";
    public const string AlreadyPaid = "Trả rồi mà bạn, đòi hoài vậy 💰";
    public const string TooManyRequests = "Ơ kìa, từ từ thôi bạn ơi 🐌";
    public const string FeatureComingSoon = "Tính năng này đang nấu, sắp xong rồi 🍳";
    public const string CoffeeBreak = "Server đang đi uống cà phê, quay lại sau nhé ☕";

    public const string Successed = "Yeahhh! Thành công rồi bạn tôi ơi! 🥳";
    public const string Exception = "Ôi trời ơi! Lỗi rồi, báo IT gấp! 🆘";
    public const string NotPermission = "Ủa, bạn không có quyền {0} đâu nhé! 🛑";
    public const string UserAlready = "{0} đã ở trong {1} rồi bạn ơi! 🤷";

    // Group specific
    public const string GroupNotFound = "Nhóm này bay màu rồi bạn ơi! 👻";
    public const string CannotLeaveAsAdmin = "Là admin duy nhất mà đòi bỏ nhóm hả? Tìm người thay đi! 👑";
    public const string InviteCodeInvalid = "Mã mời không đúng rồi, kiểm tra lại đi bạn! 🔑";
    public const string GroupHasDebt = "Còn nợ chưa trả xong mà đòi xóa nhóm hả? 💸";

    // Member specific  
    public const string MemberNotFound = "Thành viên này không tồn tại hoặc đã bay màu! 🌬️";
    public const string CannotRemoveSelf = "Tự xóa chính mình à? Dùng nút 'Rời nhóm' đi bạn! 🤦";
    public const string AlreadyInGroup = "Bạn này trong nhóm rồi mà, mời làm chi nữa! 🙄";

    // Money related
    public const string NoBalance = "Tài khoản đang âm, nạp tiền đi bạn! 💳";
    public const string DebtNotSettled = "Còn nợ {0} chưa trả kìa! 🤑";
    public const string AlreadySettled = "Thanh toán rồi mà, check lại đi! ✅";
}
