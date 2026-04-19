using Azure;
using EduCore.API.Data;
using EduCore.API.DTOs.Payment;
using EduCore.API.Entities;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Repositories.ResponseMessage;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly EduCoreDbContext _context;
        private readonly ResponseMessageResult _response;
        public PaymentRepository(EduCoreDbContext context, ResponseMessageResult response)
        {
            _context = context;
            _response = response;
        }
        public async Task<ResponseMessageResult> GetPaymentHistoryByStudentIdAsync(Guid studentId)
        {
            try
            {
                var history = await _context.Payments
                    .Where(p => p.StudentId == studentId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new PaymentHistory
                    {
                        Id = p.Id,
                        OrderCode = p.OrderCode,
                        Amount = p.Amount,
                        Status = p.Status,
                        CreatedAt = p.CreatedAt,
                        PaidAt = p.PaidAt,
                        BankCode = p.BankCode,
                        TransactionNo = p.TransactionNo,
                        CourseTitle = _context.Courses
                            .Where(c => c.Id == p.CourseId)
                            .Select(c => c.Title)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (history == null || !history.Any())
                {
                    return _response.SetSuccess("Chưa có lịch sử giao dịch nào.", new List<PaymentHistory>());
                }

                return _response.SetSuccess("Lấy lịch sử thanh toán thành công.", history);
            }
            catch (Exception ex)
            {
                return _response.SetFail($"Đã xảy ra lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<ResponseMessageResult> GetAllPaymentsAsync(int pageIndex, int pageSize)
        {
            try
            {
                var totalRecords = await _context.Payments.CountAsync();

                var data = await _context.Payments
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PaymentAdminHistory
                    {
                        Id = p.Id,
                        OrderCode = p.OrderCode,
                        Amount = p.Amount,
                        Status = p.Status,
                        CreatedAt = p.CreatedAt,
                        PaidAt = p.PaidAt,
                        BankCode = p.BankCode,
                        TransactionNo = p.TransactionNo,
                        CourseTitle = _context.Courses
                            .Where(c => c.Id == p.CourseId)
                            .Select(c => c.Title)
                            .FirstOrDefault(),
                        StudentName = _context.Users
                            .Where(u => u.Id == p.StudentId)
                            .Select(u => u.Name)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                var result = new
                {
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    Items = data
                };

                if (data.Count == 0 && totalRecords > 0)
                {
                    return _response.SetFail("Trang yêu cầu vượt quá tổng số trang hiện có.", 404);
                }

                return _response.SetSuccess("Lấy danh sách giao dịch toàn hệ thống thành công.", result);
            }
            catch (Exception ex)
            {
                return _response.SetFail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<ResponseMessageResult> ApprovePaymentAsync(Guid paymentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
                if (payment == null) return _response.SetFail("Không tìm thấy giao dịch.", 404);
                if (payment.Status == "Success") return _response.SetFail("Giao dịch đã duyệt trước đó.", 400);

                payment.Status = "Success";
                payment.PaidAt = DateTime.UtcNow;

                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.UserId == payment.StudentId && e.CourseId == payment.CourseId);

                if (enrollment == null)
                {
                    await _context.Enrollments.AddAsync(new Enrollment
                    {
                        Id = Guid.NewGuid(),
                        UserId = payment.StudentId,
                        CourseId = payment.CourseId,
                        EnrolledAt = DateTime.UtcNow,
                        IsPaid = true,
                        Status = "Active",
                        PaidAt = DateTime.UtcNow,
                        PaymentMethod = "Manual_Admin",
                        CompletedLessonIds = "[]"
                    });
                }
                else
                {
                    enrollment.IsPaid = true;
                    enrollment.Status = "Active";
                    enrollment.PaidAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return _response.SetSuccess("Duyệt giao dịch thành công.", payment);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return _response.SetFail($"Lỗi hệ thống: {ex.Message}", 500);
            }
        }

        public async Task<ResponseMessageResult> RejectPaymentAsync(Guid paymentId)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return _response.SetFail("Không tìm thấy giao dịch.", 404);
            }

            if (payment.Status != "Pending")
            {
                return _response.SetFail($"Không thể hủy giao dịch đã ở trạng thái: {payment.Status}", 400);
            }

            payment.Status = "Failed";
            await _context.SaveChangesAsync();

            return _response.SetSuccess("Đã từ chối và hủy giao dịch thành công.", payment);
        }
    }
}
