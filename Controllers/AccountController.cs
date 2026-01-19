using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ButceTakipSistemi.Controllers
{
    public class AccountController : Controller
    {
        // Giriş Sayfasını Göster (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Giriş Yap Butonuna Basılınca (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Basit kontrol (Hocanın dokümanındaki gibi)
            if (username == "admin" && password == "123")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // Başarılıysa İşlemler sayfasına git
                return RedirectToAction("Index", "Transactions");
            }

            // Hatalıysa hata mesajı göster
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        // Çıkış Yap
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}