using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class CreateRaceDto
    {
        [Required(ErrorMessage = "Tên giải chạy là bắt buộc")]
    [MaxLength(255, ErrorMessage = "Tên giải chạy không được vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Ngày tổ chức là bắt buộc")]
    [DataType(DataType.DateTime)]
    public DateTime RaceDate { get; set; }
    
    [Required(ErrorMessage = "Email BTC là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Mật khẩu email là bắt buộc")]
    [MaxLength(255)]
    public string EmailPassword { get; set; } = string.Empty;

    // NEW SYSTEM - Chỉ cần chọn Sheet Config từ dropdown
    [Required(ErrorMessage = "Sheet config là bắt buộc")]
    public int SheetConfigId { get; set; }

    // Remove legacy SheetId property - không cần nữa
    // public string? SheetId { get; set; } // REMOVED
    [MinLength(1, ErrorMessage = "Phải có ít nhất một cự ly")]
    public List<CreateRaceDistanceDto> Distances { get; set; } = new();
    public bool HasShirtSale { get; set; }
    public List<CreateRaceShirtTypeDto> ShirtTypes { get; set; } = new();
    }
    public class CreateRaceShirtTypeDto                            // NEW
    {
        [Required]
        [MaxLength(50)]
        public string ShirtCategory { get; set; } = string.Empty;  // Nam, Nữ, Trẻ em

        [Required]
        [MaxLength(50)]
        public string ShirtType { get; set; } = string.Empty;      // T-Shirt, Singlet

        [Required]
        [MaxLength(255)]
        public string AvailableSizes { get; set; } = string.Empty; // S,M,L,XL or KID-10,KID-12

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        public bool IsActive { get; set; } = true;
    }
    public class CreateRaceDistanceDto
    {
        [Required]
        [MaxLength(50)]
        public string Distance { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [MaxLength(10)]
        public string? BibPrefix { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxParticipants { get; set; }
    }
}
