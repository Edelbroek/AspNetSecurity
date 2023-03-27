using Globomantics.Models;

namespace Globomantics.Repositories;

public class UserRepository : IUserRepository
{
    private readonly List<UserModel> _users = new()
    {
        new UserModel
        {
            Id = 3522,
            Name = "Rob",
            Password = "VN5/YG8lI8uo76wXP6tC+39Z1Wzv+XTI/bc0LPLP40U=",
            FavoriteColor = "green",
            Role = "admin",
            GoogleId = "110116399658352310029"
        }
    };

    public UserModel? GetByGoogleId(string googleId)
        => _users.FirstOrDefault(u => u.GoogleId == googleId);

    public UserModel? GetByUsernameAndPassword(string username, string password)
        => _users.FirstOrDefault(u =>
                u.Name.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password.Sha256());
}
