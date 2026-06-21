using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_Connect_LoginAndRegister_API.Data;
using NGO_Connect_LoginAndRegister_API.Models;

namespace NGO_Connect_LoginAndRegister_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly P01NgoConnectContext _context;

        public AdminController(P01NgoConnectContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DASHBOARD METRICS
        // ==========================================

        [HttpGet("pending-count")]
        public async Task<ActionResult<int>> GetPendingCount()
        {
            int count = await _context.Users.CountAsync(u => u.AccountStatus == "Under Verification");
            return Ok(count);
        }

        [HttpGet("total-users-count")]
        public async Task<ActionResult> GetTotalUsersCount()
        {
            var baseQuery = _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName != "Admin" && u.AccountStatus != "Under Verification");

            var total = await baseQuery.CountAsync();
            var ngoCount = await baseQuery.CountAsync(u => u.Role.RoleName == "NGO");
            var donorCount = await baseQuery.CountAsync(u => u.Role.RoleName == "Donor");
            var beneficiaryCount = await baseQuery.CountAsync(u => u.Role.RoleName == "Beneficiary");

            return Ok(new
            {
                total = total,
                ngos = ngoCount,
                donors = donorCount,
                beneficiaries = beneficiaryCount
            });
        }

        // ==========================================
        // 2. USER APPROVAL WORKFLOW
        // ==========================================

        [HttpGet("pending-users")]
        public async Task<ActionResult<IEnumerable<object>>> GetPendingUsers()
        {
            var users = await _context.Users
                .Where(u => u.AccountStatus == "Under Verification")
                .Include(u => u.Role)
                .Include(u => u.State)
                .Include(u => u.City)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Email,
                    u.PhoneNo,
                    u.PanNo,
                    u.AccountStatus,
                    RoleName = u.Role.RoleName,
                    Location = u.City.CityName + ", " + u.State.StateName
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("approve/{userId}")]
        public async Task<ActionResult> ApproveUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.AccountStatus = "Active";
            await _context.SaveChangesAsync();

            return Ok(new { message = "User approved successfully" });
        }

        [HttpDelete("reject/{userId}")]
        public async Task<ActionResult> RejectUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User request rejected and record deleted" });
        }

        // ==========================================
        // 3. DONATION CATEGORY MANAGEMENT
        // ==========================================

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<object>>> GetCategories()
        {
            var categories = await _context.Items
                .Select(i => new { i.ItemId, i.ItemName })
                .ToListAsync();
            return Ok(categories);
        }

        [HttpPost("add-category")]
        public async Task<ActionResult> AddCategory([FromBody] CategoryDto request)
        {
            if (string.IsNullOrWhiteSpace(request.ItemName))
                return BadRequest(new { message = "Category name is required." });

            if (await _context.Items.AnyAsync(i => i.ItemName == request.ItemName))
                return BadRequest(new { message = "Category already exists." });

            var newItem = new Item { ItemName = request.ItemName };
            _context.Items.Add(newItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category added successfully." });
        }

        [HttpDelete("delete-category/{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound(new { message = "Category not found." });

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully." });
        }

        // ==========================================
        // 4. BROADCAST MESSAGE MANAGEMENT
        // ==========================================

        [HttpGet("broadcasts")]
        public async Task<ActionResult<IEnumerable<object>>> GetBroadcasts()
        {
            var broadcasts = await _context.Notifications
                .Include(n => n.TargetRole)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Subject,
                    n.Message,
                    n.TargetRoleId,
                    RoleName = n.TargetRoleId == null ? "All Users" : n.TargetRole.RoleName,
                    n.CreatedAt
                })
                .ToListAsync();

            return Ok(broadcasts);
        }

        [HttpPost("broadcast")]
        public async Task<ActionResult> BroadcastMessage([FromBody] NotificationDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Subject and Message are required." });

            var notification = new Notification
            {
                Subject = request.Subject,
                Message = request.Message,
                TargetRoleId = request.TargetRoleId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Broadcast sent successfully!" });
        }

        [HttpPut("edit-broadcast/{id}")]
        public async Task<ActionResult> EditBroadcast(int id, [FromBody] NotificationDto request)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound("Broadcast not found.");

            notification.Subject = request.Subject;
            notification.Message = request.Message;
            notification.TargetRoleId = request.TargetRoleId;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Broadcast updated successfully!" });
        }

        [HttpDelete("delete-broadcast/{id}")]
        public async Task<ActionResult> DeleteBroadcast(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound("Broadcast not found.");

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Broadcast deleted successfully!" });
        }

        // ==========================================
        // 5. ADMIN ACCOUNT MANAGEMENT (NEW)
        // ==========================================

        [HttpGet("me/{id}")]
        public async Task<ActionResult> GetAdminDetails(int id)
        {
            var admin = await _context.Users.FindAsync(id);
            if (admin == null) return NotFound("Admin not found.");

            return Ok(new
            {
                admin.UserId,
                admin.Username,
                admin.Email,
                admin.PhoneNo,
                // Do not return password for security
            });
        }

        [HttpPut("update-profile/{id}")]
        public async Task<ActionResult> UpdateAdminProfile(int id, [FromBody] AdminProfileDto request)
        {
            var admin = await _context.Users.FindAsync(id);
            if (admin == null) return NotFound("Admin not found.");

            // Update basic details
            admin.Username = request.Username;
            admin.Email = request.Email;
            admin.PhoneNo = request.PhoneNo;

            // Only update password if a new one is provided
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                admin.Password = request.Password; // In a real app, hash this!
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully!" });
        }

        [HttpDelete("delete-account/{id}")]
        public async Task<ActionResult> DeleteAdminAccount(int id)
        {
            var admin = await _context.Users.FindAsync(id);
            if (admin == null) return NotFound("Admin not found.");

            // Prevent deleting the last admin if necessary, but for now allow it
            _context.Users.Remove(admin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Account deleted successfully." });
        }
    }

    public class CategoryDto { public string ItemName { get; set; } }

    public class NotificationDto
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public int? TargetRoleId { get; set; }
    }

    public class AdminProfileDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string? Password { get; set; } // Optional
    }
}