using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Clippy
{
    public class ClipboardContentTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var clipboardContent = item as ClipboardContent;

            if (clipboardContent == null)
                return base.SelectTemplate(item, container);

            

            FrameworkElement element = container as FrameworkElement;

            if (clipboardContent.Data is ImageSource)
            {
                return element.FindResource("ImageContentDataTemplate") as DataTemplate;
            }

            switch (clipboardContent.Format)
            {
                case ClipboardFormat.FileDrop:
                    var files = clipboardContent.Data as string[];
                    if (files.Length == 1)
                    {
                        return element.FindResource("FileDropContentDataTemplate") as DataTemplate;
                    }
                    else
                    {
                        return element.FindResource("MultipleFileDropContentDataTemplate") as DataTemplate;
                    }
                    
                case ClipboardFormat.Text:
                case ClipboardFormat.CommaSeparatedValue:
                case ClipboardFormat.Html:
                case ClipboardFormat.OemText:
                case ClipboardFormat.StringFormat:
                case ClipboardFormat.UnicodeText:
                    return element.FindResource("StringContentDataTemplate") as DataTemplate;
                default:
                    return element.FindResource("ObjectContentDataTemplate") as DataTemplate;
                
            }
        }
    }
}