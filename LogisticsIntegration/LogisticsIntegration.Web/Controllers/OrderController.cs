using LogisticsIntegration.Application.Services;
using LogisticsIntegration.Domain.Entities;
using LogisticsIntegration.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsIntegration.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(OrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var viewModel = new OrderListViewModel { Orders = orders };
            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderDetailViewModel
            {
                Order = order,
                NewStatus = order.Status
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderDetailViewModel model)
        {
            if (id != model.Order.Id)
            {
                return BadRequest();
            }

            if (model.NewStatus == OrderStatus.Delivered)
            {
                if (string.IsNullOrWhiteSpace(model.PlateNumber))
                {
                    ModelState.AddModelError("PlateNumber", "Plaka alanı 'Teslim Edildi' statüsü için zorunludur.");
                }
                if (string.IsNullOrWhiteSpace(model.DelivererName))
                {
                    ModelState.AddModelError("DelivererName", "Teslim eden kişi alanı 'Teslim Edildi' statüsü için zorunludur.");
                }
            }
            
            model.Order = await _orderService.GetOrderByIdAsync(id); 

            try
            {
                await _orderService.UpdateOrderStatusAndDeliveryInfoAsync(
                    id,
                    model.NewStatus,
                    model.PlateNumber,
                    model.DelivererName
                );
                TempData["SuccessMessage"] = "Sipariş durumu başarıyla güncellendi!";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş durumu güncellenirken beklenmedik bir hata oluştu.");
                ModelState.AddModelError("", "Sipariş durumu güncellenirken bir hata oluştu.");
            }

            return View("Details", model);
        }
    }
}
