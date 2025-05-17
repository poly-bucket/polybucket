using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Core.Interfaces;
using Core.Models.Roles;

namespace Api.Controllers.Roles
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController(
        IRoleService roleService,
        ILogger<RolesController> logger) : ControllerBase
    {
        private readonly IRoleService _roleService = roleService;
        private readonly ILogger<RolesController> _logger = logger;

        // GET: api/roles
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles(CancellationToken cancellationToken)
        {
            var roles = await _roleService.GetAllRolesAsync(cancellationToken);
            return Ok(roles);
        }

        // GET: api/roles/{id}
        [HttpGet("{id}/:id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoleDto>> GetRole([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var role = await _roleService.GetRoleByIdAsync(id, cancellationToken);
            if (role == null)
                return NotFound();

            return Ok(role);
        }

        // POST: api/roles
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RoleDto>> CreateRole(
            [FromBody] CreateRoleRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdRole = await _roleService.CreateRoleAsync(request, cancellationToken);
                return CreatedAtAction(
                    nameof(GetRole),
                    new { id = createdRole.Id },
                    createdRole);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating role");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while creating the role" });
            }
        }

        // PUT: api/roles/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoleDto>> UpdateRole(
            Guid id,
            [FromBody] UpdateRoleRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedRole = await _roleService.UpdateRoleAsync(id, request, cancellationToken);
                return Ok(updatedRole);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating role with ID {roleId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role with ID {roleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while updating the role" });
            }
        }

        // DELETE: api/roles/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(id, cancellationToken);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting role with ID {roleId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role with ID {roleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while deleting the role" });
            }
        }
    }
}