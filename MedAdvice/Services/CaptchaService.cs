using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MedAdvice.Services
{
    public enum CaptchaServiceResult
    {
        Failes = -2, Robot = -1, Human = 1
    }
    public class CaptchaService
    {
        IConfiguration configuration;
        IHttpClientFactory httpClientFactory;
        public CaptchaService(IConfiguration _configuration, IHttpClientFactory _httpClientFactory)
        {
            configuration = _configuration;
            httpClientFactory = _httpClientFactory;
        }
        class RecaptchaModel
        {
            public bool success { get; set; }
        }
        public async Task<CaptchaServiceResult> VerifyCaptchaAsync(Controller controller)
        {
            string secretkey = configuration["Recaptcha:SecretKey"];
            HttpClient httpclient = httpClientFactory.CreateClient();
            var captcha = controller.Request.Form["g-recaptcha-response"];
            var res = await httpclient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretkey}&response={captcha}");
            if (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                controller.TempData["msg"] = "Error in evaluation of captcha by google";
                return CaptchaServiceResult.Failes;

            }
            else
            {
                string json = await res.Content.ReadAsStringAsync();
                RecaptchaModel recaptchaModel = Newtonsoft.Json.JsonConvert.DeserializeObject<RecaptchaModel>(json);
                if (recaptchaModel.success == false)
                {
                    controller.TempData["msg"] = "google knows you as a robot.";
                    return CaptchaServiceResult.Robot;
                }

            }
            return CaptchaServiceResult.Human;
        }
    }
}
