using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Sarnakh.Api.Constants;
using Sarnakh.Api.Data;
using Sarnakh.Api.Dtos;

namespace Sarnakh.Api.Endpoints.Handlers;

public class RegisterHandler
{
    public static async Task<IResult> Handler(
            RegisterDto registerDto,
            AppDbContext dbContext,
            UserManager<ApplicationUser> UserManager,
            IValidator<RegisterDto> validator)
    {
        var validationResult = await validator.ValidateAsync(registerDto);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email
        };

        var createUserResult = await UserManager.CreateAsync(user, registerDto.Password);

        if (!createUserResult.Succeeded)
        {
            return Results.BadRequest(createUserResult.Errors);
        }

        var addToRoleResult = await UserManager.AddToRoleAsync(user, Roles.Member);
        if (!addToRoleResult.Succeeded)
        {
            return Results.BadRequest(addToRoleResult.Errors);
        }

        await transaction.CommitAsync();
        return Results.Ok();
    }
}
