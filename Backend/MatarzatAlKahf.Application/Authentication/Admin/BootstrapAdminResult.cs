namespace MatarzatAlKahf.Application.Authentication.Admin;
public sealed record BootstrapAdminResult(bool Created, Guid? AdminUserId, string Outcome);
