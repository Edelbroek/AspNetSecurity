using Globomantics.Models;
using Globomantics.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Globomantics.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IUserRepository userRepository;

        public AccountController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public IActionResult Login(string returnUrl = "/")
            => View(new LoginModel { ReturnUrl = returnUrl });

        public IActionResult LoginWithExternalProvider(string scheme, string returnUrl = "/")
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(ExternalLoginCallback)),
                Items =
                {
                    { nameof(scheme), scheme },
                    { nameof(returnUrl), returnUrl }
                }
            };

            return Challenge(props, scheme);
        }

        public async Task<IActionResult> ExternalLoginCallback()
        {
            //read external identity from cookie
            var result = await HttpContext.AuthenticateAsync(ExternalAuthenticationDefaults.AuthenticationScheme);
            var scheme = result?.Properties?.Items["scheme"];
            var returnUrl = result?.Properties?.Items["returnUrl"];

            var externalClaims = result?.Principal?.Claims
                ?? throw new InvalidOperationException("Could not create a principal");
            var subjectIdClaim = externalClaims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value
                ?? throw new InvalidOperationException("Could not extract subject id from external principal");
            var model = GetUserFromRepositoryByExternalProvider(scheme, subjectIdClaim)
                ?? throw new InvalidOperationException("Local user was not found");

            var principal = CreatePrincipal(model);

            await HttpContext.SignOutAsync(ExternalAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return LocalRedirect(returnUrl ?? "/");
        }

        private UserModel GetUserFromRepositoryByExternalProvider(string? scheme, string subjectIdClaim)
            => scheme switch
            {
                GoogleDefaults.AuthenticationScheme => userRepository.GetByGoogleId(subjectIdClaim),
                _ => throw new InvalidOperationException($"AuthenticationScheme {scheme}, isn't supported at the moment")
            };

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = userRepository.GetByUsernameAndPassword(
                model.Username,
                model.Password);

            if (user is null)
                return Unauthorized();

            var principal = CreatePrincipal(user);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberLogin,
                    ExpiresUtc = DateTimeOffset.Now.AddMinutes(30),
                    AllowRefresh = true
                });

            return LocalRedirect(model.ReturnUrl);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/");
        }

        private static ClaimsPrincipal CreatePrincipal(UserModel user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(nameof(UserModel.FavoriteColor), user.FavoriteColor)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            return new ClaimsPrincipal(identity);
        }
    }
}
