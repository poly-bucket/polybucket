using MediatR;

namespace PolyBucket.Api.Features.Filaments.Commands;

public class DeleteFilamentCommand : IRequest<bool>
{
    public Guid Id { get; set; }
} 