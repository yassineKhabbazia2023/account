using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Pulse.Account.API.Controllers
{
    [Route("api/role-labels")]
    [ApiController]
    public class RoleLabelController : ControllerBase
    {
        private readonly IRoleLabelService _roleLabelService;

        public RoleLabelController(IRoleLabelService roleLabelService)
        {
            _roleLabelService = roleLabelService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoleLabelAsync([FromBody] RoleLabel roleLabel)
        {
            await _roleLabelService.AddRoleLabelAsync(roleLabel);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRoleLabelAsync([FromBody] RoleLabelDeleteRequest request)
        {
            await _roleLabelService.DeleteRoleLabelAsync(request.AccountId, request.ContactId, request.LabelId);
            return Ok();
        }
    }
}
