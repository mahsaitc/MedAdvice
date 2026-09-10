using MedAdvice.Areas.Identity.Data;
using MedAdvice.Data;
using MedAdvice.Services;
using MedAdvice.viewmodel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MedAdvice.Controllers
{
    public class AccountController : Controller
    {
        MedAdviceDb db;
        UserManager<ApplicationUser> userManager;
        SignInManager<ApplicationUser> signinmanager;
        IConfiguration configuration;
        public AccountController(MedAdviceDb _db,UserManager<ApplicationUser> _userManager,SignInManager<ApplicationUser> _signinmanager,IConfiguration _configuration)
        {
            signinmanager = _signinmanager;
            db = _db;
            userManager = _userManager;
            configuration = _configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SigninSignup() => View();
        private static string DescribeErrors(IdentityResult result)
        {
            return string.Join(" ", result.Errors.Select(x => x.Description));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordByPhoneNumberLevelTwo(ResetPasswordViewModel model)
        {
            string id = HttpContext.Session.GetString("id");
            ApplicationUser user = await userManager.FindByIdAsync(id);
            if (user.tokenExpirationTime < DateTime.Now)
            {
                TempData["msg"] = "your token is expired.please send new token!";
                return RedirectToAction("SignInSignUp", "Account");
            }
            var result = await userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, model.smstoken);
            if (result.Succeeded)
            {
                string token = await userManager.GeneratePasswordResetTokenAsync(user);
                result = await userManager.ResetPasswordAsync(user, token, model.password);
                if (result.Succeeded)
                    TempData["msg"] = "Your new password is changed successfully";
                else
                    TempData["msg"] = DescribeErrors(result);
            }
            else
                TempData["msg"] = DescribeErrors(result);
            return RedirectToAction("SignInSignUp", "Account");

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordByPhoneCellLevelOne(string username)
        {
            ApplicationUser user = await userManager.FindByNameAsync(username);
            if (user != null)
            {
                string token = await userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                user.tokenExpirationTime = DateTime.Now.AddSeconds(60);
                await userManager.UpdateAsync(user);
                //send sms
                SendSoapClient sendSoapClient =
                    new ServiceReference1.SendSoapClient(ServiceReference1.SendSoapClient.EndpointConfiguration.SendSoap);

                var result = await sendSoapClient.SendSimpleSMSAsync(
                    configuration["Sms:UserName"], configuration["Sms:Password"],
                    new[] { user.PhoneNumber }, configuration["Sms:LineNumber"], token, false);
                HttpContext.Session.SetString("id", user.Id);
            }
            TempData["msg"] = "changing password code is sent to your phone.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resetpasswordlevelone(string username)
        {
            ApplicationUser user = await userManager.FindByNameAsync(username);
            if(user !=null)
            {
                string Token = await userManager.GeneratePasswordResetTokenAsync(user);
                string adress = Url.Action("ResetpasswordlevelTwo", "Account", new {id= user.Id, Token = Token },"https");
              
               string body = $"{user.lastname} {user.firstname} hello <br/>" +
                   $"to change your password please click on this" +
                   $"<a href='{adress}'> link </a>";

                try
                {
                    string senderEmail = configuration["Smtp:SenderEmail"];

                    MailMessage mailmessage = new MailMessage(senderEmail, user.Email);
                    mailmessage.Subject = "change password";
                    mailmessage.Body = body;
                    mailmessage.IsBodyHtml = true;

                    SmtpClient smtpClient = new SmtpClient(configuration["Smtp:Host"],
                        int.Parse(configuration["Smtp:Port"]));
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new System.Net.NetworkCredential(
                        senderEmail, configuration["Smtp:Password"]);
                    smtpClient.Send(mailmessage);
                    TempData["msg"] = "email for changing your password is sent.";
                }
                catch
                {
                    TempData["msg"] ="error!sending email...";
                }
            }
            else
            {
                TempData["msg"] = "the username that you have entered is not correct";
            }
            return RedirectToAction("SigninSignUp", "Account");
        }
        [HttpGet]
        public IActionResult ResetPasswordLevelTwo(string id, string token)
        {
            HttpContext.Session.SetString("id", id);
            HttpContext.Session.SetString("token", token);
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordLevelThree(ResetPasswordViewModel model)
        {
            string id = HttpContext.Session.GetString("id");
            string token = HttpContext.Session.GetString("token");
            ApplicationUser user = await userManager.FindByIdAsync(id);
            var result = await userManager.ResetPasswordAsync(user, token, model.password);
            if (result.Succeeded)
                TempData["msg"] = "your password has been changed successfully";
            else
                TempData["msg"] = DescribeErrors(result);

            return RedirectToAction("SigninSignUp", "Account");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> signupconfirm(SignupUserViewmodel model,
            [FromServices] CaptchaService captchaService)
        {
            CaptchaServiceResult s = await captchaService.VerifyCaptchaAsync(this);
            if (s != CaptchaServiceResult.Human)
                return RedirectToAction("SignInSignUp", "Account");
            ApplicationUser user = await userManager.FindByNameAsync(model.username);
            if (user==null)
            {
                user = new ApplicationUser {
                    UserName = model.username,
                    firstname = model.firstname, 
                    lastname = model.lastname, 
                    Email = model.email,
                    EmailConfirmed=true,
                    PhoneNumberConfirmed = true,
                    PhoneNumber = model.phonenumber
                };
                IdentityResult createResult = await userManager.CreateAsync(user, model.password);
                if (createResult.Succeeded == false)
                {
                    TempData["msg"] = DescribeErrors(createResult);
                    return RedirectToAction("signinsignup");
                }
                IdentityResult roleResult = await userManager.AddToRoleAsync(user, "customers");
                if (roleResult.Succeeded == false)
                {
                    TempData["msg"] = DescribeErrors(roleResult);
                    return RedirectToAction("signinsignup");
                }
                TempData["msg"] = "user is created successfully!";
            }
            else
            {
                TempData["msg"] = "username is repetitive..";
            }

            return RedirectToAction("signinsignup");
        }
        class RecaptchaModel
        {
            public bool success { get; set; }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> signinconfirm(signinuserviewmodel model
             ,[FromServices] CaptchaService captchaService)
        {
            CaptchaServiceResult s = await captchaService.VerifyCaptchaAsync(this);
            if (s != CaptchaServiceResult.Human)
                return RedirectToAction("SignInSignUp", "Account");

            ApplicationUser user = await userManager.FindByNameAsync(model.username);
            if (user!=null)
            {
               Microsoft.AspNetCore.Identity.SignInResult result = await signinmanager.PasswordSignInAsync(user, model.password, model.rememberme,true);
                if (result.Succeeded)
                {
                    //if (await userManager.IsInRoleAsync(user, "admins"))
                    //    return RedirectToAction("/admin/adminpanel/index");
                    TempData["msg"] = "you signed in successfully";
                    HttpContext.Session.Remove("lockedoutdate");
                    if( await userManager.IsInRoleAsync(user, "admins"))
                    {
                        return Redirect("/admin/AdminPanel/index");
                    }
                    return RedirectToAction("home", "home");
                }
                else if(result.IsLockedOut)
                {
                    TempData["IsLockedOut"] = true;
                    TempData["msg"] = "3 times incorrect password!!you are locked for 80 seconds..";
                    HttpContext.Session.SetString("lockedoutdate", DateTime.Now.ToString());
                }
                else
                {
                    TempData["msg"] = "username or password is incorrect";
                }
            }
            else
            {
                TempData["msg"] = "username is incorrect";
            }
            return RedirectToAction("SigninSignup", "account");

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>signout()
        {
            await signinmanager.SignOutAsync();
            return RedirectToAction("signinsignup", "account");
        }
        [HttpGet]
        public async Task<IActionResult>checkusername(string username)
        {
            ApplicationUser user =await userManager.FindByNameAsync(username);
            if(user==null)
            {
                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }


    }
}
