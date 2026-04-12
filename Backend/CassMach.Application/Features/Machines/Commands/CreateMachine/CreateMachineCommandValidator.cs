using FluentValidation;

namespace CassMach.Application.Features.Machines.Commands.CreateMachine
{
    public class CreateMachineCommandValidator : AbstractValidator<CreateMachineCommand>
    {
        public CreateMachineCommandValidator()
        {
            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("Marka zorunludur")
                .MaximumLength(100).WithMessage("Marka 100 karakterden fazla olamaz");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model zorunludur")
                .MaximumLength(100).WithMessage("Model 100 karakterden fazla olamaz");
        }
    }
}
