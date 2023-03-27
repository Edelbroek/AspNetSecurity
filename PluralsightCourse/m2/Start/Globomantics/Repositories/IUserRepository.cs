using Globomantics.Models;

namespace Globomantics.Repositories
{
    public interface IUserRepository
    {
        UserModel? GetByGoogleId(string googleId);
        UserModel? GetByUsernameAndPassword(string username, string password);
    }
}