using EduCore.API.Repositories.ResponseMessage;

namespace EduCore.API.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<ResponseMessageResult> GetPaymentHistoryByStudentIdAsync(Guid studentId);
        Task<ResponseMessageResult> GetAllPaymentsAsync(int pageIndex, int pageSize);
        Task<ResponseMessageResult> ApprovePaymentAsync(Guid paymentId);
        Task<ResponseMessageResult> RejectPaymentAsync(Guid paymentId);
    }
}
