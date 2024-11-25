using Komikai_pilnas.Datat.Entities;
using Komikai_pilnas.Datat;
using Microsoft.Azure.Search.Models;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Microsoft.EntityFrameworkCore;
using Komikai_pilnas.Auth.Model;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace Komikai_pilnas
{
    public static class Endpoints
    {
        public static void AddComedianApi(this WebApplication app)
        {
            var comediansGroup = app.MapGroup("/api").AddFluentValidationAutoValidation();

            comediansGroup.MapGet("/comedians", async ( ForumDbContext dbContext) =>
            {

                return (await dbContext.Comedians.ToListAsync()).Select(comedian => comedian.ToDto());

            });


            comediansGroup.MapGet("/comedians/{comedianId}", async (int comedianId, ForumDbContext dbContext) =>
            {
                var comedian = await dbContext.Comedians.FindAsync(comedianId);

                if (comedian == null)
                {
                    return Results.NotFound($"No comedian found with ID {comedianId}.");
                }

                return TypedResults.Ok(comedian.ToDto());
            });

            comediansGroup.MapPost("/comedians", [Authorize(Roles = ForumRoles.Admin)] async (HttpRequest request, CreateComedianDto dto, ForumDbContext dbContext, HttpContext httpContext) => {

                var comedian = new Comedian { Name = dto.Name, Description = dto.Description,UserId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)};
                dbContext.Comedians.Add(comedian);

                await dbContext.SaveChangesAsync();

                return TypedResults.Created($"api/comedians/{comedian.Id}", comedian.ToDto());

            });
            /* comediansGroup.MapPost("/comedians", async (HttpRequest request, ForumDbContext dbContext) =>
             {
                 try
                 {
                     // Try to deserialize the request body into CreateComedianDto
                     var dto = await request.ReadFromJsonAsync<CreateComedianDto>();

                     // If deserialization fails, throw an exception
                     if (dto == null)
                     {
                         return Results.BadRequest("Invalid JSON body. Please check the format of the request.");
                     }

                     var comedian = new Comedian { Name = dto.Name, Description = dto.Description };
                     dbContext.Comedians.Add(comedian);
                     await dbContext.SaveChangesAsync();

                     return TypedResults.Created($"api/comedians/{comedian.Id}", comedian.ToDto());
                 }
                 catch (System.Text.Json.JsonException jsonEx)
                 {
                     // Handle JSON deserialization issues
                     return Results.BadRequest("Invalid JSON format: " + jsonEx.Message);
                 }
                 catch (Exception ex)
                 {
                     // Handle any other exceptions
                     var errorResponse = new { error = "An unexpected error occurred", details = ex.Message };
                     return Results.Json(errorResponse, statusCode: StatusCodes.Status500InternalServerError);
                 }
             }).WithName("CreateComedian");*/

            comediansGroup.MapPut("/comedians/{comedianId}", [Authorize(Roles = ForumRoles.Admin)] async (UpdateComedianDto dto, int comedianId, ForumDbContext dbContext, HttpContext httpContext) =>
            {

                var comedian = await dbContext.Comedians.FindAsync(comedianId);
                if (comedian == null)
                {
                    return Results.NotFound($"No comedian found with ID {comedianId}.");
                }

                comedian.Description = dto.Description;

                dbContext.Comedians.Update(comedian);
                // await dbContext.Comedians.ToListAsync();
                await dbContext.SaveChangesAsync();

                return Results.Ok(comedian.ToDto());


            });

            comediansGroup.MapDelete("/comedians/{comedianId}", [Authorize(Roles = ForumRoles.Admin)]  async (int comedianId, ForumDbContext dbContext) =>
            {
                var comedian = await dbContext.Comedians.FindAsync(comedianId);
                if (comedian == null)
                {
                    return Results.NotFound($"Comedian with ID {comedianId} not found");
                }
                dbContext.Comedians.Remove(comedian);
                await dbContext.SaveChangesAsync();
                return Results.NoContent();
            });
        }

        public static void AddSetsApi(this WebApplication app)
        {
            var setsGroup = app.MapGroup("/api/comedians/{comedianId}").AddFluentValidationAutoValidation();

            setsGroup.MapGet("sets", async (int comedianId, ForumDbContext dbContext, CancellationToken cancellationToken) =>
            {

                var sets = await dbContext.Sets
                    .Where(set => set.Comedian.Id == comedianId)
                    .ToListAsync(cancellationToken);

                if (!sets.Any())
                {
                    return Results.NotFound($"No sets found for comedian with ID {comedianId}.");
                }
                return Results.Ok(sets.Select(set => set.ToDto()));

            });

            /* setsGroup.MapGet("sets", async (int comedianId, [AsParameters] SearchParameters searchParams, LinkGenerator linkGenerator, HttpContext httpContext, ForumDbContext dbContext, CancellationToken cancellationToken) =>
             {

                 var queryable = dbContext.Sets.Where(set => set.Comedian.Id == comedianId).AsQueryable().OrderBy(o => o.Title);

                 if (!queryable.Any())
                 {
                     return Results.NotFound($"No sets found for comedian with ID {comedianId}.");
                 }

                 var pagedList = await PagedList<Set>.CreateAsync(queryable, searchParams.PageNumber!.Value, searchParams.PageSize!.Value);


                 var paginationMetadata = pagedList.CreatePaginationMetadata(linkGenerator, httpContext, "GetSets");
                 httpContext.Response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationMetadata));

                 return Results.Ok(pagedList.Select(set => set.ToDto()));

             }).WithName("GetSets");*/

            setsGroup.MapGet("sets/{setId}", async (int comedianId, int setId, ForumDbContext dbContext, CancellationToken cancellationToken) =>
            {

                var comedian = await dbContext.Comedians.FirstOrDefaultAsync(t => t.Id == comedianId, cancellationToken);

                if (comedian == null)
                {
                    return Results.NotFound($"No comedian found with ID {comedianId}.");
                }

                var set = await dbContext.Sets.FirstOrDefaultAsync(p => p.Id == setId && p.Comedian.Id == comedianId, cancellationToken);
                if (set == null)
                {
                    return Results.NotFound($"No set found with ID {setId}.");
                }

                return Results.Ok(set.ToDto());
            });

            setsGroup.MapPost("/sets", [Authorize(Roles = ForumRoles.Admin)] async (HttpRequest request, int comedianId, CreateSetDto dto, ForumDbContext dbContext, HttpContext httpContext) => {


                var comedian = await dbContext.Comedians.FindAsync(comedianId);

                if (comedian == null)
                {
                    return Results.NotFound($"No comedian found with ID {comedianId}.");
                }

                var set = new Set { Title = dto.Title, Body = dto.Body, CreatedAt = DateTimeOffset.UtcNow, Comedian = comedian, UserId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) };
                dbContext.Sets.Add(set);

                await dbContext.SaveChangesAsync();

                return TypedResults.Created($"api/comedians/{comedianId}/sets/{set.Id}", set.ToDto());

            });

            setsGroup.MapPut("/sets/{setId}", [Authorize(Roles = ForumRoles.Admin)] async (HttpRequest request, int comedianId, int setId, UpdateSetDto dto, ForumDbContext dbContext) =>
            {

                var set = await dbContext.Sets.FirstOrDefaultAsync(s => s.Comedian.Id == comedianId && s.Id == setId);
                if (set == null)
                {
                    return Results.NotFound($"No set found with ID {setId}.");
                }


                set.Body = dto.Body;

                dbContext.Sets.Update(set);
                await dbContext.SaveChangesAsync();

                return Results.Ok(set.ToDto());

            });

            setsGroup.MapDelete("/sets/{setId}", [Authorize(Roles = ForumRoles.Admin)] async (int comedianId, int setId, ForumDbContext dbContext) =>
            {
                var set = await dbContext.Sets.FirstOrDefaultAsync(s => s.Comedian.Id == comedianId && s.Id == setId);
                if (set == null)
                {
                    return Results.NotFound($"No set found with ID {setId}.");
                }
                dbContext.Sets.Remove(set);
                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            });
        }

        public static void AddCommentsApi(this WebApplication app)
        {
            var commentsGroup = app.MapGroup("/api/comedians/{comedianId}/sets/{setId}").AddFluentValidationAutoValidation();

            commentsGroup.MapGet("comments", async (int comedianId, int setId, ForumDbContext dbContext, CancellationToken cancellationToken) =>
            {

                //return (await dbContext.Sets.ToListAsync(cancellationToken)).Select(set => set.ToDto());
                var set = await dbContext.Sets
                      .FirstOrDefaultAsync(set => set.Comedian.Id == comedianId && set.Id == setId, cancellationToken);

                if (set == null)
                {
                    return Results.NotFound($"Set with ID {setId} not found for comedian with ID {comedianId}.");
                }

                // Now, fetch the comments for this set
                var comments = await dbContext.Comments
                    .Where(comment => comment.Set.Id == setId)
                    .ToListAsync(cancellationToken);

                if (!comments.Any())
                {
                    return Results.NotFound($"No comments found for set with ID {setId}.");
                }
                return Results.Ok(comments.Select(comment => comment.ToDto()));

            });

            commentsGroup.MapGet("/comments/{commentId}", async (int comedianId, int setId, int commentId, ForumDbContext dbContext, CancellationToken cancellationToken) =>
            {
                // Checking if the comedian exists
                var comedian = await dbContext.Comedians.FirstOrDefaultAsync(c => c.Id == comedianId, cancellationToken);

                if (comedian == null)
                {
                    return Results.NotFound($"Comedian with ID {comedianId} not found.");
                }

                // Checking if the set exists for the comedian
                var set = await dbContext.Sets.FirstOrDefaultAsync(s => s.Id == setId && s.Comedian.Id == comedianId, cancellationToken);
                if (set == null)
                {
                    return Results.NotFound($"Set with ID {setId} not found for comedian with ID {comedianId}.");
                }
                //Fetch the specific comment

                var comment = await dbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.Set.Id == setId, cancellationToken);
                if (comment == null)
                {
                    return Results.NotFound($"Comment with ID {commentId} not found for set with ID {setId}.");
                }

                return Results.Ok(comment.ToDto());
            });

            commentsGroup.MapPost("/comments", [Authorize(Roles = ForumRoles.ForumUser)] async (HttpRequest request, int comedianId, int setId, CreateCommentDto dto, ForumDbContext dbContext, HttpContext httpContext) => {


                var comedian = await dbContext.Comedians.FindAsync(comedianId);

                if (comedian == null)
                {
                    return Results.NotFound($"Comedian with ID {comedianId} not found.");
                }

                var set = await dbContext.Sets.FirstOrDefaultAsync(s => s.Id == setId && s.Comedian.Id == comedianId);
                if (set == null)
                {
                    return Results.NotFound($"Set with ID {setId} not found for comedian with ID {comedianId}.");
                }

                var comment = new Comment { Content = dto.Content, CreatedAt = DateTimeOffset.UtcNow, Set = set, Score= dto.Score, UserId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) };
                dbContext.Comments.Add(comment);

                await dbContext.SaveChangesAsync();

                return TypedResults.Created($"/api/comedians/{comedianId}/sets/{setId}/comments/{comment.Id}", comment.ToDto());

            });

            commentsGroup.MapPut("/comments/{commentId}", [Authorize] async (int comedianId, int setId, int commentId, UpdateCommentDto dto, ForumDbContext dbContext,HttpContext httpContext) =>
            {

                var comedian = await dbContext.Comedians.FindAsync(comedianId);

                if (comedian == null)
                {
                    return Results.NotFound($"Comedian with ID {comedianId} not found.");
                }
                // Check if the comment exists and is associated with the given set and comedian
                var comment = await dbContext.Comments
                    .FirstOrDefaultAsync(c => c.Set.Id == setId && c.Set.Comedian.Id == comedianId && c.Id == commentId);

                if (comment == null)
                {
                    return Results.NotFound($"Comment with ID {commentId} not found");
                }

                if(!httpContext.User.IsInRole(ForumRoles.Admin)&& httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!= comment.UserId)
                {
                    return Results.Forbid();
                }
                comment.Content = dto.Content;
                comment.Score = dto.Score;

                dbContext.Comments.Update(comment);
                await dbContext.SaveChangesAsync();

                return Results.Ok(comment.ToDto());

            });

            commentsGroup.MapDelete("/comments/{commentId}", [Authorize] async (int comedianId, int setId, int commentId, ForumDbContext dbContext, HttpContext httpContext) =>
            {
                var comment = await dbContext.Comments.FirstOrDefaultAsync(c => c.Set.Id == setId && c.Set.Comedian.Id == comedianId && c.Id == commentId);
                if (comment == null)
                {
                    return Results.NotFound($"Comment with ID {commentId} not found");
                }
                if (!httpContext.User.IsInRole(ForumRoles.Admin) && httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != comment.UserId)
                {
                    return Results.Forbid();
                }
                dbContext.Comments.Remove(comment);
                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            });
        }
    }
}
