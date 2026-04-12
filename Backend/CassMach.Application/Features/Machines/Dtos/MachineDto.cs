namespace CassMach.Application.Features.Machines.Dtos
{
    public class MachineDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
