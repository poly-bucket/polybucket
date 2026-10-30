using PolyBucket.Api.Features.Models.RecordModelView.Domain;

namespace PolyBucket.Api.Features.Models.RecordModelView.Http;

public class RecordModelViewResponseDto
{
    public int Views { get; set; }
    public bool Counted { get; set; }

    public static RecordModelViewResponseDto From(RecordModelViewOutcome outcome) => new()
    {
        Views = outcome.Views,
        Counted = outcome.Counted
    };
}
