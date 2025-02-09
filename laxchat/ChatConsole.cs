using System.Text;
using System.Text.RegularExpressions;
using OllamaSharp;
using Spectre.Console;

namespace laxchat;

public class ChatConsole(IOllamaApiClient ollama, MarkdownConverter markdown) : OllamaConsole(ollama)
{
    public override async Task Run()
    {
        WriteChatHeader();
        AnsiConsole.WriteLine();
        
        ollama.SelectedModel = "deepseek-r1:32b";

        var keepChatting = true;
        var systemPrompt = string.Empty;
        //systemPrompt = ReadInput($"Define a system prompt for {ollama.SelectedModel} [{HintTextColor}](optional)[/]");

        do
        {
            AnsiConsole.Clear();
            WriteChatHeader();
            WriteChatInstructionHint();
            
            var chat = new Chat(Ollama, systemPrompt);

            string message;

            do
            {
                AnsiConsole.WriteLine();
                message = ReadInput();
                
                if (message.Equals(EXIT_COMMAND, StringComparison.OrdinalIgnoreCase))
                {
                    keepChatting = false;
                    break;
                }

                if (message.Equals(START_NEW_COMMAND, StringComparison.OrdinalIgnoreCase))
                {
                    keepChatting = true;
                    break;
                }

                if (message.Equals(DEBUG_COMMAND, StringComparison.OrdinalIgnoreCase))
                {
                    keepChatting = true;
                    //message = "Please write all Markdown Syntax down";
                    //message = "## Header";

                    const string markdown = """
                                      <think>
                                      fhweioufwhp
                                      fwiujofwiof
                                      vijowefuoijwfgw
                                      fewiofwio
                                      </think>

                                      # Header 1
                                      ## Header 2
                                      ### Header 3
                                      #### **Header 4**
                                      ##### Header 5
                                      
                                      > paragraph 1
                                      
                                      ```csharp
                                      public static void main(String[] args)
                                      {
                                        Console.Clear();
                                      }
                                      ```
                                      
                                      ```bash
                                      dotnet new
                                      ```
                                      
                                      
                                      ```bash
                                      dotnet new
                                      dotnet run
                                      ```
                                      
                                      ---
                                      
                                      Just some **text** blabab **bold**
                                      blajhtf
                                      fhewifwo [quackquckren](https://duckduckgo.com)
                                      fhweiofow
                                      hfweiofwo
                                      
                                      *italic*
                                      
                                      - test
                                      - test2
                                      - test3
                                      - test4
                                      
                                      > paragraph multiline
                                      > multiline
                                      
                                      
                                      - Here some code:
                                        ```python
                                        let something
                                        ```
                                      - Test
                                      """;
                    
                    try
                    {
                        AnsiConsole.Write(new MarkdownView(markdown));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    Console.ReadLine();
                }
                
                List<string> tokens = new();

                await foreach (var answerToken in chat.SendAsync(message))
                {
                    AnsiConsole.MarkupInterpolated($"[grey58]{answerToken}[/]");
                    
                    tokens.Add(answerToken);
                }
                
                StringBuilder stringBuilder = new();
                foreach (var token in tokens)
                {
                    stringBuilder.Append(token);
                }
                
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                AnsiConsole.Clear();
                WriteChatHeader();
                WriteChatInstructionHint();
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[{AccentTextColor}]> [/]{message}");
                AnsiConsole.WriteLine();
                
                try
                {
                    AnsiConsole.Write(new MarkdownView(stringBuilder.ToString()));
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteException(ex, ExceptionFormats.Default);
                }
                
                
                AnsiConsole.WriteLine();
            } while(!string.IsNullOrWhiteSpace(message));
        } while (keepChatting);
    }
}