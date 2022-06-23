using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace TalosWorkshop
{
    public static class FlowDocumentExtensions
    {
        public static IEnumerable<Paragraph> Paragraphs(this FlowDocument doc)
        {
            return doc.Descendants().OfType<Paragraph>();
        }
    }

    public static class DependencyObjectExtensions
    {
        public static IEnumerable<DependencyObject> Descendants(this DependencyObject root)
        {
            if (root == null)
                yield break;
            yield return root;
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
                foreach (var descendent in child.Descendants())
                    yield return descendent;
        }
    }

    public static class TextExtensions
    {
        public static string GetHTMText(RichTextBox rtb)
        {
            string result = "";

            for (int k = 0; k < rtb.Document.Blocks.Count; k++)
            {
                Block blk = rtb.Document.Blocks.ElementAt(k);
                //Paragraph block = rtb.Document.Blocks.ElementAt(k) as Paragraph;
                string paragraph = "";
                string p_prefix = "<p>";
                string p_postfix = "</p>";

                Paragraph block = null;

                if (blk is Paragraph) block = blk as Paragraph;
                else continue;

                for (int i = 0; i < block.Inlines.Count; i++)
                {
                    Inline line = block.Inlines.ElementAt(i);
                    string prefix = "";
                    string postfix = "";
                    //line.FontWeight == FontWeights.Bold
                    //line.FontStyle == FontStyles.Italic
                    if (line.TextDecorations is not null)
                    {
                        foreach (var effect in line.TextDecorations)
                        {
                            if (effect.Location == TextDecorationLocation.Underline)
                            {
                                prefix += "<u>";
                                postfix += "</u>";
                            }
                            if (effect.Location == TextDecorationLocation.Strikethrough)
                            {
                                prefix += "<s>";
                                postfix += "</s>";
                            }
                        }
                    }
                    if (line.FontWeight == FontWeights.Bold)
                    {
                        prefix += "<b>";
                        postfix += "</b>";
                    }
                    if (line.FontStyle == FontStyles.Italic || line.FontStyle == FontStyles.Oblique)
                    {
                        prefix += "<i>";
                        postfix += "</i>";
                    }
                    Color c = ((SolidColorBrush)line.Foreground).Color;
                    if (c == Colors.Red)
                    {
                        prefix += "<span class=\"red\">";
                        postfix += "</span>";
                    }
                    if (c == Colors.Green)
                    {
                        prefix += "<span class=\"green\">";
                        postfix += "</span>";
                    }
                    if (c == Colors.Orange)
                    {
                        prefix += "<span class=\"orange\">";
                        postfix += "</span>";
                    }
                    if (c == Colors.Pink)
                    {
                        prefix += "<span class=\"glitch-text\">";
                        postfix += "</span>";
                    }

                    string text = new TextRange(line.ContentStart, line.ContentEnd).Text;
                    if (!String.IsNullOrEmpty(text)) paragraph += $"{prefix}{text}{postfix}";

                }
                if (!String.IsNullOrEmpty(paragraph))
                    result += $"{p_prefix}{paragraph}{p_postfix}\n";
            }

            return result;
        }

        public static FlowDocument ParseHTMLText(string text)
        {
            try
            {
                FlowDocument result = new();

                string[] paragraphs = text.Replace("\r", "").Split("\n");

                foreach (var par in paragraphs)
                {
                    if (par.Contains("<u>"))
                    {

                    }
                }

                return result;
            }
            catch(Exception e)
            {

                return null;
            }


            
        }
    }
}
