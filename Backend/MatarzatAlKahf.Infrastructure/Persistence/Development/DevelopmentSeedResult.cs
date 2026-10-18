namespace MatarzatAlKahf.Infrastructure.Persistence.Development;

public sealed record DevelopmentSeedResult(
    int Permissions,
    int Roles,
    int AdminUsers,
    int DeliveryLocations,
    int Categories,
    int Products,
    int ProductVariants,
    int Customers,
    int Orders,
    int Notifications,
    int ContentPages);
