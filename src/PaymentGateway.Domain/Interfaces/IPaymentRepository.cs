using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Domain.Interfaces;

public interface IPaymentRepository
{
    void Add(Payment payment);
    Payment? Get(Guid id);
}
