using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Options;

namespace PolyBucket.Api.Common.Email;

public class EmailOptionsValidator : IValidateOptions<EmailOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailOptions options)
    {
        var failures = new List<string>();

        if (options.Smtp.Port is < 1 or > 65535)
        {
            failures.Add("Email:Smtp:Port must be between 1 and 65535.");
        }

        if (options.Smtp.TimeoutSeconds is < 1 or > 600)
        {
            failures.Add("Email:Smtp:TimeoutSeconds must be between 1 and 600.");
        }

        if (!string.IsNullOrWhiteSpace(options.Smtp.PasswordFile) && !File.Exists(options.Smtp.PasswordFile))
        {
            failures.Add($"Email:Smtp:PasswordFile '{options.Smtp.PasswordFile}' does not exist.");
        }

        if (!string.IsNullOrWhiteSpace(options.Smtp.Password) && !string.IsNullOrWhiteSpace(options.Smtp.PasswordFile))
        {
            failures.Add("Set either Email:Smtp:Password or Email:Smtp:PasswordFile, not both.");
        }

        if (!string.IsNullOrWhiteSpace(options.FromAddress) && !EmailAddressValidator.IsValid(options.FromAddress))
        {
            failures.Add("Email:FromAddress must be a valid email address.");
        }

        if (!string.IsNullOrWhiteSpace(options.ReplyTo) && !EmailAddressValidator.IsValid(options.ReplyTo))
        {
            failures.Add("Email:ReplyTo must be a valid email address.");
        }

        if (!string.IsNullOrWhiteSpace(options.PublicBaseUrl) && !Uri.TryCreate(options.PublicBaseUrl, UriKind.Absolute, out _))
        {
            failures.Add("Email:PublicBaseUrl must be an absolute URL, for example https://models.example.com.");
        }

        var dispatcher = options.Dispatcher;
        if (dispatcher.BatchSize < 1 || dispatcher.MaxAttempts < 1 || dispatcher.PollSeconds < 1 || dispatcher.LockSeconds < 10)
        {
            failures.Add("Email:Dispatcher values must be positive, and LockSeconds must be at least 10.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
