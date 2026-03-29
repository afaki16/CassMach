using FluentValidation;

namespace CassMach.Application.Features.Admin.Commands.UpdateSetting
{
    public class UpdateSettingCommandValidator : AbstractValidator<UpdateSettingCommand>
    {
        public UpdateSettingCommandValidator()
        {
            RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Key is required.");

            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Value is required.");
        }
    }
}
