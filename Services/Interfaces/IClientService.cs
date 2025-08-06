using TeamDesk.DTOs;

namespace TeamDesk.Services.Interfaces
{
    public interface IClientService
    {
        Task<List<ClientResponse>> GetAllClientsAsync();
        Task<ClientResponse> CreateClientAsync(CreateClientRequest request);
        Task<ClientResponse?> GetClientByIdAsync(Guid id);
        Task<bool> UpdateClientAsync(Guid id, CreateClientRequest request);
        Task<bool> DeleteClientAsync(Guid id);
    }
}
