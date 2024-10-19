


using FluentValidation;
using Komikai_pilnas;
using Komikai_pilnas.Datat;
using Komikai_pilnas.Datat.Entities;
using Microsoft.Azure.Search.Models;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.Results;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Results;
using SharpGrip.FluentValidation.AutoValidation.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ForumDbContext>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation(configuration =>
{
    configuration.OverrideDefaultResultFactoryWith<ProblemDetailsResultFactory>();
});
builder.Services.AddResponseCaching();
var app = builder.Build();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.AddComedianApi();
app.AddSetsApi();
app.AddCommentsApi();
app.Run();

public record ComedianDto(int Id, string Name, string Description);

public record CreateComedianDto(string Name, string Description)
{
    public class CreateComedianDtoValidator : AbstractValidator<CreateComedianDto>
    {
        public CreateComedianDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().Length(min: 2, max: 80);
            RuleFor(x => x.Description).NotEmpty().Length(min: 5, max: 300);
        }
    }
}
public record UpdateComedianDto(string Description)
{
    public class UpdateComedianDtoValidator : AbstractValidator<UpdateComedianDto>
    {
        public UpdateComedianDtoValidator()
        {

            RuleFor(x => x.Description).NotEmpty().Length(min: 5, max: 300);
        }
    }
};

public record SetDto(int Id, string Title, string Body, DateTimeOffset CreationDate);
public record CreateSetDto(string Title, string Body)
{
    public class CreateSetDtoValidator : AbstractValidator<CreateSetDto>
    {
        public CreateSetDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().Length(min: 2, max: 80);
            RuleFor(x => x.Body).NotEmpty().Length(min: 5, max: 300);
        }
    }
}
public record UpdateSetDto(string Body)
{
    public class UpdateSetDtoValidator : AbstractValidator<UpdateSetDto>
    {
        public UpdateSetDtoValidator()
        {

            RuleFor(x => x.Body).NotEmpty().Length(min: 5, max: 300);
        }
    }
};





public record CommentDto(int Id, string Content, DateTimeOffset CreationDate, int Score);
public record CreateCommentDto(string Content,int Score)
{
    public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.Content).NotEmpty().Length(min: 5, max: 300);
            RuleFor(x => x.Score).NotEmpty().InclusiveBetween(1, 5);
        }
    }
}
public record UpdateCommentDto(string Content, int Score)
{
    public class UpdateCommentDtoValidator : AbstractValidator<UpdateCommentDto>
    {
        public UpdateCommentDtoValidator()
        {
            RuleFor(x => x.Content).NotEmpty().Length(min: 5, max: 300);
            RuleFor(x => x.Score).NotEmpty().InclusiveBetween(1, 5);
        }
    }
};


public class ProblemDetailsResultFactory : IFluentValidationAutoValidationResultFactory
{
    public IResult CreateResult(EndpointFilterInvocationContext contex, ValidationResult validationResult)
    {
        var problemDetails = new HttpValidationProblemDetails(validationResult.ToValidationProblemErrors())
        {
            Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
            Title = "Unprocessable Entity",
            Status = 422
        };
        return TypedResults.Problem(problemDetails);
    }
}

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (JsonException jsonEx)
        {
            await HandleJsonExceptionAsync(context, jsonEx);
        }
        catch (FluentValidation.ValidationException validationEx)
        {
            await HandleValidationExceptionAsync(context, validationEx);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleJsonExceptionAsync(HttpContext context, JsonException jsonEx)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = "Invalid JSON format: " + jsonEx.Message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return context.Response.WriteAsJsonAsync(problemDetails);
    }

    private Task HandleValidationExceptionAsync(HttpContext context, FluentValidation.ValidationException validationEx)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = string.Join(", ", validationEx.Errors.Select(e => e.ErrorMessage))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return context.Response.WriteAsJsonAsync(problemDetails);
    }

    private Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        // Log the unexpected exception (optional)
        var problemDetails = new ProblemDetails
        {
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = ex.Message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}