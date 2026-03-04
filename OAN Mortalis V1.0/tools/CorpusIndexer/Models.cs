namespace CorpusIndexer;

internal sealed record ScannedDocument(
    string Title,
    string Hash,
    IReadOnlyList<string> Keywords,
    string Content);

internal sealed record ConceptHit(
    string DocumentHash,
    string Concept,
    string Domain);

internal sealed record EngramNode(
    string Id,
    string Concept,
    string Domain,
    string Source,
    string Hash);

internal sealed record EngramLink(
    string From,
    string To,
    string Relation);
