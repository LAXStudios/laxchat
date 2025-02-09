using System.Text.RegularExpressions;
using Spectre.Console;

string output = @"
<think>
Okay, so I need to understand all the Markdown syntax. I've heard people talk about using it for formatting text on
platforms like GitHub or Reddit, but I'm not entirely sure how each part works. Let me start by recalling what I know
and then figure out what I might be missing.

First off, headings are probably done with the # symbol. So, one # would make a big heading, two ## for a slightly
smaller one, and so on. But does it go up to six levels? I think so because HTML has six heading levels. So that should
cover all sizes from h1 to h6.
</think>";

string header = @"# Header fwefbhwjieiw
fwejhfuwihfwuifwhou
fwehfuwfuw
fwjkfjwiefghwoi

# Header 2
fwjifwiofwh
nfjiowefuiwf

## Header 3
fewihfiowhfw0
frewhjifowhfiowfwu9i

## Header 4
fewfhwuifwiufw
fhu9wefhuiwefwui
fhiowefhoiwefrt

### Header 5
fhwuifhw9
fhujiwefuiwe
";

AnsiConsole.Markup($"[blue]{Conert(header)}[/]");

Console.ReadLine();

static string Conert(string markdown)
{
    markdown = Regex.Replace(markdown, @"<think>(.*?)<\/think>", "[grey58]$1[/]", RegexOptions.Singleline);
    markdown = Regex.Replace(markdown, @"^# (.*)", $"[red]$1[/]", RegexOptions.Multiline);
    markdown = Regex.Replace(markdown, @"^#{2} (.*)", $"[red]$1[/]", RegexOptions.Multiline);
    markdown = Regex.Replace(markdown, @"^#{3} (.*)", $"[red]$1[/]", RegexOptions.Multiline);
    
    return markdown;
}