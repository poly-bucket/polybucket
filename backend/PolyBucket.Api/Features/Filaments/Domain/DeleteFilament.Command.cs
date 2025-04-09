using MediatR;

namespace PolyBucket.Api.Features.Filaments.Domain;

public class DeleteFilamentCommand : IRequest<bool>
{
    public Guid Id { get; set; }
} 