using FluentValidation;

namespace CassMach.Application.Features.Errors.Commands.AcceptSolution
{
    public class AcceptSolutionCommandValidator : AbstractValidator<AcceptSolutionCommand>
    {
        public AcceptSolutionCommandValidator()
        {
            RuleFor(x => x.ConversationId)
                .NotEmpty().WithMessage("ConversationId is required.");

            RuleFor(x => x.AttemptNumber)
                .GreaterThan(0).WithMessage("AttemptNumber must be greater than 0.");
        }
    }
}
