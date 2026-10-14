using System.ComponentModel.DataAnnotations;

namespace BIT240058_MID.Models
{
    /// <summary>
    /// Validation tự viết: giá trị số phải LỚN HƠN 0 (dùng cho Price và PreparationTime).
    /// Dùng attribute riêng thay vì [Range] để kiểm tra đúng điều kiện "> 0" (không phải ">= 1").
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class GreaterThanZeroAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            // null để cho lỗi "bắt buộc nhập" xử lý, ở đây coi như hợp lệ
            if (value == null) return true;

            return value switch
            {
                int i => i > 0,
                long l => l > 0,
                decimal d => d > 0,
                double db => db > 0,
                _ => false
            };
        }
    }
}
