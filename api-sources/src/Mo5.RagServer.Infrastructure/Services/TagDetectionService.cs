using System.Text.RegularExpressions;
using Mo5.RagServer.Core.Entities;
using Mo5.RagServer.Core.Services;

namespace Mo5.RagServer.Infrastructure.Services;

/// <summary>
/// Rule-based tag detection service
/// </summary>
public class TagDetectionService : ITagDetectionService
{
    private static readonly Dictionary<string, List<TagPattern>> TagPatterns = new()
    {
        ["C"] = new()
        {
            new TagPattern(@"\b(#include|printf|scanf|malloc|free|int\s+main|void\s+main)\b", 0.9f, "C language keywords"),
            new TagPattern(@"\.c\b", 0.8f, "C file extension"),
            new TagPattern(@"\bc\b.*\b(programming|language|code)\b", 0.7f, "C programming context"),
            new TagPattern(@"\b(gcc|clang|compiler)\b", 0.6f, "C compiler references")
        },
        ["Assembly"] = new()
        {
            new TagPattern(@"\b(asm|assembly|assembleur)\b", 0.9f, "Assembly keywords"),
            new TagPattern(@"\b(6809|motorola)\b", 0.9f, "6809 processor"),
            new TagPattern(@"\b(lda|sta|jmp|jsr|rts|bra|beq|bne)\b", 0.8f, "6809 instructions"),
            new TagPattern(@"\.s\b|\.asm\b", 0.8f, "Assembly file extensions"),
            new TagPattern(@"\$[0-9A-Fa-f]+\b", 0.6f, "Hexadecimal addresses")
        },
        ["Basic"] = new()
        {
            new TagPattern(@"\b(basic|BASIC)\b", 0.9f, "BASIC keyword"),
            new TagPattern(@"\b(PRINT|INPUT|FOR|NEXT|IF|THEN|GOTO|GOSUB|RETURN)\b", 0.8f, "BASIC commands"),
            new TagPattern(@"\.bas\b", 0.8f, "BASIC file extension"),
            new TagPattern(@"\b(line\s+number|POKE|PEEK)\b", 0.7f, "BASIC programming concepts")
        },
        ["text-mode"] = new()
        {
            new TagPattern(@"\b(text\s*mode|mode\s*texte)\b", 0.9f, "Text mode explicit"),
            new TagPattern(@"\b(character|caractère|char|text|texte)\b.*\b(display|affichage|screen|écran)\b", 0.7f, "Text display context"),
            new TagPattern(@"\b(console|terminal|cursor)\b", 0.6f, "Text interface elements"),
            new TagPattern(@"\b(80\s*x\s*25|40\s*x\s*25)\b", 0.8f, "Text mode resolutions")
        },
        ["graphics-mode"] = new()
        {
            new TagPattern(@"\b(graphics?\s*mode|mode\s*graphique)\b", 0.9f, "Graphics mode explicit"),
            new TagPattern(@"\b(pixel|bitmap|sprite|graphics?|graphique)\b", 0.7f, "Graphics concepts"),
            new TagPattern(@"\b(draw|plot|line|circle|rectangle)\b", 0.6f, "Drawing operations"),
            new TagPattern(@"\b(320\s*x\s*200|160\s*x\s*200)\b", 0.8f, "Graphics mode resolutions"),
            new TagPattern(@"\b(palette|color|couleur)\b", 0.6f, "Color/palette references")
        },
        ["hardware"] = new()
        {
            new TagPattern(@"\b(hardware|matériel|register|registre)\b", 0.8f, "Hardware keywords"),
            new TagPattern(@"\b(memory\s*map|carte\s*mémoire|I/O|port)\b", 0.8f, "Hardware mapping"),
            new TagPattern(@"\b(ROM|RAM|PIA|VIA|ACIA)\b", 0.9f, "Hardware components"),
            new TagPattern(@"\b(interrupt|interruption|IRQ|NMI)\b", 0.8f, "Hardware interrupts"),
            new TagPattern(@"\$[A-Fa-f0-9]{4}\b", 0.6f, "Hardware addresses")
        },
        ["tools"] = new()
        {
            new TagPattern(@"\b(tools?|outils?|compiler?|compilateur)\b", 0.8f, "Tools keywords"),
            new TagPattern(@"\b(gcc|make|cmake|build|compilation)\b", 0.8f, "Build tools"),
            new TagPattern(@"\b(debugger?|débogueur|emulator|émulateur)\b", 0.8f, "Development tools"),
            new TagPattern(@"\b(install|installation|setup|configuration)\b", 0.6f, "Setup instructions")
        },
        ["examples"] = new()
        {
            new TagPattern(@"\b(example|exemple|sample|échantillon)\b", 0.8f, "Example keywords"),
            new TagPattern(@"\b(tutorial|tutoriel|guide|how\s*to)\b", 0.7f, "Tutorial content"),
            new TagPattern(@"```", 0.6f, "Code blocks"),
            new TagPattern(@"\b(demo|demonstration|test)\b", 0.6f, "Demo content")
        }
    };

    public Task<List<DetectedTag>> DetectTagsAsync(string fileName, string content, List<Tag> availableTags)
    {
        var detectedTags = new List<DetectedTag>();
        var lowerFileName = fileName.ToLowerInvariant();
        var lowerContent = content.ToLowerInvariant();

        foreach (var tag in availableTags.Where(t => t.IsActive))
        {
            var confidence = CalculateTagConfidence(tag.Name, lowerFileName, lowerContent);
            
            if (confidence > 0.5f) // Only include tags with reasonable confidence
            {
                detectedTags.Add(new DetectedTag
                {
                    Tag = tag,
                    Confidence = confidence,
                    Source = "auto",
                    Reason = GetDetectionReason(tag.Name, lowerFileName, lowerContent)
                });
            }
        }

        // Sort by confidence descending
        detectedTags.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

        return Task.FromResult(detectedTags);
    }

    private float CalculateTagConfidence(string tagName, string fileName, string content)
    {
        if (!TagPatterns.TryGetValue(tagName, out var patterns))
            return 0f;

        float maxConfidence = 0f;
        var combinedText = $"{fileName} {content}";

        foreach (var pattern in patterns)
        {
            if (Regex.IsMatch(combinedText, pattern.Pattern, RegexOptions.IgnoreCase))
            {
                maxConfidence = Math.Max(maxConfidence, pattern.Confidence);
            }
        }

        // Boost confidence for filename matches
        if (fileName.Contains(tagName.ToLowerInvariant()))
        {
            maxConfidence = Math.Min(1.0f, maxConfidence + 0.2f);
        }

        // Apply frequency boost for multiple matches
        var matchCount = patterns.Count(p => Regex.IsMatch(combinedText, p.Pattern, RegexOptions.IgnoreCase));
        if (matchCount > 1)
        {
            maxConfidence = Math.Min(1.0f, maxConfidence + (matchCount - 1) * 0.1f);
        }

        return maxConfidence;
    }

    private string GetDetectionReason(string tagName, string fileName, string content)
    {
        if (!TagPatterns.TryGetValue(tagName, out var patterns))
            return "Unknown";

        var combinedText = $"{fileName} {content}";
        var matchedPatterns = patterns
            .Where(p => Regex.IsMatch(combinedText, p.Pattern, RegexOptions.IgnoreCase))
            .OrderByDescending(p => p.Confidence)
            .Take(2)
            .ToList();

        if (matchedPatterns.Any())
        {
            return string.Join(", ", matchedPatterns.Select(p => p.Reason));
        }

        return "Pattern match";
    }

    private class TagPattern
    {
        public string Pattern { get; }
        public float Confidence { get; }
        public string Reason { get; }

        public TagPattern(string pattern, float confidence, string reason)
        {
            Pattern = pattern;
            Confidence = confidence;
            Reason = reason;
        }
    }
}
