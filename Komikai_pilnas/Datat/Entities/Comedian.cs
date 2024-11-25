using Komikai_pilnas.Auth.Model;
using System.ComponentModel.DataAnnotations;

namespace Komikai_pilnas.Datat.Entities;

public class Comedian
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    [Required]
    public required string UserId { get; set; }

    public ForumUser User { get; set; }

    public ComedianDto ToDto()
    {
        return new ComedianDto(Id, Name, Description);
    }
}
