// Services/ProjectService.cs
using Microsoft.EntityFrameworkCore;
using TeamDesk.Data;
using TeamDesk.DTOs;
using TeamDesk.Enum;
using TeamDesk.Models.Entities;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Services
{
    public class clientService : IClientService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<clientService> _logger;

        public clientService(AppDbContext context, ILogger<clientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ClientResponse>> GetAllClientsAsync()
        {
            try
            {
                var clients = await _context.Clients
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return clients.Select(c => new ClientResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    ContactPerson = c.ContactPerson,
                    ContactEmail = c.ContactEmail,
                    ContactPhone = c.ContactPhone,
                    Address = c.Address
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clients");
                throw;
            }
        }

        public async Task<ClientResponse> CreateClientAsync(CreateClientRequest request)
        {
            try
            {
                // Check if client with same email exists
                var existingClient = await _context.Clients
                    .FirstOrDefaultAsync(c => c.ContactEmail == request.ContactEmail);

                if (existingClient != null)
                {
                    throw new InvalidOperationException($"Client with email {request.ContactEmail} already exists");
                }

                var client = new Client
                {
                    Name = request.Name,
                    Description = request.Description,
                    ContactEmail = request.ContactEmail,
                    ContactPhone = request.ContactPhone,
                    ContactPerson = request.ContactPerson,
                    Address = request.Address,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new client: {ClientName} with ID {ClientId}",
                    client.Name, client.Id);

                return new ClientResponse
                {
                    Id = client.Id,
                    Name = client.Name,
                    ContactPerson = client.ContactPerson,
                    ContactEmail = client.ContactEmail,
                    ContactPhone = client.ContactPhone,
                    Address = client.Address
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                throw;
            }
        }

        public async Task<ClientResponse?> GetClientByIdAsync(Guid id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null || !client.IsActive)
                return null;

            return new ClientResponse
            {
                Id = client.Id,
                Name = client.Name,
                ContactPerson = client.ContactPerson,
                ContactEmail = client.ContactEmail,
                ContactPhone = client.ContactPhone,
                Address = client.Address,
            };
        }


        public async Task<bool> UpdateClientAsync(Guid id, CreateClientRequest request)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null || !client.IsActive)
                return false;

            client.Name = request.Name;
            client.ContactPerson = request.ContactPerson;
            client.ContactEmail = request.ContactEmail;
            client.ContactPhone = request.ContactPhone;

            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteClientAsync(Guid id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null || !client.IsActive)
                return false;

            client.IsActive = false; // Soft delete
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
