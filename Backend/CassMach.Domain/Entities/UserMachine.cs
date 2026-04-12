namespace CassMach.Domain.Entities
{
    public class UserMachine : BaseAuditableEntity
    {
        public int UserId { get; set; }
        public int MachineId { get; set; }
        public string? Name { get; set; }

        public User User { get; set; }
        public Machine Machine { get; set; }
    }
}
