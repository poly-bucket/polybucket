using PolyBucket.Api.Features.Models.RecordModelView.Domain;

namespace PolyBucket.Api.Features.Models.Http;

public class ModelViewResponse
{
    public int Views { get; set; }
    public bool Counted { get; set; }

    public static ModelViewResponse From(RecordModelViewOutcome outcome) => new()
    {
        Views = outcome.Views,
        Counted = outcome.Counted
    };
}
