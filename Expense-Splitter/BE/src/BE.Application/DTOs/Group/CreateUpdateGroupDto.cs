using System.ComponentModel.DataAnnotations;

namespace BE.Application.DTOs.Group;

public class CreateGroupDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = Common.CommonData.Currency.VND;
}

public class UpdateGroupDto
{
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [StringLength(3)]
    public string Currency { get; set; }

    public bool? IsActive { get; set; }
}