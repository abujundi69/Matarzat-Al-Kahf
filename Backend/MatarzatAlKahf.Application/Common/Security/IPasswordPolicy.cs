using MatarzatAlKahf.Application.Common.Exceptions;

namespace MatarzatAlKahf.Application.Common.Security;
public interface IPasswordPolicy
{
    void Validate(string password);
}
