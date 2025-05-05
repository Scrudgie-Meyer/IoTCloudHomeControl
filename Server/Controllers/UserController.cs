using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data.DBManager;
using Server.Data.Entities;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DBSetup _context;

        public UserController(DBSetup context)
        {
            _context = context;
        }

        // CREATE: api/user
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest("User data is invalid.");
            }

            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email || u.Username == user.Username);

            if (existingUser != null)
            {
                return Conflict("User with the same email or username already exists.");
            }

            // Add new user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }

        // GET: api/user/by-email/{email}
        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }

        // PUT: api/user/{id} (Update user information)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (updatedUser == null || id != updatedUser.Id)
            {
                return BadRequest("Invalid user data.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update user fields
            user.Username = updatedUser.Username ?? user.Username;
            user.Email = updatedUser.Email ?? user.Email;
            user.PasswordHash = updatedUser.PasswordHash ?? user.PasswordHash;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent(); // Success, no content to return
        }

        // POST: api/user/authenticate
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] LoginRequest login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
                return BadRequest("Invalid login data.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (user == null)
                return Unauthorized("Invalid credentials.");

            if (!user.IsEmailConfirmed)
                return Forbid("Email not confirmed.");

            var hasher = new PasswordHasher<User>();
            var verificationResult = hasher.VerifyHashedPassword(user, user.PasswordHash, login.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid credentials.");

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.IsAdmin
            });
        }


        // PUT: api/user/{id}/update-status (Update Hidden, IsEmailConfirmed)
        [HttpPut("{id}/update-status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UserStatusUpdate statusUpdate)
        {
            if (statusUpdate == null)
            {
                return BadRequest("Invalid status update data.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.IsEmailConfirmed = statusUpdate.IsEmailConfirmed ?? user.IsEmailConfirmed;
            user.Hidden = statusUpdate.Hidden ?? user.Hidden;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent(); // Success, no content to return
        }

        // PUT: api/user/{id}/update-admin (Update IsAdmin field)
        [HttpPut("{id}/update-admin")]
        public async Task<IActionResult> UpdateAdminStatus(int id, [FromBody] bool isAdmin)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.IsAdmin = isAdmin;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent(); // Success, no content to return
        }
    }


    // DTO to update user status (Hidden and IsEmailConfirmed)
    public class UserStatusUpdate
    {
        public bool? IsEmailConfirmed { get; set; }
        public bool? Hidden { get; set; }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

}
