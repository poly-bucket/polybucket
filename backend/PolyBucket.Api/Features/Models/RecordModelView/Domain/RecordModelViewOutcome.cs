namespace PolyBucket.Api.Features.Models.RecordModelView.Domain;

public enum RecordModelViewOutcomeKind
{
    NotFound,
    Forbid,
    Ok
}

public sealed class RecordModelViewOutcome
{
    public RecordModelViewOutcomeKind Kind { get; }
    public int Views { get; }
    public bool Counted { get; }

    private RecordModelViewOutcome(RecordModelViewOutcomeKind kind, int views, bool counted)
    {
        Kind = kind;
        Views = views;
        Counted = counted;
    }

    public static RecordModelViewOutcome NotFound() =>
        new(RecordModelViewOutcomeKind.NotFound, 0, false);

    public static RecordModelViewOutcome Forbid() =>
        new(RecordModelViewOutcomeKind.Forbid, 0, false);

    public static RecordModelViewOutcome Ok(int views, bool counted) =>
        new(RecordModelViewOutcomeKind.Ok, views, counted);
}
