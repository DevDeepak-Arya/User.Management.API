using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using User.Management.API.Models;
using User.Management.API.Models.Authentication.SignUp;
using User.Management.Service.Models;
using User.Management.Service.Services;

namespace User.Management.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSvc _emailService;
        public AuthenticationController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,IConfiguration configuration, IEmailSvc emailService)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegisterUser registerUser,string role)
        {
            //Check User Exists
            var userExist = await _userManager.FindByEmailAsync(registerUser.Email!);
            if (userExist != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new Response { Status = "Error", Message = "User already exists!" });
            }

            //Add the User in the DB
            IdentityUser user = new()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.Username
            };
            if(await _roleManager.RoleExistsAsync(role))
            {
                var result = await _userManager.CreateAsync(user, registerUser.Password!);
                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                           new Response { Status = "Error", Message = "User Failed to Create!" });
                }

                //Assign a Role to the user

                await _userManager.AddToRoleAsync(user,role);
                return StatusCode(StatusCodes.Status200OK,
                          new Response { Status = "Success", Message = "User Created Successfully!" });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                           new Response { Status = "Error", Message = "This Role Does Not Exist!" });
            }

            


        }

        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            var message = new Message(new string[]
                {"dinesharya.97.97@gmail.com"}, "Test", "<h1>Hello from Deepak</h1>");

            _emailService.SendEmail(message);
            return StatusCode(StatusCodes.Status200OK,
                          new Response { Status = "Success", Message = "Email Sent Successfully!" });

        }

    }
}
