using FluentValidation;
using CassMach.Application.Features.Roles.Commands.CreateRole;
using CassMach.Application.Features.Roles.Commands.UpdateRole;
using CassMach.Application.Features.Roles.Commands.DeleteRole;

namespace CassMach.Application.Features.Roles.Commands.CreateRole
{
    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required.")
                .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
} 
