using Application.ContractMapping;
using Application.Dtos;
using Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Address
{
    public class AddressService : IAddressService
    {
        private readonly EmployeeAppDbContext _context;

        public AddressService(EmployeeAppDbContext context)
        {
            _context = context;
        }

        public async Task<AddressDto> AddAddressAsync(CreateAddressDto dto)
        {

            var existing = await _context.Addresses.FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId);
            if (existing != null)
            {
                throw new InvalidOperationException("This employee already has an address.");
            }
            var address = dto.ToModel();
          await  _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();
            return address.ToDto();
        }

        public async Task RemoveEmployeeAddressAsync(Guid employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Address)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee != null && employee.Address != null)
            {
                _context.Addresses.Remove(employee.Address);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dtos.AddressDto?> GetAddressByEmployeeIdAsync(Guid employeeId)
        {
            var address = await _context.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId);

            if (address == null) return null;

            return new Dtos.AddressDto
            {
                EmployeeId = address.EmployeeId,
                Street = address.Street,
                City = address.City,
                State = address.State
            };
        }

        public async Task UpdateAddressAsync(Dtos.AddressDto dto)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId);
            if (address == null) return;

            address.Street = dto.Street;
            address.City = dto.City;
            address.State = dto.State;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAddressAsync(UpdateAddressDto dto)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId);

            if (address == null)
                return false;

            address.Street = dto.Street;
            address.City = dto.City;
            address.State = dto.State;
            address.Country = dto.Country;

            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteAddressAsync(Guid employeeId)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.EmployeeId == employeeId);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }
        }

        public List<string> GetAllStates()
        {
            return _context.States
                .OrderBy(s => s.Name)
                .Select(s => s.Name)
                .ToList();
        }
    }
}
