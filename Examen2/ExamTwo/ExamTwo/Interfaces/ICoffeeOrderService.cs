using ExamTwo.Models;

namespace ExamTwo.Interfaces
{
    public interface ICoffeeOrderService
    {
        Task<OrderResultDto> ProcessOrderAsync(OrderRequestDto orderRequest);
    }
}
