using EduCore.API.Data;
using EduCore.API.DTOs.Payment;
using EduCore.API.Entities;
using EduCore.API.Repositories.Implementations;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Service.Vnpay;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly EduCoreDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPaymentRepository _repo;
        private readonly INotificationRepository _notificationRepo;

        public PaymentController(IVnPayService vnPayService, EduCoreDbContext context, IConfiguration configuration, IPaymentRepository repo, INotificationRepository notificationRepo)
        {
            _vnPayService = vnPayService;
            _context = context;
            _configuration = configuration;
            _repo = repo;
            _notificationRepo = notificationRepo;
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDto request)
        {
            // 1. Tạo mã đơn hàng duy nhất
            string orderCode = DateTime.Now.Ticks.ToString();

            // 2. Lưu vào DB với trạng thái Pending
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                CourseId = request.CourseId,
                StudentId = request.StudentId,
                Amount = (decimal)request.Amount,
                OrderCode = orderCode, // Cột này sẽ dùng để tìm lại khi VNPAY trả về
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // 3. Tạo URL thanh toán
            var model = new PaymentInformationModel
            {
                Amount = request.Amount,
                OrderDescription = "Thanh toan khoa hoc",
                OrderId = orderCode // Truyền mã khớp với DB sang VNPAY
            };

            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Ok(new { paymentUrl = url });
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (!response.Success)
            {
                Console.WriteLine("VNPAY Signature Validation Failed!");
                return Redirect($"{_configuration["FrontendUrl"]}/payment-result?code=99");
            }

            if (!string.IsNullOrEmpty(response.OrderId))
            {
                var payment = await _context.Payments
                    .Include(p => p.Course)
                    .FirstOrDefaultAsync(p => p.OrderCode.Trim() == response.OrderId.Trim());

                if (payment != null)
                {
                    payment.TransactionNo = response.TransactionId;
                    payment.BankCode = response.BankCode;
                    payment.ResponseCode = response.VnPayResponseCode;

                    if (response.VnPayResponseCode == "00")
                    {
                        payment.Status = "Success";
                        payment.PaidAt = !string.IsNullOrEmpty(response.PayDate)
                            ? DateTime.ParseExact(response.PayDate, "yyyyMMddHHmmss", null)
                            : DateTime.UtcNow;

                        var existingEnrollment = await _context.Enrollments
                            .FirstOrDefaultAsync(e => e.UserId == payment.StudentId && e.CourseId == payment.CourseId);

                        if (existingEnrollment == null)
                        {
                            var enrollment = new Enrollment
                            {
                                Id = Guid.NewGuid(),
                                UserId = payment.StudentId,
                                CourseId = payment.CourseId,
                                EnrolledAt = DateTime.UtcNow,
                                ProgressPercent = 0,
                                CompletedLessons = 0,
                                IsCompleted = false,
                                IsPaid = true,          
                                Status = "Active",     
                                PaidAt = payment.PaidAt,
                                PaymentMethod = "VNPAY",
                                LastAccessedAt = DateTime.UtcNow,
                                CompletedLessonIds = "[]" 
                            };
                            _context.Enrollments.Add(enrollment);
                            Console.WriteLine(" Đã tạo mới bản ghi Enrollment cho học viên.");
                        }
                        else
                        {
                            existingEnrollment.IsPaid = true;
                            existingEnrollment.Status = "Active";
                            existingEnrollment.PaidAt = payment.PaidAt;
                            _context.Enrollments.Update(existingEnrollment);
                            Console.WriteLine("Đã cập nhật trạng thái Enrollment thành Active.");
                        }
                        try
                        {
                            var notify = new Notification
                            {
                                Id = Guid.NewGuid(),
                                SenderId = payment.StudentId, 
                                Content = $"đã thanh toán thành công khóa học: {payment.Course?.Title ?? "N/A"}",
                                Type = "payment",
                                RedirectUrl = "/admin/finance",
                                IsRead = false,
                                CreatedAt = DateTime.UtcNow
                            };

                            await _notificationRepo.AddNotificationAsync(notify);
                            Console.WriteLine("SignalR: Đã gửi thông báo thanh toán tới Admin.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi gửi thông báo SignalR: {ex.Message}");
                        }
                    }
                    else
                    {
                        payment.Status = "Failed";
                    }

                    try
                    {
                        _context.Payments.Update(payment);
                        await _context.SaveChangesAsync();
                        Console.WriteLine("Cập nhật Database (Payment & Enrollment) thành công!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi SaveChanges: {ex.Message}");
                    }
                }
            }

            return Redirect($"{_configuration["FrontendUrl"]}/payment-result?code={response.VnPayResponseCode}");
        }

        [HttpGet("history/{studentId}")]
        public async Task<IActionResult> GetPaymentHistory(Guid studentId)
        {
            var result = await _repo.GetPaymentHistoryByStudentIdAsync(studentId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("all-history")]
        public async Task<IActionResult> GetAllHistory([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _repo.GetAllPaymentsAsync(pageIndex, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApprovePayment(Guid id)
        {
            var result = await _repo.ApprovePaymentAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("reject/{id}")]
        public async Task<IActionResult> RejectPayment(Guid id)
        {
            var result = await _repo.RejectPaymentAsync(id);

            return StatusCode(result.StatusCode, result);
        }

        private async Task SendPaymentNotification(Guid studentId, string courseName)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                SenderId = studentId,
                // Nội dung khớp với giao diện Admin đã thiết kế
                Content = $"vừa thanh toán thành công khóa học: {courseName}",
                Type = "payment",
                RedirectUrl = "/admin/finance", // URL để Admin bấm vào xem chi tiết đơn hàng
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            // Gọi Repository để lưu DB và Push SignalR
            await _notificationRepo.AddNotificationAsync(notification);
        }
    }
}
