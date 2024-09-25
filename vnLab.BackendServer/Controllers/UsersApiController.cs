using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vnLab.BackendServer.Authorization;
using vnLab.BackendServer.Constants;
using vnLab.BackendServer.Data;
using vnLab.BackendServer.Data.Entities;
using vnLab.BackendServer.Helpers;
using vnLab.ViewModel;
using vnLab.ViewModel.Systems;

namespace vnLab.BackendServer.Controllers;

public class UsersController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _mapper = mapper;
    }

    [HttpPost]
    [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.CREATE)]
    [ApiValidationFilter]
    public async Task<IActionResult> PostUser(UserCreateRequest request)
    {
        var user = new User()
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            Dob = request.Dob != null ? DateTime.Parse(request.Dob) : DateTime.Now,
            UserName = request.UserName,
            LastName = request.LastName,
            FirstName = request.FirstName,
            PhoneNumber = request.PhoneNumber,
            CreateDate = DateTime.Now
        };

        if (request.Password == null)
            return BadRequest(new ApiBadRequestResponse("Password is required"));

        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, request);
        }
        else
        {
            return BadRequest(new ApiBadRequestResponse(result));
        }
    }

    private static UserViewModel CreateUserViewModel(User user)
    {
        return new UserViewModel()
        {
            Id = user.Id,
            UserName = user.UserName,
            Dob = user.Dob,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreateDate = user.CreateDate,
            Avatar = user.Avatar
        };
    }

    [HttpGet]
    [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
    public async Task<IActionResult> GetUsers()
    {
        var users = _userManager.Users;

        var userViewModels = await users.Select(u => new UserViewModel()
        {
            Id = u.Id,
            UserName = u.UserName,
            Dob = u.Dob,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            FirstName = u.FirstName,
            LastName = u.LastName,
            CreateDate = u.CreateDate,
            Avatar = u.Avatar,
            Description = u.Description
        }).ToListAsync();

        return Ok(userViewModels);
    }

    [HttpGet("filter")]
    [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
    public async Task<IActionResult> GetUsersPaging(string? keyword, int page = 1, int pageSize = 10)
    {
        var query = _userManager.Users;

        if (!String.IsNullOrEmpty(keyword))
        {
            query = query.Where(x =>
                (x.UserName != null && x.UserName.Contains(keyword))
                || (x.Email != null && x.Email.Contains(keyword))
                || (x.FirstName != null && x.FirstName.Contains(keyword))
                || ((x.FirstName + " " + x.LastName) != null && (x.FirstName + " " + x.LastName).Contains(keyword))
                || (x.LastName != null && x.LastName.Contains(keyword))
                || (x.PhoneNumber != null && x.PhoneNumber.Contains(keyword)));
        }

        var item = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => new UserViewModel()
        {
            Id = x.Id,
            UserName = x.UserName,
            Dob = x.Dob,
            Email = x.Email,
            PhoneNumber = x.PhoneNumber,
            FirstName = x.FirstName,
            LastName = x.LastName,
            CreateDate = x.CreateDate,
            Avatar = x.Avatar,
            Description = x.Description
        }).ToListAsync();

        var totalRecords = await query.CountAsync();

        var pagination = new Pagination<UserViewModel>
        {
            Items = item,
            TotalRecords = totalRecords
        };

        return Ok(pagination);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));

        var userViewModel = new UserViewModel()
        {
            Id = user.Id,
            UserName = user.UserName,
            Dob = user.Dob,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreateDate = user.CreateDate,
            Avatar = user.Avatar,
            Description = user.Description
        };
        return Ok(userViewModel);
    }
    [HttpPut("{id}")]
    [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
    public async Task<IActionResult> PutUser(string id, [FromBody] UserCreateRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Dob = request.Dob != null ? DateTime.Parse(request.Dob) : DateTime.Now;
        user.LastModifiedDate = DateTime.Now;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return NoContent();
        }
        return BadRequest(new ApiBadRequestResponse(result));
    }

    [HttpPut("{id}/change-password")]
    [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
    public async Task<IActionResult> PutUserPassword(string id, [FromBody] UserPasswordChangeRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));

        if (request.CurrentPassword == null || request.NewPassword == null)
            return BadRequest(new ApiBadRequestResponse("The current password and new password cannot be null."));
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (result.Succeeded)
        {
            return NoContent();
        }
        return BadRequest(new ApiBadRequestResponse(result));
    }

    [HttpDelete("{id}")]
    [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.DELETE)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var adminUsers = await _userManager.GetUsersInRoleAsync(Constants.SystemConstants.Roles.Admin);
        var otherUsers = adminUsers.Where(x => x.Id != id).ToList();
        if (otherUsers.Count == 0)
        {
            return BadRequest(new ApiBadRequestResponse("You cannot remove the only admin user remaining."));
        }
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            var userViewModel = new UserViewModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                Dob = user.Dob,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreateDate = user.CreateDate,
                Avatar = user.Avatar,
                Description = user.Description
            };
            return Ok(userViewModel);
        }
        return BadRequest(new ApiBadRequestResponse(result));
    }

    [HttpGet("{userId}/menu")]
    public async Task<IActionResult> GetMenuByUserPermission(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {userId}"));
        }
        var roles = await _userManager.GetRolesAsync(user);
        var query = from f in _context.Functions
                    join p in _context.Permissions
                        on f.Id equals p.FunctionId
                    join r in _roleManager.Roles on p.RoleId equals r.Id
                    join a in _context.Commands
                        on p.CommandId equals a.Id
                    where !string.IsNullOrEmpty(r.Name) && roles.Contains(r.Name) && a.Id == "VIEW"
                    select new FunctionViewModel
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Url = f.Url,
                        ParentId = f.ParentId,
                        SortOrder = f.SortOrder,
                        Icon = f.Icon
                    };
        var data = await query.Distinct()
            .OrderBy(x => x.ParentId)
            .ThenBy(x => x.SortOrder)
            .ToListAsync();
        return Ok(data);
    }

    [HttpGet("{userId}/roles")]
    [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
    public async Task<IActionResult> GetUserRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {userId}"));
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(roles);
    }

    [HttpPost("{userId}/roles")]
    [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
    public async Task<IActionResult> PostRolesToUserUser(string userId, [FromBody] RoleAssignRequest request)
    {
        if (request.RoleNames == null)
        {
            return BadRequest(new ApiBadRequestResponse("Role names cannot empty"));
        }
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {userId}"));
        var result = await _userManager.AddToRolesAsync(user, request.RoleNames);
        if (result.Succeeded)
            return Ok();

        return BadRequest(new ApiBadRequestResponse(result));
    }

    [HttpDelete("{userId}/roles")]
    [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
    public async Task<IActionResult> RemoveRolesFromUser(string userId, [FromQuery] RoleAssignRequest request)
    {
        if (request.RoleNames?.Length == 0)
        {
            return BadRequest(new ApiBadRequestResponse("Role names cannot empty"));
        }
        if (request.RoleNames!.Length == 1 && request.RoleNames[0] == Constants.SystemConstants.Roles.Admin)
        {
            return base.BadRequest(new ApiBadRequestResponse($"Cannot remove {Constants.SystemConstants.Roles.Admin} role"));
        }
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {userId}"));
        var result = await _userManager.RemoveFromRolesAsync(user, request.RoleNames);
        if (result.Succeeded)
            return Ok();

        return BadRequest(new ApiBadRequestResponse(result));
    }
}