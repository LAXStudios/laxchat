using System.Text.RegularExpressions;

namespace laxchat;

public class MarkdownConverter
{
    public string Convert(string markdown)
    {
        var m = MarkdownParser.Parse(markdown);
        markdown = CleanUp(markdown);
        markdown = ExtractThinking(markdown);
        
        // Replace headers
        markdown = Regex.Replace(markdown, @"^# (.*)", $"[underline blue]$1[/]", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^#{2} (.*)", $"[underline blue]$1[/]", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^#{3} (.*)", $"[underline blue]$1[/]", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^#{4} (.*)", $"[underline blue]$1[/]", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^#{5} (.*)", $"[underline blue]$1[/]", RegexOptions.Multiline);

        // Replace bold text
        markdown = Regex.Replace(markdown, @"\*\*(.*?)\*\*", "[white]$1[/]");

        // Replace italic text
        markdown = Regex.Replace(markdown, @"\*(.*?)\*", "[italic]$1[/]");
        
        // Replace links
        markdown = Regex.Replace(markdown, @"\[(.*?)\]\((.*?)\)", "[link=$2][$1][/]");

        markdown = ExtractCode(markdown);
        
        return markdown;
    }

    private string CleanUp(string markdown)
    {
        // Replace "[" with "[[" and vise versa
        markdown = Regex.Replace(markdown, @"\[", "[[");
        markdown = Regex.Replace(markdown, @"\]", "]]");

        return markdown;
    }

    private string ExtractThinking(string markdown)
    {
        // Replace Think Section
        markdown = Regex.Replace(markdown, @"<think>(.*?)<\/think>", "[grey58]$1[/]", RegexOptions.Singleline);
        
        return markdown;
    }

    private string ExtractCode(string markdown)
    {
        // Replace code blocks
        markdown = Regex.Replace(markdown, @"```([\s\S]*?)```", "[green]$1[/]", RegexOptions.Singleline);
        
        return markdown;
    }
}