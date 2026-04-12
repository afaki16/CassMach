namespace CassMach.Application.Features.UserMachines.Dtos
{
    public class UserMachineDto
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
