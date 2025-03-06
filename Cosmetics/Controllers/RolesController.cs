using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Cosmetics.Models;
using Cosmetics.DTO;

namespace ComedicShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")] 
    public class RolesController : ControllerBase
    {
        private readonly ComedicShopDBContext _context;

        public RolesController(ComedicShopDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(Role model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data.");

            _context.Roles.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Role created successfully." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, Role model)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound("Role not found.");

            role.RoleId = model.RoleId;
            role.RoleName = model.RoleName;
            role.Description = model.Description;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Role updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound("Role not found.");

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Role deleted successfully." });
        }
    }
}
