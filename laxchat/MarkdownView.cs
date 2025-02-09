using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace laxchat;

 #region Model Classes
public enum MarkdownType
{
    Think,
    Header,
    Paragraph,
    Blockquote,
    CodeBlock,
    Image,
    Link,
    UnorderedList,
    OrderedList,
    ListWithCodeBlock,
    HorizontalRule,
}

public enum CodeLanguage
{
    Csharp,
    Python,
    C,
    Cpp,
    Bash
}

public class MarkdownBlock
{
    public required MarkdownType MarkdownType { get; set; }
    
    public string? Language { get; set; }
    
    public required string Content{ get; set; }
}
#endregion

public class MarkdownView : JustInTimeRenderable
{
    private readonly string markdown;
    List<MarkdownBlock> blocks = new();

    public MarkdownView(string markdown)
    {
        this.markdown = markdown;
        blocks = MarkdownParser.Parse(markdown);
    }

    private Style? HeaderStyle { get; set; } = Style.Plain.Foreground(Color.Blue);
    private Style? ParagraphStyle { get; set; } = Style.Plain.Foreground(Color.Grey70);

    IRenderable Build(MarkdownBlock block)
    {
        return block.MarkdownType switch
        {
            MarkdownType.Think => BuildThink(block),
            MarkdownType.Header =>  BuildHeader(block),
            MarkdownType.Paragraph => BuildParagraph(block),
            MarkdownType.Blockquote => BuildBlockquote(block),
            MarkdownType.CodeBlock => BuildCodeBlock(block),
            MarkdownType.Image => BuildImage(block),
            MarkdownType.Link => BuildLink(block),
            MarkdownType.UnorderedList => BuildUnorderedList(block),
            MarkdownType.OrderedList => BuildOrderedList(block),
            MarkdownType.ListWithCodeBlock => BuildListwithCodeBlock(block),
            MarkdownType.HorizontalRule => BuildHorizontalRule(block),
            _ => throw new NotImplementedException(),
        };
    }

    #region Build Renderables

    private IRenderable BuildThink(MarkdownBlock block)
    {
        return MarkdownToSpectreRenderables.Think(block);
    }
    
    private IRenderable BuildHeader(MarkdownBlock block)
    {
        return MarkdownToSpectreRenderables.Header(block);
    }

    private IRenderable BuildParagraph(MarkdownBlock block)
    {
        return MarkdownToSpectreRenderables.Paragraph(block);
    }

    private IRenderable BuildBlockquote(MarkdownBlock block)
    {
        return MarkdownToSpectreRenderables.Blockquote(block);
    }
    
    private IRenderable BuildCodeBlock(MarkdownBlock block)
    {
        return MarkdownToSpectreRenderables.CodeBlock(block);
    }
    
    private IRenderable BuildImage(MarkdownBlock block)
    {
        return new Text(block.Content, ParagraphStyle);
    }
    
    private IRenderable BuildLink(MarkdownBlock block)
    {
        return new Text(block.Content, ParagraphStyle);
    }
    
    private IRenderable BuildUnorderedList(MarkdownBlock block)
    {
        return MarkdownToSpectreRenderables.UnorderedList(block);
    }
    
    private IRenderable BuildOrderedList(MarkdownBlock block)
    {
        return MarkdownToSpectreRenderables.OrderedList(block);
    }

    private IRenderable BuildListwithCodeBlock(MarkdownBlock block)
    {
        return new Text(block.Content, ParagraphStyle);
    }
    
    private IRenderable BuildHorizontalRule(MarkdownBlock block)
    {
        return MarkdownToSpectreRenderables.HorizontalRule(block);
    }
    #endregion

    protected override IRenderable Build()
    {
        var elements = new List<IRenderable>();
        foreach (MarkdownBlock block in blocks)
        {
            elements.Add(Build(block));
        }

        return new Rows(elements);
    }
}

public static class MarkdownParser
{
    private static readonly List<Tuple<MarkdownType, Regex>> BlockTypes = new()
    {
        #region Patterns for block parsing
        // Thinking
        Tuple.Create(MarkdownType.Think, new Regex(@"<think>(.*?)", RegexOptions.Singleline)),
        
        // List with code block
        //Tuple.Create(MarkdownType.ListWithCodeBlock, new Regex(@"\s{1,4}```", RegexOptions.Singleline)),
        
        // Headers
        Tuple.Create(MarkdownType.Header, new Regex(@"^#{1,6} (.*)")),
        
        // Code-Block
        Tuple.Create(MarkdownType.CodeBlock, new Regex(@"```[\s\S]*?", RegexOptions.Singleline)),

        // Blockquote
        Tuple.Create(MarkdownType.Blockquote, new Regex(@"^>.+")),

        // UnorderedList
        Tuple.Create(MarkdownType.UnorderedList, new Regex(@"^- .+")),

        // Ordered List
        Tuple.Create(MarkdownType.OrderedList, new Regex(@"^\d+\. .+")),

        // Image
        Tuple.Create(MarkdownType.Image, new Regex(@"!\[.?\]\(.?\)")),

        // Link
        Tuple.Create(MarkdownType.Link, new Regex(@"\[.?\]\(.?\)")),

        //Horizontal Rule
        Tuple.Create(MarkdownType.HorizontalRule, new Regex(@"^-{3}")),
        
        // Paragraph
        Tuple.Create(MarkdownType.Paragraph, new Regex(@"^(.*?)(\r?\n|\Z)", RegexOptions.Multiline)),
        #endregion
    };

    public static List<MarkdownBlock> Parse(string markdown)
    {
        var blocks = new List<MarkdownBlock>();
        string[] lines = markdown.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
        int currentLine = 0;

        while (currentLine < lines.Length)
        { 
            foreach (var blockType in BlockTypes)
            {
                if (blockType.Item2.IsMatch(lines[currentLine]))
                {
                    var blockContent = new StringBuilder();
                    string language = String.Empty;
                    
                    // Parses for the "Think" block of the DeepSeek Model
                    if (blockType.Item1 == MarkdownType.Think && lines[currentLine].StartsWith("<think>"))
                    {
                        blockContent.AppendLine(lines[currentLine]);
                        currentLine++;

                        while (currentLine < lines.Length && !lines[currentLine].StartsWith('\t') && !lines[currentLine].StartsWith("</think>"))
                        {
                            blockContent.AppendLine(lines[currentLine]);
                            currentLine++;
                        }
                    }
                    
                    // Parses for Codeblocks that are multiple lines long
                    if (blockType.Item1 == MarkdownType.CodeBlock && lines[currentLine].StartsWith("```"))
                    {
                        
                        if (lines[currentLine].StartsWith("```"))
                        {
                            language = Regex.Replace(lines[currentLine], @"```(.*)", @"$1");
                            
                            lines[currentLine] = Regex.Replace(lines[currentLine], @"```(.*)", @"```");
                        }
                        
                        blockContent.AppendLine(lines[currentLine]);
                        currentLine++;

                        while (currentLine < lines.Length && !lines[currentLine].StartsWith('\t') && !lines[currentLine].StartsWith("```"))
                        {
                            blockContent.AppendLine(lines[currentLine]);
                            currentLine++;
                        }
                    }

                    // Parses for Blockquotes that are multiple lines long
                    if (blockType.Item1 == MarkdownType.Blockquote)
                    {
                        while (currentLine < lines.Length && lines[currentLine].StartsWith('>'))
                        {
                            blockContent.AppendLine(lines[currentLine]);
                            if(lines.Length != currentLine)
                                currentLine++;
                        }
                    }

                    // Parses for Lists that are multiple lines long
                    // Note:
                    // Parses as well for a code block but currently no Rendering
                    // TODO: Implement Rendering and better Parsing for in List item Code Blocks
                    if (blockType.Item1 == MarkdownType.UnorderedList || blockType.Item1 == MarkdownType.OrderedList)
                    {
                        while (currentLine < lines.Length &&
                               (((blockType.Item1 == MarkdownType.UnorderedList && lines[currentLine].StartsWith("- ")) ||
                                ((blockType.Item1 == MarkdownType.OrderedList && Regex.IsMatch(lines[currentLine], @"^\d+\. .+"))) || 
                                ((Regex.IsMatch(lines[currentLine], @"\s{1,4}```")) || lines[currentLine].StartsWith('\u0020')))))
                        {
                            var inListItemCodeBlock = new StringBuilder();
                            if (Regex.IsMatch(lines[currentLine], @"\s{1,4}```"))
                            {
                                inListItemCodeBlock.AppendLine(lines[currentLine]);
                                currentLine++;
                                while (currentLine < lines.Length && lines[currentLine].StartsWith('\u0020') && !Regex.IsMatch(lines[currentLine], @"\s{1,4}```"))
                                {
                                    inListItemCodeBlock.AppendLine(lines[currentLine]);
                                    currentLine++;
                                }
                                inListItemCodeBlock.AppendLine(lines[currentLine]);
                            }
                            
                            if(inListItemCodeBlock.ToString() != String.Empty)
                                blockContent.AppendLine(inListItemCodeBlock.ToString());
                            else
                                blockContent.AppendLine(lines[currentLine]);
                            currentLine++;
                        }
                    }

                    // Adds the blocks who are single lined like Header, Paragraphs etc.
                    if(lines.Length != currentLine)
                        blockContent.Append(lines[currentLine]);

                    blocks.Add(new MarkdownBlock
                    {
                        MarkdownType = blockType.Item1,
                        Content = blockContent.ToString(),
                        Language = language
                    });

                    break;
                }
            }
            currentLine++;
        }

        foreach (var block in blocks)
        {
            if (block.MarkdownType != MarkdownType.CodeBlock && block.MarkdownType != MarkdownType.Header && block.MarkdownType != MarkdownType.UnorderedList)
            {
                // Replace Markdown link with Spectre.Console link
                block.Content = Regex.Replace( MarkdownToSpectreRenderables.CleanUp(block.Content), @"\[(.*?)\]\((.*?)\)", "[link=$2][$1][/]");
                // Replace Markdown bold with Spectre.Console Bold
                block.Content = Regex.Replace(block.Content, @"\*\*(.*?)\*\*", "[white]$1[/]");
                // Replace Markdown italic with Spectre.Console italic
                block.Content = Regex.Replace(block.Content, @"\*(.*?)\*", "[italic]$1[/]");
                // Replace Markdown code with Spectre.Console Style
                block.Content = Regex.Replace(block.Content, @"`(.*?)`", "[grey70 on grey11] $1 [/]");
            }
            else if (block.MarkdownType == MarkdownType.Header)
            {
                // If a header is bolded or italic the markings will be removed
                block.Content = Regex.Replace(block.Content, @"\*\*(.*?)\*\*", "$1");
                block.Content = Regex.Replace(block.Content, @"\*(.*?)\*", "$1");
            }
        }
        
        return blocks;
    }
}

public static class MarkdownToSpectreRenderables
{
    #region Spectre.Console Styles

    private static Style? HeaderStyle { get; set; } = Style.Plain.Foreground(Color.Blue);
    private static Style? ParagraphStyle { get; set; } = Style.Plain.Foreground(Color.Grey70);
    private static Style? BlockquoteStyle { get; set; } = Style.Plain.Foreground(Color.Grey42);
    private static Style? ThinkStyle { get; set; } = Style.Plain.Foreground(Color.Grey54);

    #endregion

    public static IRenderable Think(MarkdownBlock block)
    {
        var panelThink = new Panel(
            Align.Left(
                new Markup(Regex.Replace(CleanUp(block.Content), @"<think>(.*?)<\/think>", "[italic grey58]$1[/]", RegexOptions.Singleline),
                    ThinkStyle)
            )
        );
        panelThink.Header = new PanelHeader("Thinking");
        
        return panelThink;
    }
    
    public static IRenderable Header(MarkdownBlock block)
    {
        return new Markup(Regex.Replace(block.Content, @"^#{1,6} (.*)", $"[underline blue]$1[/]", RegexOptions.Singleline), HeaderStyle);
    }

    public static IRenderable Paragraph(MarkdownBlock block)
    {
        if (block.Content != String.Empty)
            return new Markup(block.Content, ParagraphStyle);
        else
            return new Text(block.Content, ParagraphStyle);
    }

    public static IRenderable Blockquote(MarkdownBlock block)
    {
        return new Markup(Regex.Replace(block.Content, @"^> (.*)", "[grey23]\u2502[/] $1", RegexOptions.Multiline), BlockquoteStyle);
    }

    public static IRenderable CodeBlock(MarkdownBlock block)
    {
        var codePanel = new Panel(Align.Left(
                new SyntaxHighlighting(block)
            ));
        
        codePanel.Header = new PanelHeader(block.Language!);
        codePanel.Padding = new Padding(4, 0, 0, 0);
        codePanel.Border = BoxBorder.Rounded;
        
        return codePanel;
    }

    public static IRenderable HorizontalRule(MarkdownBlock block)
    {
        return new Rule();
    }

    public static IRenderable OrderedList(MarkdownBlock block)
    {
        return new Markup(block.Content, ParagraphStyle);
    }

    public static IRenderable UnorderedList(MarkdownBlock block)
    {
        return new Markup(block.Content, ParagraphStyle);
    }
    
    public static string CleanUp(string markdown)
    {
        // Replace "[" with "[[" and vise versa
        markdown = Regex.Replace(markdown, @"\[", "[[");
        markdown = Regex.Replace(markdown, @"\]", "]]");

        return markdown;
    }
}

public class SyntaxHighlighting(MarkdownBlock block) : JustInTimeRenderable
{
    // Highlight via SyntaxHighlightExtensions
    protected override IRenderable Build()
    {
        return block.Language switch
        {
            "csharp" => SyntaxHighlightingExtensions.CSharpHighlight(block),
            "python" => SyntaxHighlightingExtensions.PythonHighlight(block),
            "bash" => SyntaxHighlightingExtensions.BashHighlight(block),
            _ => SyntaxHighlightingExtensions.NotImplemented(block)
        };
    }
}

public static class SyntaxHighlightingExtensions
{
    public static IRenderable CSharpHighlight(MarkdownBlock block)
    {
        string code = CleanUp(block.Content);
        
        // declaration
        code = Regex.Replace(code, @"using", "[blue]$0[/]");
        code = Regex.Replace(code, @"namespace", "[blue]$0[/]");
        code = Regex.Replace(code, @"var", "[blue]$0[/]");
        code = Regex.Replace(code, @"string", "[blue]$0[/]");
        code = Regex.Replace(code, @"int", "[blue]$0[/]");
        code = Regex.Replace(code, @"double", "[blue]$0[/]");
        code = Regex.Replace(code, @"void", "[blue]$0[/]");
        code = Regex.Replace(code, @"class", "[blue]$0[/]");
        code = Regex.Replace(code, @"static", "[blue]$0[/]");
        code = Regex.Replace(code, @"public", "[blue]$0[/]");
        
        // Methods
        code = Regex.Replace(code, @"Console", "[purple_2]$0[/]");
        code = Regex.Replace(code, @"(\.)([A-Z][A-Za-z0-9_]*)(\(\))", "$1[green]$2[/]$3");

        return new Markup(code);
        
        return NotImplemented(block);
    }

    public static IRenderable PythonHighlight(MarkdownBlock block)
    {
        return NotImplemented(block);
    }

    public static IRenderable BashHighlight(MarkdownBlock block)
    {
        string code = CleanUp(block.Content);
        List<string> lines = code.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None).ToList();
        
        StringBuilder stringBuilder = new StringBuilder();
        int currentLine = 0;
        
        while (currentLine < lines.Count)
        {
            if (lines[currentLine] == String.Empty)
            {
                lines.RemoveAt(currentLine);
            }
            else
            {
                if(currentLine == 0 && lines[currentLine] != String.Empty) 
                    stringBuilder.Append("[grey27]>[/] " + lines[currentLine]);
                else 
                    stringBuilder.Append("\n[grey27]>[/] " + lines[currentLine]);
                
                currentLine++;
            }
        }
        
        return new Markup(stringBuilder.ToString());
    }

    public static IRenderable HtmlHighlight(MarkdownBlock block)
    {
        return NotImplemented(block);
    }

    public static IRenderable NotImplemented(MarkdownBlock block)
    {
        string code = CleanUp(block.Content);
        return new Markup($"[green]{code}[/]");
    }

    private static string CleanUp(string markdown)
    {
        markdown = Regex.Replace(markdown, @"```([\s\S]*?)```", "$1", RegexOptions.Singleline);
        // Replace "[" with "[[" and vise versa
        markdown = Regex.Replace(markdown, @"\[", "[[");
        markdown = Regex.Replace(markdown, @"\]", "]]");

        return markdown;
    }
}