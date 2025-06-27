using Data.Context;
using Data.Model;


namespace Application.Services.StateServices
{
    public class StateService : IStateService
    {
        private readonly EmployeeAppDbContext _context;

        public StateService(EmployeeAppDbContext context)
        {
            _context = context;
        }

        public List<NigeriaState> GetAllStates()
        {
            return _context.States.ToList();
        }
    }
}
