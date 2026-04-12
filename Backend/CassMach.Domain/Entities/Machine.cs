namespace CassMach.Domain.Entities
{
    public class Machine : BaseAuditableEntity
    {
        public string Brand { get; set; }
        public string Model { get; set; }

        public ICollection<ErrorSolution> ErrorSolutions { get; set; }
        public ICollection<UserMachine> UserMachines { get; set; }

        public Machine()
        {
            ErrorSolutions = new HashSet<ErrorSolution>();
            UserMachines = new HashSet<UserMachine>();
        }
    }
}
