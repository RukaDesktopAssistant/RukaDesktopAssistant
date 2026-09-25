namespace RukaDesktopAssistant.Services.AI;

public sealed class LocalCorpusExpander
{
    private static readonly string[] Prefixes =
    [
        "", "ねえ、", "ちょっと、", "るか、", "あのさ、", "そういえば、",
        "今、", "とりあえず、", "ちなみに、", "ていうか、"
    ];

    private static readonly string[] Suffixes =
    [
        "", "？", "かな", "なんだけど", "って感じ", "ってこと？",
        "できる？", "お願い", "教えて", "今どう？", "どうしよう", "ｗ"
    ];

    private static readonly string[] ToneHints =
    [
        "", "ちょっと", "かなり", "めっちゃ", "とりあえず", "一旦",
        "ちゃんと", "もう一回", "できれば", "今すぐ"
    ];

    private static readonly string[] ResponsePrefixes =
    [
        "", "うん、", "なるほど、", "OK、", "よし、", "えっと、",
        "それなら、", "じゃあ、"
    ];

    private static readonly string[] ResponseSuffixes =
    [
        "", "ｗ", "〜", "だね", "しよ", "いこう", "で大丈夫だよ"
    ];

    public const int TargetVirtualEntries = 1_000_000;

    public IEnumerable<(string Trigger, string Response, string Category)> Expand(
        string trigger,
        string response,
        string category,
        int max = 256)
    {
        if (string.IsNullOrWhiteSpace(trigger) || string.IsNullOrWhiteSpace(response))
            yield break;

        var produced = 0;

        for (var p = 0; p < Prefixes.Length && produced < max; p++)
        for (var t = 0; t < ToneHints.Length && produced < max; t++)
        for (var s = 0; s < Suffixes.Length && produced < max; s++)
        {
            var modifier = ToneHints[t];
            var prefix = Prefixes[p];
            var suffix = Suffixes[s];

            var expandedTrigger = $"{prefix}{modifier}{trigger}{suffix}".Trim();
            if (string.IsNullOrWhiteSpace(expandedTrigger))
                continue;

            for (var rp = 0; rp < ResponsePrefixes.Length && produced < max; rp++)
            for (var rs = 0; rs < ResponseSuffixes.Length && produced < max; rs++)
            {
                var expandedResponse = $"{ResponsePrefixes[rp]}{response}{ResponseSuffixes[rs]}".Trim();
                if (string.IsNullOrWhiteSpace(expandedResponse))
                    continue;

                produced++;
                yield return (expandedTrigger, expandedResponse, category);
            }
        }
    }

    public IEnumerable<(string Trigger, string Response, string Category)> ExpandCandidates(
        IReadOnlyList<(string Trigger, string Response, string Category)> seeds,
        string userText,
        int max = 256)
    {
        var keys = ExtractKeys(userText);
        var yielded = 0;

        foreach (var seed in seeds)
        {
            if (keys.Count > 0 && !ExtractKeys(seed.Trigger).Overlaps(keys))
                continue;

            foreach (var item in Expand(seed.Trigger, seed.Response, seed.Category, max: 64))
            {
                yield return item;
                if (++yielded >= max)
                    yield break;
            }
        }
    }

    private static HashSet<string> ExtractKeys(string text)
    {
        var normalized = text.Trim().ToLowerInvariant();
        var keys = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i + 1 < normalized.Length; i++)
        {
            var pair = normalized.Substring(i, 2);
            if (pair.Any(char.IsLetterOrDigit) || pair.Any(c => c >= 0x3040))
                keys.Add(pair);
        }

        return keys;
    }
}
