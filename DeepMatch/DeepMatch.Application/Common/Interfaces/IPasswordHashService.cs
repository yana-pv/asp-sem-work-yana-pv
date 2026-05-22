using DeepMatch.Domain.Entities;

namespace DeepMatch.Application.Common.Interfaces;

public interface IPasswordHashService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password);
}
