namespace CassMach.Domain.Entities
{
    public class Machine : BaseAuditableEntity
    {
        public int UserId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string? Name { get; set; }

        public User User { get; set; }
        public ICollection<ErrorSolution> ErrorSolutions { get; set; }

        public Machine()
        {
            ErrorSolutions = new HashSet<ErrorSolution>();
        }
    }
}
