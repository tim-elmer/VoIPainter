using Markdig;
using Neo.Markdig.Xaml;
using System;
using System.ComponentModel;
using System.Windows;
using System.Linq;

namespace VoIPainter.View
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class MdWindow : Window, INotifyPropertyChanged
    {
        private string _document;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Document
        {
            get => _document;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(Document));

                _document = value;

                if (!IsLoaded)
                    Loaded += (s, e) => LoadDocument();
                else
                    LoadDocument();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Document)));
            }
        }

        public MdWindow(string document)
        {
            InitializeComponent();
            Document = document;
        }

        private void LoadDocument()
        {
            using var stream = new System.IO.StreamReader(GetType().Assembly.GetManifestResourceStream($"VoIPainter.{Document}"));
            FlowDocumentScrollViewer.Document = MarkdownXaml.ToFlowDocument(stream.ReadToEnd(), new MarkdownPipelineBuilder().UseXamlSupportedExtensions().Build());
        }

        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (e.Parameter.ToString().StartsWith("#", StringComparison.OrdinalIgnoreCase))
            {
                var search = e.Parameter.ToString().Remove(0, 1);

                var block = FlowDocumentScrollViewer.Document.Blocks.FirstBlock;
                do
                {
                    if (block.ContentStart.Paragraph != null)
                    {
                        var inline = block.ContentStart.Paragraph.Inlines.FirstInline;

                        if (new System.Windows.Documents.TextRange(inline.ContentStart, inline.ContentEnd).Text.Replace(' ', '-').Contains(search, StringComparison.OrdinalIgnoreCase))
                        {
                            inline.BringIntoView();
                            return;
                        }
                    }
                    block = block.NextBlock;
                }
                while (!(block is null));

            }
            UICommon.OpenLink(e.Parameter.ToString());
        }
    }
}
