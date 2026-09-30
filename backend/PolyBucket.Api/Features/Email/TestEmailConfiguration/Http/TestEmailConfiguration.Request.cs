using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Email.TestEmailConfiguration.Http;

public class TestEmailConfigurationRequest
{
    [Required(ErrorMessage = "Test email address is required")]
    [EmailAddress(ErrorMessage = "Test email address must be a valid email address")]
    [MaxLength(320)]
    public string TestEmailAddress { get; set; } = string.Empty;
}
