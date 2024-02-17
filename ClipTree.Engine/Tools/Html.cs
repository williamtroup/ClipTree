using System;
using System.IO;
using ClipTree.Engine.Clipboard;
using ClipTree.Engine.Windows;

namespace ClipTree.Engine.Tools
{
    public static class Html
    {
        #region Private Constants

        private const string PreviewClipStorageFolder = "ClipPreviews";
        private const string PreviewLookTag1 = "<html>";
        private const string PreviewLookTag2 = "<head>";
        private const string PriviewTitle = "<html><title>\"{0}\" Clip</title>";

        #endregion

        public static bool View(ClipboardHistoryItem clipboardHistoryItem)
        {
            bool loaded = false;

            string html = StripHeader(clipboardHistoryItem);
            if (!string.IsNullOrEmpty(html))
            {
                SetupStorageFolder();

                string filename = GetNewFilename();

                File.WriteAllText(filename, html);

                loaded = Processes.Start(filename);
            }

            return loaded;
        }

        public static string StripHeader(ClipboardHistoryItem clipboardHistoryItem)
        {
            string returnHtml = null;
            string html = clipboardHistoryItem.Text;
            string htmlTitle = string.Format(PriviewTitle, clipboardHistoryItem.Name);

            int startHTMLIndex = html.IndexOf(PreviewLookTag1, StringComparison.CurrentCultureIgnoreCase);
            int startLength = PreviewLookTag1.Length;

            if (startHTMLIndex <= -1)
            {
                startHTMLIndex = html.IndexOf(PreviewLookTag2, StringComparison.CurrentCultureIgnoreCase);
                startLength = PreviewLookTag2.Length;
            }

            if (startHTMLIndex > -1)
            {
                int startIndex = startHTMLIndex + startLength;

                returnHtml = html.Substring(startIndex, html.Length - startIndex);
                returnHtml = string.Format("{0}{1}", htmlTitle, returnHtml);
            }

            return returnHtml;
        }

        #region Private Path Helpers

        private static string GetNewFilename()
        {
            DateTime dateTime = DateTime.Now;

            string filename = string.Format(
                "clip_{0}-{1}-{2}_{3}-{4}-{5}.html",
                dateTime.Day,
                dateTime.Month,
                dateTime.Year,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second);

            return Path.Combine(PreviewClipStorageFolder, filename);
        }

        private static void SetupStorageFolder()
        {
            if (Directory.Exists(PreviewClipStorageFolder))
            {
                string[] files = Directory.GetFiles(PreviewClipStorageFolder);

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);

                    if (fileInfo.LastWriteTime < DateTime.Now.AddDays(-1))
                    {
                        fileInfo.Delete();
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(PreviewClipStorageFolder);
            }
        }

        #endregion
    }
}