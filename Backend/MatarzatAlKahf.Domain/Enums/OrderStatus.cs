namespace MatarzatAlKahf.Domain.Enums;

public enum OrderStatus
{
    New,
    UnderReview,
    Confirmed,
    Preparing,
    ReadyForShipping,
    Shipped,
    Delivered,
    Cancelled,
    Returned,
    PaymentFailed
}
