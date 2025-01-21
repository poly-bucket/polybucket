using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class Authentication : Controller
    {
        [HttpPost]
        [Route("login")]
        public IActionResult Login(string Username, string Password)
        {
            if (Username == "admin" && Password == "admin")
            {
                return Ok(new { token = "123456" });
            }
            return Unauthorized();
        }
    }
}