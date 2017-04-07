using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Clippy
{
    public class ClipboardContent
    {
        public ClipboardFormat Format { get; set; }
        public object Data { get; set; }
    }

    public class ClipboardContentListViewModel
    {
        public ObservableCollection<ClipboardContent> ClipboardContent { get; private set; }

        public ClippyCommand CopyCommand { get; private set; }
        public ClippyCommand DeleteCommand { get; private set; }

        public ClipboardContentListViewModel()
        {
            ClipboardContent = new ObservableCollection<ClipboardContent>();
            
                
            CopyCommand = new ClippyCommand(CopyCommandExecute);
            DeleteCommand = new ClippyCommand(DeleteCommandExecute);
        }

        private void DeleteCommandExecute(object obj)
        {
            ClipboardContent.Remove(obj as ClipboardContent);
        }

        private bool _disabled = false;

        private void CopyCommandExecute(object o)
        {
            _disabled = true;
            var clipboardContent = o as ClipboardContent;
            Clipboard.SetData(clipboardContent.Format.ToString(), clipboardContent.Data);

            Task.Delay(500).ContinueWith(x =>
            {
                _disabled = false;
            });
        }

        public void Add(ClipboardContent content)
        {
            if (!_disabled)
            {
                ClipboardContent.Insert(0,content);
                if (ClipboardContent.Count > 25)
                {
                    ClipboardContent.Remove(ClipboardContent.Last());
                }
            }
        }
    }
}