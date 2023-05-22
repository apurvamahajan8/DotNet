using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task.Models;

using BCryptNet = BCrypt.Net.BCrypt;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Task.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Task.Data;
using Task.Models;

namespace Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private TaskContext context = new TaskContext();
      
        [HttpGet]
        [Authorize]
        [Route("getAllEmployees")]
        public IActionResult Get()
        {
            List<Employee> employees = context.Employees
        .Select(e => new Employee
        {
            Id = e.Id,
            Name = e.Name,
            Email = e.Email,
            Role = e.Role
        })
        .ToList();

            return Ok(employees);
        }

        [HttpPost]
       // [Authorize(Roles = "admin")]
        [Route("register")]
        public IActionResult RegisterUser([FromBody] Employee employee)
        {
            try
            {
                if (employee.Role != "admin" && employee.Role != "non-admin")
                {
                    ModelState.AddModelError("Role", "Invalid role. Role must be either 'admin' or 'non-admin'.");
                    return new BadRequestObjectResult(new { errors = ModelState }) { StatusCode = 400 };

                }
                //if(IsAdmin())
                //{
                bool emailExists = context.Employees.Any(e => e.Email == employee.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return new BadRequestObjectResult(ModelState) { StatusCode = 400 };
                }

                string hashedPassword = BCryptNet.HashPassword(employee.Password);

                var user = new Employee
                {
                    Name = employee.Name,
                    Email = employee.Email,
                    Role = employee.Role,
                    Password = hashedPassword
                };


                context.Employees.Add(user);
                context.SaveChanges();

                return Ok(user);



            }
            catch (Exception ex)
            {

                return new BadRequestObjectResult(ex.Message) { StatusCode = 403 };
                //StatusCode(500, "An error occurred while registering the user.");
            }
        }

        //[ApiController]
        //[Route("api/user")]
        //public class UserController : Controller
        //{
        //    private readonly Assignment_1Context _context;
        //    private readonly JwtService _jwtService;
        //    private readonly IConfiguration _configuration;
        //    public UserController(Assignment_1Context context, JwtService jwtService, IConfiguration configuration)
        //    {
        //        _context = context;
        //        _jwtService = jwtService;
        //        _configuration = configuration;
        //    }

        //    [Authorize(Roles = "Admin")]
        //    public void AddUser(string Username)
        //    {
        //        try
        //        {
        //            var user = new User
        //            {
        //                Username = Username,
        //                Password = "123456",
        //                Role = "User"
        //            };
        //            _context.Users.Add(user);
        //            _context.SaveChanges();
        //            Console.WriteLine("USER ADDED");
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //    }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginRequest user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            var userInDb = context.Employees.FirstOrDefault(u => u.Email == user.Email);
            if (userInDb == null)
            {
                return NotFound();
            }
            bool isPasswordValid = BCryptNet.Verify(user.Password, userInDb.Password);

            if (!isPasswordValid)
            {
                return BadRequest("Invalid password");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, userInDb.Email),
                new Claim(ClaimTypes.Role, userInDb.Role),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is my custom Secret key for authentication"));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "JWTAuthenticationServer",
                audience: "JWTServicePostmanClient",
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: signIn);
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            Console.WriteLine("JWT TOKEN : " + jwtToken);
            var response = new
            {
                token = jwtToken,
                user = userInDb
            };
            return Ok(response);
        }

        [HttpPut]
        [Authorize(Roles = "admin")]
        [Route("update/{Id}")]
        public IActionResult UpdateContact([FromRoute] int Id, UpdateEmployee employee)
        {
            //if (IsAdmin())
            //{
            var user = context.Employees.Find(Id);
            if (employee == null)
            {
                return NotFound();
            }
            if (user == null)
            {
                return NotFound();
            }
            user.Email = employee.Email;
            user.Name = employee.Name;
            user.Role = employee.Role;
            context.SaveChanges();
            return Ok(user);
            //}
            return NotFound();
        }

        [HttpDelete]
        [Authorize(Roles = "admin")]
        [Route("delete/{Id}")]
        public IActionResult Delete(int Id)
        {
            //if(IsAdmin())
            //{
            var employee = context.Employees.Find(Id);

            if (employee == null)
            {
                return NotFound();
            }
            context.Employees.Remove(employee);
            context.SaveChanges();
            return Ok(employee);
            //}
            return NotFound();
        }
    }
}