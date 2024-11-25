using Komikai_pilnas.Auth.Model;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace Komikai_pilnas.Datat.Entities;

public class Comment
{
    // Guid
    // Ulid
    public int Id { get; set; }
    public required string Content { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    
    public required int Score { get; set; }

    public Set Set { get; set; }

    [Required]
    public required string UserId { get; set; }

    public ForumUser User { get; set; }

    public CommentDto ToDto()
    {
        return new CommentDto(Id, Content, CreatedAt,Score);
    }

}
