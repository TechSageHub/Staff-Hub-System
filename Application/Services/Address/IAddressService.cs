using Application.Dtos;

namespace Application.Services.Address
{
    public interface IAddressService
    {
        Task<AddressDto?> AddAddressAsync(CreateAddressDto dto);
        Task<AddressDto?> GetAddressByEmployeeIdAsync(Guid employeeId);
        Task<bool> UpdateAddressAsync(UpdateAddressDto dto);
        Task DeleteAddressAsync(Guid employeeId);
        List<string> GetAllStates();
    }
}
