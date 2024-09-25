using FluentValidation;

namespace vnLab.ViewModel.Systems.Validators
{
    public class UpdatePermissionRequestValidator : AbstractValidator<UpdatePermissionRequest>
    {
        public UpdatePermissionRequestValidator()
        {
            RuleFor(x => x.Permissions).NotNull()
                .WithMessage(string.Format(Messages.Required, nameof(UpdatePermissionRequest.Permissions)));

            RuleFor(x => x.Permissions).Must(x => x.Count > 0)
                .When(x => x.Permissions != null)
                .WithMessage(string.Format(Messages.Required, nameof(UpdatePermissionRequest.Permissions)));

            RuleForEach(x => x.Permissions).ChildRules(permission =>
            {
                permission.RuleFor(x => x.CommandId).NotEmpty()
                .WithMessage(string.Format(Messages.Required, nameof(PermissionViewModel.CommandId)));
                permission.RuleFor(x => x.FunctionId).NotEmpty()
               .WithMessage(string.Format(Messages.Required, nameof(PermissionViewModel.FunctionId)));
                permission.RuleFor(x => x.RoleId).NotEmpty()
               .WithMessage(string.Format(Messages.Required, nameof(PermissionViewModel.RoleId)));
            });
        }
    }
}