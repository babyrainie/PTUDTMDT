using System.Security.Claims;
using AutoMapper;
using dailybook.Data;
using dailybook.Helpers;
using dailybook.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dailybook.Controllers
{
    public class CustomerController : Controller
    {
        private readonly SneakerdailyContext db;
        private readonly IMapper _mapper;
        private object _notyfService;

        public CustomerController(SneakerdailyContext context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
        }

        #region Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var customer = _mapper.Map<Customer>(model);
                    customer.Salt = Salt.GenerateRandomKey();
                    //customer.Pass = model.Pass.ToMd5Hash(customer.Salt);
                    customer.Pass = model.Pass;
                    customer.Active = true;
                    customer.CreateDate = DateTime.UtcNow;

                    db.Add(customer);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Product");
                }
                catch (Exception ex)
                {
                }
            }
            return View();
        }
        #endregion

        #region Login
        [HttpGet]
        public IActionResult Login(string? ReturnURL)
        {
            ViewBag.ReturnURL = ReturnURL;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model, string? ReturnURL)
        {
            ViewBag.ReturnURL = ReturnURL;
            if (ModelState.IsValid)
            {
                var customer = db.Customers.SingleOrDefault(cus => cus.Email == model.Username);
                if(customer == null)
                {
                    ModelState.AddModelError("Error", "Customer not found");
                }
                else
                {
                    if(!customer.Active)
                    {
                        ModelState.AddModelError("Error", "Account is not active");
                    }
                    else
                    {
                        //if (customer.Pass != model.Password.ToMd5Hash(customer.Salt))
                        if (customer.Pass !=model.Password)
                        {
                            ModelState.AddModelError("Error", "Incorrect Password");
                        }
                        else
                        {
                            var claims = new List<Claim> {
                                new Claim(ClaimTypes.Email,customer.Email),
                                new Claim(ClaimTypes.Name,customer.Fullname),
                                new Claim("CustomerId", customer.CusId.ToString()),

                                new Claim(ClaimTypes.Role,"Customer")
                            };
                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                            await HttpContext.SignInAsync(claimsPrincipal);
                            //object value = _notyfService.Login("Đăng nhập thành công");

                            if (Url.IsLocalUrl(ReturnURL))
                            {
                                return Redirect(ReturnURL);
                            }
                            else
                            {
                                return Redirect("/");
                            }
                        }
                    }
                }
            }

            return View();
        }
        #endregion

        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
    }
}
