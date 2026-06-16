using System.ComponentModel.DataAnnotations;

public class TestModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}