using FluentValidation;

namespace CassMach.Application.Features.UserMachines.Commands.AddUserMachine
{
    public class AddUserMachineCommandValidator : AbstractValidator<AddUserMachineCommand>
    {
        public AddUserMachineCommandValidator()
        {
            RuleFor(x => x.MachineId)
                .GreaterThan(0).WithMessage("Geçerli bir makine seçilmelidir");

            RuleFor(x => x.Name)
                .MaximumLength(150).WithMessage("Makine adı 150 karakterden fazla olamaz");
        }
    }
}
