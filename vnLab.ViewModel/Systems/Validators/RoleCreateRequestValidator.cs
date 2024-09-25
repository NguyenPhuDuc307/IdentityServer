using FluentValidation;

namespace vnLab.ViewModel.Systems.Validators
{
    public class RoleCreateRequestValidator : AbstractValidator<RoleCreateRequest>
    {
        public RoleCreateRequestValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Role Id is required")
                .MaximumLength(50).WithMessage("Role Id must be less than 50 characters")
                .MinimumLength(5).WithMessage("Role Id must be greater than 5 characters");

            RuleFor(x => x.Name).NotEmpty().WithMessage("Role Name is required");
        }
    }
}