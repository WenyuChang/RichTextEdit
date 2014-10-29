using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace System.Windows.Controls
{
    public class ResourceManager
    {
        public static String LoadText(String InResPath)
        {
            Stream Stream = typeof(ResourceManager).Assembly.GetManifestResourceStream(InResPath);
            Byte[] Buffer = new Byte[Stream.Length];

            Stream.Read(Buffer, 0, Buffer.Length);

            return Encoding.UTF8.GetString(Buffer, 0, Buffer.Length);
        }

        public static ImageSource LoadImage(
            String InResPath,
            Int32 InWidth,
            Int32 InHeight)
        {
            return new ImageSource(
                typeof(ResourceManager).Assembly.GetManifestResourceStream(
                    "RichTextEdit.Icons." + InResPath),
                InWidth,
                InHeight);
        }
    }
}
