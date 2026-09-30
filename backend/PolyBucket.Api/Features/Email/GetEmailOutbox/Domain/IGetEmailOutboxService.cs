using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Features.Email.Domain;

namespace PolyBucket.Api.Features.Email.GetEmailOutbox.Domain;

public class EmailOutboxItemDto
{
    public Guid Id { get; set; }
    public EmailTemplateKey Template { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public EmailMessageStatus Status { get; set; }
    public int Attempts { get; set; }
    public DateTime NextAttemptAt { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
}

public class EmailOutboxPageDto
{
    public List<EmailOutboxItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public Dictionary<EmailMessageStatus, int> StatusCounts { get; set; } = [];
}

public interface IGetEmailOutboxService
{
    Task<EmailOutboxPageDto> GetAsync(EmailMessageStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
}
