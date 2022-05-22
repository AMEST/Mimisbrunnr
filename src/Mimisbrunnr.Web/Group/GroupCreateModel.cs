using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace Mimisbrunnr.Web.Group;

public class GroupCreateModel
{
    [Required]
    public string Name { get; set; }

    public string Description { get; set; }
}