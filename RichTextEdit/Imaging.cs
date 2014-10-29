using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace System.Windows.Controls
{
    public class ImageSource
    {
        MemoryStream Source = new MemoryStream();
        Int32 Width;
        Int32 Height;

        public ImageSource(
            Stream InStream,
            Int32 InWidth,
            Int32 InHeight)
        {
            Source.SetLength(InStream.Length);
            InStream.Read(Source.GetBuffer(), 0, (Int32)InStream.Length);

            Width = InWidth;
            Height = InHeight;
            Source.Position = 0;
        }

        public Image CreateImage()
        {
            Image Result = new Image();

            Result.HorizontalAlignment = HorizontalAlignment.Left;
            Result.VerticalAlignment = VerticalAlignment.Top;
            Result.Width = Width;
            Result.Height = Height;
            Result.Stretch = Stretch.Fill;

            BitmapImage Src = new BitmapImage();

            Src.SetSource(Source);

            Result.Source = Src;

            return Result;
        }
    }
}
