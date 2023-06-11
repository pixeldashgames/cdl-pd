
using System.Collections;

namespace cdl_pd;

public static class Program
{
    private const string QueryStartTag = "#queried";
    private const string QueryEndTag = "#endqueried";

    private static IEnumerable<string> TranspileRegions((string reg, bool isQueried)[] regions)
    {
        switch (regions.Length)
        {
            case 0:
                return Array.Empty<string>();
            case 1:
                if(!regions[0].isQueried)
                    return Array.Empty<string>();

                // TODO
                throw new NotImplementedException();
            default:
                return TranspileRegions(regions[..1]).Concat(TranspileRegions(regions[1..]));
        }
    }
    
    // TODO Make async
    private static void TranspileFiles(string[] files)
    {
        switch (files.Length)
        {
            // Base case
            case 0:
                return;
            case 1:
                var fileText = File.ReadAllText(files[0]);
                var regions = SplitIntoQueriedRegions(fileText);
                var transpiled = TranspileRegions(regions.ToArray());
                var newText = transpiled.Aggregate((acc, curr) => acc + curr);
                File.WriteAllText(files[0].Split('.')[0] + ".c", newText);
                break;
            default:
                // Transpile the first file
                TranspileFiles(files[..1]);
        
                // Transpile the rest of the files
                TranspileFiles(files[1..]);
                break;
        }
    }
    
    private static IEnumerable<(string reg, bool isQueried)> SplitIntoQueriedRegions(string text, bool inQueried = false)
    {
        if(text.Length == 0)
            return Array.Empty<(string, bool)>();
        
        var tag = inQueried ? QueryEndTag : QueryStartTag;
        var idx = text.IndexOf(tag, StringComparison.Ordinal);
        
        return idx switch
        {
            < 0 => new[] { (text, inQueried) },
            0 => SplitIntoQueriedRegions(text[tag.Length..], !inQueried),
            _ => new[] { (text[..idx], inQueried) }.Concat(SplitIntoQueriedRegions(text[(idx + tag.Length)..], !inQueried))
        };
    }

    private static void Main(string[] args)
    {
        var split = SplitIntoQueriedRegions(File.ReadAllText("../../../test.cdl"));
        
        Console.WriteLine(
            split.Aggregate<(string, bool), string>("", 
                (string acc, (string reg, bool isQ) curr) 
                    => $"{acc}\n{(curr.isQ ? "Queried" : "Not Queried")} Region :\n{curr.reg}"));
    }
}
