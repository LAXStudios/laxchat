using laxchat;
using OllamaSharp;
using Spectre.Console;

AnsiConsole.Clear();

try
{
    var uri = new Uri("http://localhost:11434");
    var ollama = new OllamaApiClient(uri);

    var markdown = new MarkdownConverter();

    await new ChatConsole(ollama!, markdown!).Run();
}
catch (Exception ex)
{
    AnsiConsole.MarkupLineInterpolated($"[{OllamaConsole.ErrorTextColor}]{Markup.Escape(ex.Message)}[/]");
    AnsiConsole.WriteLine();
}