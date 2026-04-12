using FluentValidation;

namespace CassMach.Application.Features.Machines.Commands.UpdateMachine
{
    public class UpdateMachineCommandValidator : AbstractValidator<UpdateMachineCommand>
    {
        public UpdateMachineCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Geçerli bir makine ID'si gereklidir");

            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("Marka zorunludur")
                .MaximumLength(100).WithMessage("Marka 100 karakterden fazla olamaz");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model zorunludur")
                .MaximumLength(100).WithMessage("Model 100 karakterden fazla olamaz");

        }
    }
}
