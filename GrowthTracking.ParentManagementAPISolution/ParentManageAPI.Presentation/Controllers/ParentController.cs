using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParentManageApi.Application.DTOs;
using ParentManageApi.Application.Interfaces;
using System.Security.Claims;
using GrowthTracking.ShareLibrary.Logs;
using ParentManagementAPI.Application.DTOs; 
using System.Collections.Generic;

namespace ParentManageApi.Presentation.Controllers
{
    [Route("api/parent")]
    [ApiController]
    public class ParentController(IParentService parentService) : ControllerBase
    {
        [HttpPost]
        // [Authorize]
        public async Task<IActionResult> CreateParent([FromBody] ParentDTO parentDTO)
        {
            var parentId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(parentId) || !Guid.TryParse(parentId, out var parsedParentId))
            {
                LogHandler.LogExceptions(new UnauthorizedAccessException($"Invalid or missing user token for CreateParent"));
                return Unauthorized(new ApiResponse(false, "Invalid or missing user token"));
            }

            LogHandler.LogToFile($"Starting CreateParent for ParentId: {parsedParentId}");
            var response = await parentService.CreateParentAsync(parentDTO, parsedParentId);
            if (response.Flag)
            {
                LogHandler.LogToConsole($"Successfully created parent with ParentId: {parsedParentId}");
                return Ok(new ApiResponse(true, response.Message, Array.Empty<object>()));
            }
            else
            {
                LogHandler.LogToDebugger($"Failed to create parent with ParentId: {parsedParentId}. Reason: {response.Message}");
                return BadRequest(new ApiResponse(false, response.Message, Array.Empty<object>()));
            }
        }

        [HttpPut]
        // [Authorize]
        public async Task<IActionResult> UpdateParent([FromBody] ParentDTO parentDTO)
        {
            var parentId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(parentId) || !Guid.TryParse(parentId, out var parsedParentId))
            {
                LogHandler.LogExceptions(new UnauthorizedAccessException($"Invalid or missing user token for UpdateParent"));
                return Unauthorized(new ApiResponse(false, "Invalid or missing user token"));
            }

            LogHandler.LogToFile($"Starting UpdateParent for ParentId: {parsedParentId}");
            var response = await parentService.UpdateParentAsync(parentDTO, parsedParentId);
            if (response.Flag)
            {
                LogHandler.LogToConsole($"Successfully updated parent with ParentId: {parsedParentId}");
                return Ok(new ApiResponse(true, response.Message, Array.Empty<object>()));
            }
            else
            {
                LogHandler.LogToDebugger($"Failed to update parent with ParentId: {parsedParentId}. Reason: {response.Message}");
                return BadRequest(new ApiResponse(false, response.Message, Array.Empty<object>()));
            }
        }

        [HttpGet("{parentId}")]
        // [Authorize]
        public async Task<IActionResult> GetParent([FromRoute] Guid parentId)
        {
            LogHandler.LogToFile($"Starting GetParent for ParentId: {parentId}");
            var parent = await parentService.GetParentAsync(parentId);
            if (parent == null)
            {
                LogHandler.LogToDebugger($"Parent with ParentId: {parentId} not found");
                return NotFound(new ApiResponse(false, "Parent not found", Array.Empty<object>()));
            }

            LogHandler.LogToConsole($"Successfully retrieved parent with ParentId: {parentId}");
            return Ok(new ApiResponse(true, "Parent found", parent));
        }

        [HttpGet]
        // [Authorize]
        public async Task<IActionResult> GetAllParents()
        {
            LogHandler.LogToFile("Starting GetAllParents");
            var parents = await parentService.GetAllParentsAsync();
            LogHandler.LogToConsole("Successfully retrieved all parents");
            return Ok(new ApiResponse(true, "Parents retrieved successfully", parents));
        }

        [HttpDelete("{parentId}")]
        // [Authorize]
        public async Task<IActionResult> DeleteParent([FromRoute] Guid parentId)
        {
            LogHandler.LogToFile($"Starting DeleteParent for ParentId: {parentId}");
            var response = await parentService.DeleteParentAsync(parentId);
            if (response.Flag)
            {
                LogHandler.LogToConsole($"Successfully deleted parent with ParentId: {parentId}");
                return Ok(new ApiResponse(true, response.Message, Array.Empty<object>()));
            }
            else
            {
                LogHandler.LogToDebugger($"Failed to delete parent with ParentId: {parentId}. Reason: {response.Message}");
                return NotFound(new ApiResponse(false, response.Message, Array.Empty<object>()));
            }
        }

        [HttpGet("{parentId}/children")]
        // [Authorize]
        public async Task<IActionResult> GetChildrenByParent(Guid parentId)
        {
            LogHandler.LogToFile($"Starting GetChildrenByParent for ParentId: {parentId}");
            
            // Since we don't have direct access to ChildApi from ParentManagement service,
            // we'll return a message guiding the user to use the ChildApi endpoint instead
            LogHandler.LogToConsole($"Redirecting children request to Child API for parent: {parentId}");
            
            return Ok(new ApiResponse(
                true, 
                "This endpoint is now handled by the Child API. Please use GET /api/child/parent/{parentId} endpoint.",
                Array.Empty<object>()
            ));
        }
    }
}