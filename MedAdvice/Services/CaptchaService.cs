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

    /// Verifies a reCAPTCHA response with Google.
    ///
    /// Takes the response token rather than the calling Controller: reading Request.Form and
    /// writing TempData made a plain service depend on MVC, and meant it could only ever be
    /// called from a controller. Reporting the outcome to the user is the caller's job.
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

        /// Describes a result for the user. Lives here so the wording stays with the
        /// behaviour that produces it, rather than being duplicated at each call site.
        public static string Describe(CaptchaServiceResult result)
        {
            switch (result)
            {
                case CaptchaServiceResult.Failes:
                    return "Error in evaluation of captcha by google";
                case CaptchaServiceResult.Robot:
                    return "google knows you as a robot.";
                default:
                    return null;
            }
        }

        public async Task<CaptchaServiceResult> VerifyCaptchaAsync(string captchaResponse)
        {
            string secretkey = configuration["Recaptcha:SecretKey"];
            HttpClient httpclient = httpClientFactory.CreateClient();
            var res = await httpclient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretkey}&response={captchaResponse}");
            if (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return CaptchaServiceResult.Failes;
            }
            else
            {
                string json = await res.Content.ReadAsStringAsync();
                RecaptchaModel recaptchaModel = Newtonsoft.Json.JsonConvert.DeserializeObject<RecaptchaModel>(json);
                if (recaptchaModel.success == false)
                {
                    return CaptchaServiceResult.Robot;
                }

            }
            return CaptchaServiceResult.Human;
        }
    }
}
