using System.ComponentModel.DataAnnotations;

namespace VKITActivityManager.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản!")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}