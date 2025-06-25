using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Filaments.Http;

[Route("api/filaments")]
[ApiController]
public class DeleteFilament : ControllerBase
{
    private readonly IMediator _mediator;

    public DeleteFilament(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteFilamentCommand { Id = id });
        return success ? NoContent() : NotFound();
    }
}

public class DeleteFilamentCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteFilamentHandler : IRequestHandler<DeleteFilamentCommand, bool>
{
    private readonly PolyBucketDbContext _context;

    public DeleteFilamentHandler(PolyBucketDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteFilamentCommand request, CancellationToken cancellationToken)
    {
        var filament = await _context.Filaments.FindAsync(request.Id);

        if (filament == null)
        {
            return false;
        }

        _context.Filaments.Remove(filament);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
} 