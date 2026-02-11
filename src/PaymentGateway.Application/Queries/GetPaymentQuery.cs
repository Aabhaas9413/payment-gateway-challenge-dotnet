using MediatR;

namespace PaymentGateway.Application.Queries;

public class GetPaymentQuery : IRequest<GetPaymentQueryResult?>
{
    public Guid Id { get; set; }
}
