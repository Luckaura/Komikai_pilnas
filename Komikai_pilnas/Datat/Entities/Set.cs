﻿using Komikai_pilnas.Auth.Model;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Komikai_pilnas.Datat.Entities;

public class Set
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }

    public Comedian Comedian { get; set; }

    [Required]
    public required string UserId { get; set; }

    public ForumUser User { get; set; }

    public SetDto ToDto()
    {
        return new SetDto(Id, Title, Body, CreatedAt);
    }
}
