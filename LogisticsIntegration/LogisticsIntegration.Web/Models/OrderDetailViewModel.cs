using LogisticsIntegration.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace LogisticsIntegration.Web.Models
{
    public class OrderDetailViewModel
    {
        public Order Order { get; set; }

        [Required(ErrorMessage = "Lütfen yeni statüyü seçin.")]
        public OrderStatus NewStatus { get; set; }

        [Display(Name = "Plaka")]
        [StringLength(20, ErrorMessage = "Plaka en fazla 20 karakter olabilir.")]
        public string PlateNumber { get; set; }

        [Display(Name = "Teslim Eden Kişi")]
        [StringLength(100, ErrorMessage = "Teslim eden kişi en fazla 100 karakter olabilir.")]
        public string DelivererName { get; set; }
    }
}
