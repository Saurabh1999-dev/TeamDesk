using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamDesk.DTOs;
using TeamDesk.Migrations;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientService clientService, ILogger<ClientController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        /// <summary>
        /// Get all clients
        /// </summary>
        [HttpGet("clients")]
        public async Task<ActionResult<List<ClientResponse>>> GetAllClients()
        {
            try
            {
                var clients = await _clientService.GetAllClientsAsync();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clients");
                return StatusCode(500, new { message = "An error occurred while retrieving clients" });
            }
        }

        /// <summary>
        /// Create new client
        /// </summary>
        [HttpPost("clients")]
        [Authorize]
        public async Task<ActionResult<ClientResponse>> CreateClient([FromBody] CreateClientRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                var client = await _clientService.CreateClientAsync(request);
                return Ok(client);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return StatusCode(500, new { message = "An error occurred while creating client" });
            }
        }

        /// <summary>
        /// Get client by ID
        /// </summary>
        [HttpGet("clients/{id}")]
        public async Task<ActionResult<ClientResponse>> GetClientById(Guid id)
        {
            try
            {
                var client = await _clientService.GetClientByIdAsync(id);
                if (client == null)
                    return NotFound();

                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client by ID");
                return StatusCode(500, new { message = "An error occurred while retrieving the client" });
            }
        }

        /// <summary>
        /// Update an existing client
        /// </summary>
        [HttpPut("clients/{id}")]
        public async Task<IActionResult> UpdateClient(Guid id, [FromBody] CreateClientRequest request)
        {
            try
            {
                var result = await _clientService.UpdateClientAsync(id, request);
                if (!result)
                    return NotFound();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client");
                return StatusCode(500, new { message = "An error occurred while updating the client" });
            }
        }

        /// <summary>
        /// Delete a client
        /// </summary>
        [HttpDelete("clients/{id}")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            try
            {
                var result = await _clientService.DeleteClientAsync(id);
                if (!result)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client");
                return StatusCode(500, new { message = "An error occurred while deleting the client" });
            }
        }

    }
}