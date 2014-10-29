/*
Released under MIT - License:
 
Copyright (c) 2008 by Christoph Husse

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Browser;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Windows.Markup;
using System.IO;
using System.ComponentModel; 

namespace System.Windows.Controls
{
    public enum TextOrientation
    {
        /// <summary>
        /// All lines will be vertically top aligned...
        /// </summary>
        VerticalTop = 0x01,
        /// <summary>
        /// All lines will be vertically centered...
        /// </summary>
        VerticalCenter = 0x02,
        /// <summary>
        /// All lines will be vertically bottom aligned...
        /// </summary>
        VerticalBottom = 0x03,

        VerticalMask = 0x0F,
    }

    public enum CursorPosition
    {
        Start,
        End
    }

    public enum ControlCode : byte
    {
        None = 0,
        Enter = 1,
        Indent = 2,
        Unindent = 3,
        Item = 4,
        Terminator = 5,
    }

    [Flags]
    public enum TextAttributes : byte
    {
        None = 0,
        Sup = 1,
        Sub = 2,
        StrikeThrough = 4,
        Underlined = 8,
        Mask = 0xF,
    }

    public enum TextAlign : byte
    {
        Left = 2,
        Center = 1,
        Block = 0,
    }

    public struct SizeRange
    {
        internal Double Min;
        internal Double Max;

        public SizeRange(
            Double InMin,
            Double InMax)
        {
            Min = InMin;
            Max = InMax;
        }
    }
    public struct ColorRange
    {
        internal Color Min;
        internal Color Max;

        public ColorRange(
            Color InMin,
            Color InMax)
        {
            Min = InMin;
            Max = InMax;
        }
    }

    public class RichTextRestrictionEntry
    {
        public enum Type
        {
            Including,
            Excluding,
        }

        private Type m_Type;
        private List<ColorRange> m_Colors = new List<ColorRange>();
        private List<SizeRange> m_Sizes = new List<SizeRange>();
        private List<FontStretch> m_Stretches = new List<FontStretch>();
        private List<FontWeight> m_Weights = new List<FontWeight>();
        private List<FontStyle> m_Styles = new List<FontStyle>();
        private List<FontFamily> m_Families = new List<FontFamily>();

        public RichTextRestrictionEntry(Type InType, params object[] InValues)
        {
            m_Type = InType;

            for (int i = 0; i < InValues.Length; i++)
            {
                object v = InValues[i];

                if ((v as IEnumerable<FontWeight>) != null) AddWeight(v as IEnumerable<FontWeight>);
                else if ((v as IEnumerable<FontStretch>) != null) AddStretch(v as IEnumerable<FontStretch>);
                else if ((v as IEnumerable<FontFamily>) != null) AddFamily(v as IEnumerable<FontFamily>);
                else if ((v as IEnumerable<SizeRange>) != null) AddSizeRange(v as IEnumerable<SizeRange>);
                else if ((v as IEnumerable<Double>) != null) AddSize(v as IEnumerable<Double>);
                else if ((v as IEnumerable<Color>) != null) AddColor(v as IEnumerable<Color>);
                else if ((v as IEnumerable<ColorRange>) != null) AddColorRange(v as IEnumerable<ColorRange>);
                else if ((v as IEnumerable<FontStyle>) != null) AddStyle(v as IEnumerable<FontStyle>);
                else
                    throw new InvalidCastException("At least one value is not an valid enumeration.");
            }
        }

        public void AddWeight(IEnumerable<FontWeight> InWeight)
        {
            m_Weights.AddRange(InWeight);
        }

        public void AddWeight(params FontWeight[] InWeight)
        { AddWeight((IEnumerable<FontWeight>)InWeight); }

        public void AddStyle(IEnumerable<FontStyle> InStyle)
        {
            m_Styles.AddRange(InStyle);
        }

        public void AddStyle(params FontStyle[] InStyle)
        { AddStyle((IEnumerable<FontStyle>)InStyle); }

        public void AddFamily(IEnumerable<FontFamily> InFamily)
        {
            m_Families.AddRange(InFamily);
        }

        public void AddFamily(params FontFamily[] InFamily)
        { AddFamily((IEnumerable<FontFamily>)InFamily); }

        public void AddSize(IEnumerable<Double> InSize)
        {
            List<Double> List = new List<double>(InSize);

            for (int i = 0; i < List.Count; i++)
            {
                AddSizeRange(new SizeRange(List[i], List[i]));
            }
        }

        public void AddSize(params Double[] InSize)
        { AddSize((IEnumerable<Double>)InSize); }

        public void AddSizeRange(IEnumerable<SizeRange> InRange)
        {
            m_Sizes.AddRange(InRange);
        }

        public void AddSizeRange(params SizeRange[] InRange)
        { AddSizeRange((IEnumerable<SizeRange>)InRange); }

        public void AddColor(IEnumerable<Color> InColor)
        {
            List<Color> List = new List<Color>(InColor);

            for (int i = 0; i < List.Count; i++)
            {
                AddColorRange(new ColorRange(List[i], List[i]));
            }
        }

        public void AddColor(params Color[] InColor)
        { AddColor((IEnumerable<Color>)InColor); }

        public void AddColorRange(IEnumerable<ColorRange> InColorRange)
        {
            m_Colors.AddRange(InColorRange);
        }

        public void AddColorRange(params ColorRange[] InColorRange)
        { AddColorRange((IEnumerable<ColorRange>)InColorRange); }

        public void AddStretch(IEnumerable<FontStretch> InStretch)
        {
            m_Stretches.AddRange(InStretch);
        }

        public void AddStretch(params FontStretch[] InStretch)
        { AddStretch((IEnumerable<FontStretch>)InStretch); }
    }

    public class RichTextRestriction
    {
        List<RichTextRestrictionEntry> m_Entries = new List<RichTextRestrictionEntry>();

        public RichTextRestriction(params IEnumerable<RichTextRestrictionEntry>[] InEntries)
        {
            for (int i = 0; i < InEntries.Length; i++)
            {
                m_Entries.AddRange(InEntries[i]);
            }
        }

        public RichTextRestriction(params RichTextRestrictionEntry[] InEntries)
        {
            m_Entries.AddRange(InEntries);
        }

        public void AddEntry(IEnumerable<RichTextRestrictionEntry> InEntry)
        {
            m_Entries.AddRange(InEntry);
        }
    }

    public partial class RichTextEdit : UserControl
    {
        private static MemoryStream m_Clipboard = null;
        private static String m_ClipboardText = null;
        private static Boolean m_ClipboardOperation = false;
        private Boolean IsUpdating = false;
        private Grid m_View;
        private Grid m_Surface;
        private Grid m_MouseCapture;
        private Grid m_Overlay;
        private Boolean IsMouseDown = false;
        private TextBox m_Input;
        private List<LineItem> LineItems = new List<LineItem>();
        private Int32 m_SelectionStart = 0;
        private Int32 m_SelectionEnd = 0;
        private Canvas m_Cursor;
        private DispatcherTimer m_CursorTimer = new DispatcherTimer();
        private Int32 IsCtrlDown = 0;
        private Int32 IsShiftDown = 0;
        private Int32 ShiftCursorPos = 0;
        private Boolean IsInvalidated = false;
        private Int64 m_LastClick = 0;
        private Color m_FontForeground = Colors.Black;
        private Color m_FontBackground = Colors.Transparent;
        private Double m_FontSize = 14;
        private FontStretch m_FontStretch = FontStretches.Normal;
        private FontWeight m_FontWeight = FontWeights.Normal;
        private FontStyle m_FontStyle = FontStyles.Normal;
        private FontFamily m_FontFamily = new FontFamily("Arial");
        private String m_DirectInput = "";
        private Int32 m_CursorPosition = 0;
        private List<Replacement> RepList = new List<Replacement>();
        private Int32 m_MaxMacroLen = 0;
        private TextAttributes m_TextAttributes = TextAttributes.None;
        private TextAlign m_TextAlignment = TextAlign.Block;
        private Int64 m_LastModification = DateTime.Now.Ticks;
        private Int32 m_CursorTick = 0;
        private ScrollViewer m_Scroll;
        private Double m_MaxTextWidth = Double.NaN;
        private DispatcherTimer m_UpdateDpc;
        private Double m_AutoTextWidth = 0;
        private Int32 m_HasClipboardAccess = -1;
        private Border m_Border;
        private Color m_SelectionBackground = Colors.Black;
        private Color m_SelectionForeground = Colors.White;
        private List<IRichTextObject> m_RTOList = new List<IRichTextObject>();
        private Boolean m_IsReadOnly = false;
        private Boolean m_IsItem = false;
        private Boolean m_IsIndent = false;
        private Int32 m_UndoIndex = 0;
        private Int64 m_LastUndoSave = 0;
        private Int32 m_MaxUndoCount = 50;
        private List<UndoEntry> m_UndoList = new List<UndoEntry>();

        class UndoEntry
        {
            public MemoryStream Stream;
            public Int32 SelStart;
        }

        void ThrowException(Exception e)
        {
            throw e;
        }

        void RichTextEdit_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize(e.NewSize.Width, e.NewSize.Height);
        }

        void UpdateSize()
        {
            UpdateSize(Width, Height);
        }

        void UpdateSize(
            Double InWidth,
            Double InHeight)
        {
            Double Radius = Math.Max(
                Math.Max(m_Border.CornerRadius.BottomLeft, m_Border.CornerRadius.BottomRight),
                Math.Max(m_Border.CornerRadius.TopLeft, m_Border.CornerRadius.TopRight));

            m_Scroll.Width = m_Border.ActualWidth - (m_Border.BorderThickness.Right + Radius + m_Border.BorderThickness.Right + Radius);
            m_Scroll.Height = m_Border.ActualHeight - (m_Border.BorderThickness.Top + Radius + m_Border.BorderThickness.Bottom + Radius);

                if ((m_Scroll.VerticalScrollBarVisibility == ScrollBarVisibility.Visible) ||
                    (
                        (m_Scroll.ComputedVerticalScrollBarVisibility == Visibility.Visible) &&
                        (m_Scroll.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled) &&
                        (m_Scroll.VerticalScrollBarVisibility != ScrollBarVisibility.Hidden)
                    )
                )
                m_AutoTextWidth = m_Scroll.Width - VerticalScrollBarWidth;
            else
                m_AutoTextWidth = m_Scroll.Width - 1;

            Invalidate();
        }

        public RichTextEdit()
        {
            m_UpdateDpc = new DispatcherTimer();
            m_UpdateDpc.Interval = new TimeSpan(10000 * 50);
            m_UpdateDpc.Tick += m_UpdateDpc_Tick;
            m_UpdateDpc.Start();

            LineItems.Add(new LineItem(this, ControlCode.Terminator));

            //m_Scroll = (System.Windows.Controls.ScrollViewer)XamlReader.Load(ResourceManager.LoadText("RichTextEdit.ScrollViewer.xaml"));
            m_Scroll = new ScrollViewer();
            m_Scroll.Margin = new Thickness(0, 0, 0, 0);
            m_Scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            m_Scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            m_Scroll.BorderThickness = new Thickness(0, 0, 0, 0);
            
            m_View = new Grid();
            m_View.Margin = new Thickness(0, 0, 0, 0);

            m_Surface = new Grid();
            m_Surface.Margin = new Thickness(0, 0, 0, 0);
            m_Surface.Background = new SolidColorBrush(Colors.White);

            m_MouseCapture = new Grid();
            m_MouseCapture.Margin = new Thickness(0, 0, 0, 0);
            m_MouseCapture.MouseLeftButtonDown += new MouseButtonEventHandler(m_Input_MouseLeftButtonDown);
            m_MouseCapture.MouseMove += new MouseEventHandler(m_Input_MouseMove);
            m_MouseCapture.MouseLeftButtonUp += new MouseButtonEventHandler(m_Input_MouseLeftButtonUp);
            m_MouseCapture.Background = new SolidColorBrush(Colors.Red);
            m_MouseCapture.Opacity = 0;
            m_MouseCapture.GotFocus += new RoutedEventHandler(m_Overlay_GotFocus);

            m_Overlay = new Grid();
            m_Overlay.Margin = new Thickness(0, 0, 0, 0);

            m_Input = new TextBox();
            m_Input.AcceptsReturn = true;
            m_Input.Opacity = 0.1;
            m_Input.Margin = new Thickness(0, 0, 0, 0);
            m_Input.KeyDown += new KeyEventHandler(RichTextEdit_KeyDown);
            m_Input.TextChanged += new TextChangedEventHandler(m_Input_TextChanged);
            m_Input.KeyUp += new KeyEventHandler(m_Input_KeyUp);
            m_Input.MouseLeftButtonDown += new MouseButtonEventHandler(m_Input_MouseLeftButtonDown);
            m_Input.MouseMove += new MouseEventHandler(m_Input_MouseMove);
            m_Input.MouseLeftButtonUp += new MouseButtonEventHandler(m_Input_MouseLeftButtonUp);
            m_Input.BorderThickness = new Thickness(0, 0, 0, 0);

            m_Cursor = new Canvas();
            m_Cursor.VerticalAlignment = VerticalAlignment.Top;
            m_Cursor.HorizontalAlignment = HorizontalAlignment.Left;
            m_Cursor.Width = 1;
            m_Cursor.MouseLeftButtonDown += new MouseButtonEventHandler(m_Input_MouseLeftButtonDown);
            m_Cursor.MouseMove += new MouseEventHandler(m_Input_MouseMove);
            m_Cursor.MouseLeftButtonUp += new MouseButtonEventHandler(m_Input_MouseLeftButtonUp);

            CursorColor = Colors.Black;

            m_CursorTimer.Interval = new TimeSpan(10000 * 500);
            m_CursorTimer.Tick += new EventHandler(m_CursorTimer_Tick);
            m_CursorTimer.Start();

            m_View.Children.Add(m_Input);
            m_View.Children.Add(m_Surface);
            m_View.Children.Add(m_MouseCapture);
            m_View.Children.Add(m_Overlay);
            m_View.Children.Add(m_Cursor);

            m_Input.LostFocus += new RoutedEventHandler(RichTextEdit_LostFocus);
            m_Input.GotFocus += new RoutedEventHandler(RichTextEdit_GotFocus);

            SizeChanged += new SizeChangedEventHandler(RichTextEdit_SizeChanged);

            m_Border = new Border();
            m_Border.Margin = new Thickness(0, 0, 0, 0);
            m_Border.BorderThickness = new Thickness(1, 1, 1, 1);
            m_Border.CornerRadius = new CornerRadius(3);
            m_Border.BorderBrush = new SolidColorBrush(Colors.Black);

            m_Scroll.HorizontalAlignment = m_Border.HorizontalAlignment = m_Surface.HorizontalAlignment = m_MouseCapture.HorizontalAlignment =
                m_View.HorizontalAlignment = m_Input.HorizontalAlignment = HorizontalAlignment.Stretch;
            m_Scroll.VerticalAlignment = m_Border.VerticalAlignment = m_Surface.VerticalAlignment = m_MouseCapture.VerticalAlignment =
                m_View.VerticalAlignment = m_Input.VerticalAlignment = VerticalAlignment.Stretch;

            m_Scroll.Content = m_View;
            m_Border.Child = m_Scroll;
            Content = m_Border;

            VerticalScrollBarWidth = HorizontalScrollBarHeight = 25;
        }

        void m_Overlay_GotFocus(object sender, RoutedEventArgs e)
        {
            m_Input.Focus();
        }

        void RichTextEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                m_CursorTick = 0;

                m_CursorTimer_Tick(null, null);

                m_CursorTimer.Stop();
            }
            catch (Exception ex)
            {
                ThrowException(ex);
            }
        }

        void RichTextEdit_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                m_CursorTick = 1;

                m_CursorTimer_Tick(null, null);

                m_CursorTimer.Start();
            }
            catch (Exception ex)
            {
                ThrowException(ex);
            }
        }

        void m_Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_IsReadOnly)
            {
                m_Input.Text = "";

                return;
            }

            try
            {
                if ((m_Input.Text != null) && (m_Input.Text.Length > 0))
                {
                    if (!m_ClipboardOperation)
                        InsertString(m_Input.Text);

                    m_ClipboardOperation = false;

                    m_Input.Text = "";
                }
            }
            catch (Exception ex)
            {
                ThrowException(ex);
            }
        }

        void m_CursorTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if ((m_CursorTick % 2) == 0)
                {
                    m_Cursor.Background = new SolidColorBrush(CursorColor);
                }
                else
                {
                    m_Cursor.Background = new SolidColorBrush(Colors.Transparent);

                    // check if something has changed
                    if (m_LastModification - m_LastUndoSave > 0)
                    {
                        if ((m_UndoIndex < m_UndoList.Count - 1) && (m_UndoList.Count > 0))
                        {
                            m_UndoList = new List<UndoEntry>(new UndoEntry[] {m_UndoList[
                                Math.Max(Math.Min(m_UndoList.Count - 1, m_UndoIndex), 0)] });
                        }

                        // add new state
                        MemoryStream Stream = new MemoryStream();

                        QueryText().Serialize(true, Stream);

                        m_UndoList.Add(new UndoEntry { Stream = Stream, SelStart = SelectionStart });

                        if (m_UndoList.Count > MaxUndoCount)
                            m_UndoList.RemoveRange(0, m_UndoList.Count - MaxUndoCount);

                        m_UndoIndex = m_UndoList.Count;
                        m_LastUndoSave = DateTime.Now.Ticks;
                    }
                }

                m_CursorTick++;
            }
            catch (Exception ex)
            {
                ThrowException(ex);
            }
        }

        Int32 PositionToIndex(Point InPos)
        {
            // search for line...
            Boolean IsBehind = false;
            Int32 Result = 0;
            Double LastBottom = 0;

            try
            {
                for (int i = 0; i < LineItems.Count; i++)
                {
                    LineItem Item = LineItems[i];
                    Int32 LineIndex = Item.LineIndex;

                    if (InPos.Y > Item.Top)
                        IsBehind = true;

                    if ((InPos.Y >= LastBottom) && (InPos.Y <= LastBottom + Item.LineHeight ))
                    {
                        // search for column...
                        for (; i < LineItems.Count; i++)
                        {
                            Item = LineItems[i];

                            if (Item.LineIndex != LineIndex)
                            {
                                i--;

                                break;
                            }

                            if (InPos.X <= Item.Left + Item.FastActualWidth / 2)
                                return (Result = i);
                        }

                        // set index to last element of line...
                        return (Result = i);
                    }

                    if((i < LineItems.Count - 1) && (LineItems[i + 1].LineIndex != LineIndex))
                        LastBottom = Item.Top + Item.LineHeight;
                }

                if (IsBehind)
                    return (Result = LineItems.Count);
                else
                    return (Result = -1);
            }
            finally
            {
            }
        }

        void m_Input_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_Input.Focus();

            if (m_IsReadOnly) return;

            IsMouseDown = false;
        }

        void m_Input_MouseMove(object sender, MouseEventArgs e)
        {
            m_Input.Focus();

            if (m_IsReadOnly) return;

            try
            {
                if (AutoFocus && !m_CursorTimer.IsEnabled)
                {
                    m_Input.Focus();
                }

                if (IsMouseDown)
                {
                    Point Pos = e.GetPosition(m_Input);
                    Int32 End = PositionToIndex(Pos);

                    if (End != m_SelectionEnd)
                    {
                        m_CursorPosition = m_SelectionEnd = PositionToIndex(Pos);

                        UpdateSelection();
                    }
                }
            }
            catch (Exception ex)
            {
                ThrowException(ex);
            }
        }

        void m_Input_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_Input.Focus();

            if (m_IsReadOnly) return;

            try
            {
                Point Pos = e.GetPosition(m_Input);

                MoveCursor(PositionToIndex(Pos));

                if ((DateTime.Now.Ticks - m_LastClick) < 10000 * 250)
                {
                    // select pointed word...
                    Letter[] Word = EnumFrameLetters(false, true, 0, SelectionStart, LineItems.Count - 1);

                    if (Word.Length > 0)
                        Select(CursorPosition.End, Word[0].Index, Word.Length);
                }

                IsMouseDown = true;
                m_LastClick = DateTime.Now.Ticks;
            }
            catch (Exception ex)
            {
                ThrowException(ex);
            }

            m_Input.Focus();
        }

        void m_Input_KeyUp(object sender, KeyEventArgs e)
        {
            if (m_IsReadOnly) return;

            try
            {
                switch (e.Key)
                {
                    case Key.Ctrl: IsCtrlDown = 0; break;
                    case Key.Shift: IsShiftDown = 0; break;
                }

                if ((IsCtrlDown == 0) && (m_DirectInput.Length > 0))
                {
                    String c = new String((Char)Int16.Parse(m_DirectInput), 1);

                    InsertString(c);

                    m_DirectInput = "";
                }
            }
            catch (Exception ex)
            {
                ThrowException(ex);
            }
        }

        /// <summary>
        /// Browser that don't support clipboard operations (currently only IE does),
        /// will behave in the wrong way, because only the Ctrl+V will work. This is due
        /// to native support for TextBox! But Ctrl+C won't work and so we have to
        /// disable this feature completely if we can't copy to clipboard or the user
        /// has denied access. So only Copy/Paste within RichTextEdits of the current
        /// SilverlightApplication will work!
        /// </summary>
        void NegotiateClipboardAccess()
        {
            if (m_HasClipboardAccess >= 0)
                return;

            try
            {
                // read current content
                if (!(Boolean)HtmlPage.Window.Eval("window.clipboardData.getData != null"))
                    return;

                String CurrentClipboard = (String)HtmlPage.Window.Eval("window.clipboardData.getData('Text')");

                // write current content
                if (!(Boolean)HtmlPage.Window.Eval("window.clipboardData.setData != null"))
                    return;

                if (!(Boolean)HtmlPage.Window.Eval("window.clipboardData.setData('Text','" + CurrentClipboard.Replace("'", "\\'") + "')"))
                    return;
            }
            catch
            {
                m_HasClipboardAccess = 0;

                return;
            }

            m_HasClipboardAccess = 1;
        }

        void RichTextEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_IsReadOnly) return;

            try
            {
                e.Handled = true;

                switch (e.Key)
                {
                    case Key.Enter:
                        {
                            if (IsCtrlDown > 0)
                            {
                                InsertCtrlCode(ControlCode.Enter);
                            }
                            else
                            {
                                if (m_IsItem)
                                {
                                    InsertCtrlCode(ControlCode.Unindent);
                                    InsertCtrlCode(ControlCode.Item);
                                }
                                else if (m_IsIndent)
                                    InsertCtrlCode(ControlCode.Unindent);
                                else
                                    InsertCtrlCode(ControlCode.Enter);
                            }

                        } break;

                    #region Cursor movement support
                    case Key.Up:
                        {
                            MoveCursor(PositionToIndex(new Point(m_Cursor.Margin.Left, m_Cursor.Margin.Top - m_Cursor.Height / 2)));
                        } break;
                    case Key.Left:
                        {
                            if ((IsCtrlDown > 0) && (IsShiftDown > 0))
                            {
                                while ((m_CursorPosition > 0) &&
                                    LineItems[m_CursorPosition - 1].IsSpace &&
                                    MoveCursor(m_CursorPosition - 1)) ;

                                do
                                {
                                    if (!MoveCursor(m_CursorPosition - 1))
                                        break;

                                } while (!LineItems[m_CursorPosition].IsSpace);

                                if (m_CursorPosition > 0)
                                    MoveCursor(m_CursorPosition + 1);
                            }
                            else
                            {
                                MoveCursor(m_CursorPosition - 1);
                            }
                        } break;
                    case Key.Down:
                        {
                            MoveCursor(PositionToIndex(new Point(m_Cursor.Margin.Left, m_Cursor.Margin.Top + m_Cursor.Height * 1.5)));
                        } break;
                    case Key.Right:
                        {
                            if ((IsCtrlDown > 0) && (IsShiftDown > 0))
                            {
                                while ((m_CursorPosition < LineItems.Count - 1) &&
                                    LineItems[m_CursorPosition + 1].IsSpace &&
                                    MoveCursor(m_CursorPosition + 1)) ;

                                do
                                {
                                    if (!MoveCursor(m_CursorPosition + 1))
                                        break;

                                } while (!LineItems[m_CursorPosition].IsSpace);
                            }
                            else
                            {
                                MoveCursor(m_CursorPosition + 1);
                            }
                        } break;
                    case Key.End:
                        {
                            if (IsCtrlDown == 0)
                                MoveCursor(PositionToIndex(new Point(Double.MaxValue, m_Cursor.Margin.Top + m_Cursor.Height / 2)));
                            else
                                MoveCursor(PositionToIndex(new Point(Double.MaxValue, Double.MaxValue)));

                        } break;
                    case Key.Home:
                        {
                            if (IsCtrlDown == 0)
                                MoveCursor(PositionToIndex(new Point(0, m_Cursor.Margin.Top + m_Cursor.Height / 2)));
                            else
                                MoveCursor(PositionToIndex(new Point(0, 0)));
                        } break;
                    case Key.PageUp:
                        {
                            MoveCursor(PositionToIndex(new Point(m_Cursor.Margin.Left, m_Cursor.Margin.Top - m_Scroll.ViewportHeight)));
                        } break;
                    case Key.PageDown:
                        {
                            MoveCursor(PositionToIndex(new Point(m_Cursor.Margin.Left, m_Cursor.Margin.Top + m_Scroll.ViewportHeight)));
                        } break;

                    #endregion

                    #region Deletion support
                    case Key.Back:
                        {
                            Int32 SelStart = SelectionStart;

                            if (SelectionLength > 0)
                            {
                                RemoveRange(SelStart, SelectionLength);

                                Select(CursorPosition.Start, SelStart, 0);
                            }
                            else
                            {
                                RemoveRange(SelStart - 1, 1);

                                Select(CursorPosition.Start, SelStart - 1, 0);
                            }
                        } break;
                    case Key.Delete:
                        {
                            if (SelectionLength > 0)
                            {
                                RemoveRange(SelectionStart, SelectionLength);

                                Select(CursorPosition.Start, SelectionStart, 0);
                            }
                            else
                            {
                                RemoveRange(SelectionStart, 1);

                                Select(CursorPosition.Start, SelectionStart, 0);
                            }
                        } break;
                    #endregion

                    default:
                        e.Handled = false; break;
                }

                #region Control and Shift support
                switch (e.Key)
                {
                    case Key.Ctrl:
                        {
                            IsCtrlDown++;
                        } break;

                    case Key.Shift:
                        {
                            if (IsShiftDown == 0)
                                ShiftCursorPos = SelectionStart;

                            IsShiftDown++;
                        } break;
                }
                #endregion

                #region Control down
                if (IsCtrlDown > 0)
                {
                    e.Handled = true;

                    if (((Int32)e.Key >= (Int32)Key.NumPad0) && ((Int32)e.Key <= (Int32)Key.NumPad9))
                    {
                        m_DirectInput += new String((char)('0' + (Char)((Int32)e.Key - (Int32)Key.NumPad0)), 1);
                    }
                    else
                    {
                        switch (e.Key)
                        {
                            #region Clipboard support
                            case Key.C:
                                {
                                    ClipboardCopy();
                                } break;

                            case Key.V:
                                {
                                    e.Handled = false;

                                    ClipboardPaste();
                                } break;
                            #endregion

                            case Key.A:
                                {
                                    Select(CursorPosition.End, 0, LineItems.Count);
                                } break;

                            case Key.Y: // move forwards in undo list
                                {
                                    Int32 NewUndoIndex = Math.Min(m_UndoList.Count - 1, m_UndoIndex + 1);
                                    
                                    if (NewUndoIndex != m_UndoIndex)
                                        RestoreState(NewUndoIndex);

                                    // prevent text update by TextBox
                                    m_ClipboardOperation = true;
                                } break;

                            case Key.Z: // move backwards in undo list
                                {
                                    Int32 NewUndoIndex = Math.Max(0, m_UndoIndex - 1);

                                    if (NewUndoIndex != m_UndoIndex)
                                        RestoreState(NewUndoIndex);

                                    // prevent text update by TextBox
                                    m_ClipboardOperation = true;
                                } break;
                            default:
                                {
                                    e.Handled = false;
                                } break;
                        }
                    }
                }
                #endregion
                else
                {
                    if (!(((Int32)e.Key >= (Int32)Key.NumPad0) && ((Int32)e.Key <= (Int32)Key.NumPad9)))
                        m_DirectInput = "";
                }
            }
            catch (Exception ex)
            {
                ThrowException(ex);
            }
        }


        private void RestoreState(Int32 InNewUndoIndex)
        {
            if ((InNewUndoIndex < 0) || (InNewUndoIndex >= m_UndoList.Count))
                return;

            InternalSelect(CursorPosition.Start, 0, LineItems.Count);
            RemoveSelection();

            m_UndoList[InNewUndoIndex].Stream.Position = 0;

            InternalInsertDeserialization(true, m_UndoList[InNewUndoIndex].Stream);

            m_UndoIndex = InNewUndoIndex;
            m_LastUndoSave = DateTime.Now.Ticks;

            Update();

            Select(CursorPosition.Start, m_UndoList[InNewUndoIndex].SelStart, 0);
        }

        public void ClipboardCopy()
        {
            Snapshot Snapshot = QuerySelectionText();

            NegotiateClipboardAccess();

            try
            {
                if (HasClipboardAccess && (Boolean)HtmlPage.Window.Eval("window.clipboardData.setData != null"))
                {
                    if ((Boolean)HtmlPage.Window.Eval("window.clipboardData.setData('Text','" + Snapshot.Text.Replace("'", "\\'") + "')"))
                    {
                        m_ClipboardText = Snapshot.Text;
                    }
                    else
                        m_ClipboardText = null;
                }
                else
                    m_ClipboardText = null;

            }
            catch
            {
                m_ClipboardText = null;
            }

            // save formatting to static clipboard...
            m_Clipboard = new MemoryStream();

            try
            {
                Snapshot.Serialize(true, m_Clipboard);
            }
            catch
            {
                m_Clipboard = null;
                m_ClipboardText = null;
            }
        }

        public void ClipboardPaste()
        {
            NegotiateClipboardAccess();

            if (m_Clipboard != null)
            {
                // is formatting available for clipboard data?
                String Content = "";

                try
                {
                    if (HasClipboardAccess && (Boolean)HtmlPage.Window.Eval("window.clipboardData.getData != null"))
                        Content = (String)HtmlPage.Window.Eval("window.clipboardData.getData('Text')");
                }
                catch
                {
                }

                if (Content == null)
                    Content = "";

                if ((m_ClipboardText == null) || (Content.CompareTo(m_ClipboardText) == 0))
                {
                    // just insert formatted clipboard stream
                    m_Clipboard.Position = 0;

                    try
                    {
                        InsertDeserialization(true, m_Clipboard);

                        m_ClipboardOperation = true;
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                // no formatting to insert - this is if text has beend copied from another component...
            }
        }

        /// <summary>
        /// Asynchronously requests an update of the rendered content.
        /// This can't be done synchronously because the editor may change hundreds
        /// of items and so will invoke Invalidate() hundreds times or even more in
        /// common cases. So we just collect various update requests and process them
        /// within one call. The user won't notice a delay of 50 ms...
        /// </summary>
        void Invalidate()
        {
            IsInvalidated = true;
        }

        void m_UpdateDpc_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!IsInvalidated)
                    return;

                IsInvalidated = false;

                m_UpdateDpc.Stop();

                try
                {
                    Update();
                }
                finally
                {
                    m_UpdateDpc.Start();
                }
            }
            catch (Exception ex)
            {
                ThrowException(ex);
            }
        }

        /// <summary>
        /// This is the main method doing all the alignment necessary for rendering.
        /// All letters and items will be arranged within the control depending on
        /// the overall state. As Microsoft's DependencyProperty seems to be quiet slow,
        /// it uses some weird performance tweaks. But this will speed up the whole thing
        /// about 200000 percents, so it's not worthless at all!
        /// </summary>
        void Update()
        {
            IsInvalidated = false; 

            if (IsUpdating)
                return;

            try
            {
                IsUpdating = true;

                for (int i = 0; i < LineItems.Count; i++)
                {
                    LineItem Item = LineItems[i];

                    Item.Index = i;
                    Item.ReadState();
                    Item.FastIsRenderable = true;
                    Item.FastHeight = Double.NaN;
                    Item.FastWidth = Double.NaN;
                }

                if (m_Scroll.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                    m_AutoTextWidth = m_Scroll.Width - VerticalScrollBarWidth;
                else
                    m_AutoTextWidth = m_Scroll.Width - 1;
                
                // realign components...
                Int32 Index = 0;
                Int32 CtrlIndent = 0;
                Double CurrentTop = 0;
                Int32 IsItem = 0;
                Int32 LineIndex = 0;
                Double TextWidth = HasMaxTextWidth ? m_MaxTextWidth : m_AutoTextWidth;
                Int32 OldIndex = 0;

                do
                {
                    Double CurrentLeft = 0;
                    Double LineHeight = 0;
                    Int32 LineStart = Index;
                    Int32 SpaceCount = 0;
                    Int32 LastSpace = 0;
                    Double LastSpaceLeft = 0;
                    Boolean LineBreak = false;
                    Int32 CurrentCtrlIndent = CtrlIndent;
                    TextAlign Align = TextAlign.Left;

                    if (IsItem > 0)
                        IsItem--;

                    for (; Index < LineItems.Count; Index++)
                    {
                        LineItem Ctrl = LineItems[Index];

                        Ctrl.LineIndex = LineIndex;
                        Ctrl.FastLeft = CurrentLeft + CurrentCtrlIndent;
                        Ctrl.FastTop = CurrentTop;


                        switch (Ctrl.Code)
                        {
                            case ControlCode.Enter:
                                {
                                    if (LineHeight == 0)
                                    {
                                        if (Index > 0)
                                            LineHeight = LineItems[Index - 1].LineHeight;
                                        else
                                            LineHeight = 20;
                                    }

                                    LineBreak = true;
                                } break;
                            case ControlCode.Indent:
                                {
                                    LineBreak = true;
                                    CtrlIndent += 30;
                                } break;
                            case ControlCode.Unindent:
                                {
                                    LineBreak = true;

                                    if (CtrlIndent > 0)
                                        CtrlIndent -= 30;
                                } break;
                        }

                        Ctrl.LineHeight = LineHeight = Math.Max(LineHeight, Ctrl.FastActualHeight);

                        if (LineBreak)
                        {
                            Index++;

                            break;
                        }

                        if (Ctrl.Code == ControlCode.Item)
                        {
                            CtrlIndent += 30;
                            IsItem += 2;
                        }

                        if (CurrentLeft + Ctrl.FastActualWidth > TextWidth - CurrentCtrlIndent)
                        {
                            // break after last space...
                            if (LastSpace > LineStart + 3)
                            {
                                Index = LastSpace;
                                CurrentLeft = LastSpaceLeft;
                            }
                            else
                            {
                                Index--;
                            }

                            break;
                        }

                        if (Ctrl.IsSpace)
                        {
                            LastSpace = Index;
                            LastSpaceLeft = CurrentLeft;

                            SpaceCount++;
                        }

                        if (Ctrl is Letter)
                        {
                            Align = Ctrl.Alignment;
                        }

                        CurrentLeft += Ctrl.FastActualWidth - 0.3;
                    }
                    

                    Double Indent = 0;

                    if (Index < LineItems.Count)
                    {
                        // skip spaces at line start
                        for (int i = LineStart; i < Index; i++)
                        {
                            LineItem Ctrl = LineItems[i];

                            if (!Ctrl.IsSpace)
                            {
                                if (!Ctrl.IsCtrlCode || (Ctrl.Code == ControlCode.Item))
                                {
                                    Indent += -LineItems[i].FastLeft + CurrentCtrlIndent;

                                    break;
                                }
                            }
                            else
                            {
                                SpaceCount--;

                                Ctrl.FastIsRenderable = false;
                            }

                            LineStart++;
                        }

                        for (int i = LineStart; i < Index; i++)
                        {
                            LineItem Ctrl = LineItems[i];

                            Ctrl.FastLeft += Indent;
                        }
                    }

                    // determine horizontal alignment correction...
                    Double LeftIndent = 0;

                    switch (Align)
                    {
                        case TextAlign.Block:
                        case TextAlign.Left: break;
                        case TextAlign.Center: LeftIndent = (TextWidth - CurrentLeft) / 2; break;
                    }

                    // realign according to LineAlign and save line height for each component
                    for (int i = LineStart; i < Index; i++)
                    {
                        LineItem Ctrl = LineItems[i];

                        Ctrl.FastHeight = LineHeight;
                        Ctrl.LineHeight = LineHeight;
                        Ctrl.FastLeft += LeftIndent;

                        if (Ctrl.Item == null)
                            continue;

                        Letter Entry = Ctrl as Letter;

                        if(Entry == null)
                            Ctrl.FastItemTop = (LineHeight - Ctrl.FastActualHeight) / 2;
                        else if(Entry.IsSup)
                            Ctrl.FastItemTop = 0;
                        else if(Entry.IsSub)
                            Ctrl.FastItemTop = LineHeight - Ctrl.FastActualHeight;
                        else
                            Ctrl.FastItemTop = (LineHeight - Ctrl.FastActualHeight) / 2;
                    }
                    
                    if (TextAlign.Block == Align)
                    {
                        // check block condition...
                        if ((Index < LineItems.Count) && !LineBreak)
                        {
                            Double SpaceIndent = 0;

                            // ignore spaces at line end...
                            for (int i = Index; i > 0; i--)
                            {
                                LineItem Ctrl = LineItems[i];

                                if (!Ctrl.IsSpace)
                                    break;

                                Ctrl.FastIsRenderable = false;

                                SpaceCount--;
                            }

                            if (SpaceCount >= 1)
                                SpaceIndent = ((TextWidth - (CurrentLeft + Indent)) - CurrentCtrlIndent) / SpaceCount;

                            Indent = 0;

                            for (int i = LineStart; i < Index; i++)
                            {
                                LineItem Ctrl = LineItems[i];

                                Ctrl.FastLeft += Indent;

                                if (Ctrl.IsSpace)
                                {
                                    Indent += SpaceIndent;
                                    Ctrl.FastWidth = Ctrl.FastActualWidth + SpaceIndent;
                                }
                            }
                        }
                    }

                    if (OldIndex == Index)
                    {
                        // content won't fit...
                        break;
                    }


                    CurrentTop += LineHeight;

                    OldIndex = Index;
                    LineIndex++;
                }
                while (Index < LineItems.Count);


                // write changes...
                for (int i = 0; i < LineItems.Count; i++)
                {
                    LineItem Item = LineItems[i];

                    Item.WriteState();
                }

                InternalUpdateSelection();
            }
            finally
            {
                IsUpdating = false;
            }
        }

        void UpdateSelection()
        {
            InternalUpdateSelection();

            // update font state
            Boolean HasFont = false;
            FontFamily Family = new FontFamily("Arial");
            FontStyle Style = FontStyles.Normal;
            FontWeight Weight = FontWeights.Normal;
            Double Size = 14;
            Color Foreground = Colors.Black;
            Color Background = Colors.Transparent;
            TextAlign Alignment = TextAlign.Block;
            TextAttributes Attributes = TextAttributes.None;
            FontStretch Stretch = FontStretches.Normal;

            // determine letter before selection...
            for (int i = SelectionStart - 1; i >= 0; i--)
            {
                Letter Item = LineItems[i] as Letter;

                if (Item == null)
                    continue;

                Family = Item.Label.FontFamily;
                Style = Item.Label.FontStyle;
                Weight = Item.Label.FontWeight;
                Size = Item.FontSize;
                Foreground = Item.Foreground;
                Background = Item.Background;
                Attributes = Item.TextAttributes;
                Alignment = Item.Alignment;
                Stretch = Item.Label.FontStretch;

                break;
            }

            m_FontFamily = Family;
            m_FontStyle = Style;
            m_FontWeight = Weight;
            m_FontSize = Size;
            m_FontStretch = Stretch;
            m_FontForeground = Foreground;
            m_FontBackground = Background;
            m_TextAlignment = Alignment;
            m_TextAttributes = Attributes;

            for (int i = SelectionStart; i < SelectionStart + SelectionLength; i++)
            {
                Letter Item = LineItems[i] as Letter;

                if (Item == null)
                    continue;

                if (!HasFont)
                {
                    Family = Item.Label.FontFamily;
                    Style = Item.Label.FontStyle;
                    Weight = Item.Label.FontWeight;
                    Size = Item.FontSize;
                    Foreground = Item.Foreground;
                    Background = Item.Background;
                    Attributes = Item.TextAttributes;
                    Alignment = Item.Alignment;
                    Stretch = Item.Label.FontStretch;

                    HasFont = true;
                }
                else
                {
                    if (!Family.Equals(Item.Label.FontFamily) ||
                        !Style.Equals(Item.Label.FontStyle) ||
                        !Weight.Equals(Item.Label.FontWeight) ||
                        (Size != Item.FontSize) ||
                        !Foreground.Equals(Item.Foreground) ||
                        !Background.Equals(Item.Background) ||
                        (Alignment != Item.Alignment) ||
                        (Attributes != Item.TextAttributes) ||
                        (Stretch != Item.Label.FontStretch)
                        )
                    {
                        // only homogenous formatting may be saved...
                        return;
                    }
                }
            }

            m_FontFamily = Family;
            m_FontStyle = Style;
            m_FontWeight = Weight;
            m_FontSize = Size;
            m_FontForeground = Foreground;
            m_FontBackground = Background;
            m_TextAlignment = Alignment;
            m_TextAttributes = Attributes;
            m_FontStretch = Stretch;
        }

        /// <summary>
        /// Just updates selection related stuff. As selection is a very frequent
        /// operation, it leads to a bottleneck if we would update the rendering content
        /// on each selection change...
        /// </summary>
        void InternalUpdateSelection()
        {
            // update cursor
            if (m_CursorPosition < 0)
                m_CursorPosition = 0;

            if (m_CursorPosition >= LineItems.Count)
                m_CursorPosition = LineItems.Count - 1;

            LineItem CurPos = LineItems[m_CursorPosition];

            if (CurPos.Code == ControlCode.Terminator)
            {
                if (LineItems.Count > 1)
                {
                    CurPos = LineItems[LineItems.Count - 2];

                    if (CurPos.Code == ControlCode.Enter)
                        m_Cursor.Margin = new Thickness(0, CurPos.Top + CurPos.LineHeight, 0, 0);
                    else
                        m_Cursor.Margin = new Thickness(CurPos.Left + CurPos.ActualWidth, CurPos.Top, 0, 0);
                     
                    m_Cursor.Height = CurPos.LineHeight;
                }
                else
                {
                    m_Cursor.Margin = new Thickness(0, 0, 0, 0);
                    m_Cursor.Height = 20;
                }
            }
            else
            {
                m_Cursor.Margin = new Thickness(CurPos.Left, CurPos.Top, 0, 0);
                m_Cursor.Height = CurPos.LineHeight;
            }

            // is cursor visible?
            Point CursorTopLeft = new Point(m_Cursor.Margin.Left, m_Cursor.Margin.Top);
            Point CursorBottomRight = new Point(m_Cursor.Margin.Left + m_Cursor.Width, m_Cursor.Margin.Top + m_Cursor.Height);
            Rect Viewport = new Rect(
                m_Scroll.HorizontalOffset,
                m_Scroll.VerticalOffset,
                m_Scroll.ViewportWidth,
                m_Scroll.ViewportHeight);

            if (!Viewport.Contains(CursorTopLeft) || !Viewport.Contains(CursorBottomRight))
            {
                // adjust scroll viewer
                m_Scroll.ScrollToVerticalOffset(CursorTopLeft.Y - Viewport.Height / 2);
                m_Scroll.ScrollToHorizontalOffset(CursorTopLeft.X - Viewport.Width / 2);
            }

            // update selection
            Stack<ControlCode> CtrlStack = new Stack<ControlCode>();

            for (int i = 0; i < LineItems.Count; i++)
            {
                LineItem Item = LineItems[i];

                if ((i >= SelectionStart) && (i < SelectionStart + SelectionLength))
                    Item.IsSelected = true;
                else
                    Item.IsSelected = false;

                if ((i <= SelectionStart) && Item.IsCtrlCode)
                {
                    switch (Item.Code)
                    {
                        case ControlCode.Unindent: if(CtrlStack.Count > 0) CtrlStack.Pop(); break;
                        case ControlCode.Item: CtrlStack.Push(ControlCode.Item); break;
                        case ControlCode.Indent: CtrlStack.Push(ControlCode.Indent); break;
                    }
                }
            }

            // determine indent and item state
            if (CtrlStack.Count > 0)
            {
                ControlCode Code = CtrlStack.Pop();

                if (Code == ControlCode.Indent)
                {
                    m_IsIndent = true;
                    m_IsItem = false;
                }
                else
                {
                    m_IsIndent = false;
                    m_IsItem = true;
                }
            }
            else
            {
                m_IsIndent = false;
                m_IsItem = false;
            }

            
        }

        delegate void EnumSelectedLetterHandler(Letter InLetter);

        void EnumSelectedLetters(EnumSelectedLetterHandler InHandler)
        {
            for (int i = SelectionStart; i < SelectionStart + SelectionLength; i++)
            {
                Letter Item = LineItems[i] as Letter;

                if (Item == null)
                    continue;

                InHandler(Item);
            }
        }

        delegate void EnumSelectedObjectHandler(LineItem InLetter);

        void EnumSelectedObjects(EnumSelectedObjectHandler InHandler)
        {
            for (int i = SelectionStart; i < SelectionStart + SelectionLength; i++)
            {
                LineItem Item = LineItems[i];

                if (Item == null)
                    continue;

                InHandler(Item);
            }
        }

        class Replacement
        {
            public String Macro;
            public IRichTextObject Element;
        }

        /// <summary>
        /// An internal method providing various letter selection options.
        /// </summary>
        /// <param name="InIgnoreSpaces">
        ///     Set to "true" if you want to include spaces in enumeration...
        ///     This is important to determine the current word to select.
        /// </param>
        /// <param name="InIgnoreTimestamps">
        ///     Set to "false" if you want to ensure that all enumerated letters have been typed
        ///     chronologically. This is important for internal macro replacement.
        /// </param>
        /// <param name="InMinIndex">
        ///     The minimum letter index to include in enumeration.
        /// </param>
        /// <param name="InStartIndex">
        ///     The letter index to start enumeration backwards and forwards.
        /// </param>
        /// <param name="InMaxIndex">
        ///     The maximum letter index to include in enumeration.
        /// </param>
        /// <returns></returns>
        Letter[] EnumFrameLetters(
            Boolean InIgnoreSpaces,
            Boolean InIgnoreTimestamps,
            Int32 InMinIndex,
            Int32 InStartIndex,
            Int32 InMaxIndex)
        {
            Letter Prev = null;
            Letter Next = null;

            InStartIndex = Math.Max(0, Math.Min(LineItems.Count - 1, InStartIndex));
            InMinIndex = Math.Max(0, InMinIndex);
            InMaxIndex = Math.Min(LineItems.Count - 1, InMaxIndex);

            for (int i = InStartIndex; i >= InMinIndex; i--)
            {
                Letter Entry = LineItems[i] as Letter;

                if( (Entry == null) ||
                    (!InIgnoreTimestamps && (Prev != null) && (Entry.Timestamp > Prev.Timestamp)) ||
                    (!InIgnoreSpaces && Entry.IsSpace))
                {
                    InMinIndex = i + 1;

                    break;
                }

                Prev = Entry;
            }

            for (int i = InStartIndex; i <= InMaxIndex; i++)
            {
                Letter Entry = LineItems[i] as Letter;

                if ((Entry == null) ||
                    (!InIgnoreTimestamps && (Next != null) && (Entry.Timestamp < Next.Timestamp)) ||
                    (!InIgnoreSpaces && Entry.IsSpace))
                {
                    InMaxIndex = i - 1;

                    break;
                }

                Next = Entry;
            }

            List<Letter> Result = new List<Letter>();

            for(int i = InMinIndex; i <= InMaxIndex; i++)
            {
                Result.Add(LineItems[i] as Letter);
            }

            return Result.ToArray();
        }

        void EnumFrameLetters(
            Boolean InIgnoreSpaces,
            Boolean InIgnoreTimestamps,
            Int32 InMinIndex,
            Int32 InStartIndex,
            Int32 InMaxIndex,
            EnumSelectedLetterHandler InHandler)
        {
            Letter[] Enum = EnumFrameLetters(InIgnoreSpaces, InIgnoreTimestamps, InMinIndex, InStartIndex, InMaxIndex);

            for (int i = 0; i < Enum.Length; i++)
            {
                InHandler(Enum[i]);
            }
        }

        void InternalInsertFrameworkElement(
            Boolean InIsFocusable,
            IRichTextObject InRTO,
            String InMacro,
            FrameworkElement InElement)
        {
            if (InRTO != null)
            {
                Boolean Exists = false;

                for (int i = 0; i < m_RTOList.Count; i++)
                {
                    if (m_RTOList[i].GetTypeID() == InRTO.GetTypeID())
                    {
                        Exists = true;

                        break;
                    }
                }

                if(!Exists)
                    throw new KeyNotFoundException("The given rich text object is not registered in this editor.");
            }
            m_LastModification = DateTime.Now.Ticks;

            InElement.VerticalAlignment = VerticalAlignment.Top;
            InElement.HorizontalAlignment = HorizontalAlignment.Left;

            RemoveRange(SelectionStart, SelectionLength);

            // insert element
            LineItem Entry = new LineItem(this);

            Entry.Item = InElement;
            Entry.IsFocusable = InIsFocusable;
            Entry.RTO = InRTO;
            Entry.Macro = InMacro;

            LineItems.Insert(SelectionStart, Entry);

            if (InIsFocusable)
                m_Overlay.Children.Add(Entry);
            else
                m_Surface.Children.Add(Entry);

            InternalSelect(CursorPosition.End, SelectionStart + 1, 0);
        }


        internal Int32 InternalInsertString(
            Boolean InEnableMacros,
            Int32 InMacroShift,
            String InText)
        {
            Int32 SelStart = SelectionStart;
            Int32 FrameEnd = SelStart + InText.Length;

            m_LastModification = DateTime.Now.Ticks;

            for (int i = 0, iReal = 0; i < InText.Length; i++)
            {
                Char c = InText[i];
                Boolean Continue = false;
                Boolean IsEnter = false;
                LineItem Entry = null;

                if (Char.IsControl(c))
                {
                    Continue = true;

                    if ((c == '\r') || (c == '\n'))
                        IsEnter = true;

                    if (IsEnter)
                    {
                        Entry = new LineItem(this, ControlCode.Enter);

                        LineItems.Insert(SelStart + iReal++, Entry);

                        m_Surface.Children.Add(Entry);
                    }
                }
                else if (Char.IsSeparator(c))
                {
                    switch (c)
                    {
                        case ' ': break;
                        case '\t':
                            {
                                for (int ix = 0; ix < 4; ix++)
                                {
                                    Entry = new Letter(this, ' ');

                                    LineItems.Insert(SelStart + iReal++, Entry);

                                    m_Surface.Children.Add(Entry);
                                }

                                Continue = true;
                            } break;
                        default:
                            {
                                Continue = true;
                            } break;
                    }
                }

                if(Continue)
                    continue;

                Entry = new Letter(this, InText[i]);

                LineItems.Insert(SelStart + iReal++, Entry);

                m_Surface.Children.Add(Entry);
            }

            if (InEnableMacros)
            {
                // check if we can replace substrings with a macro...
                Letter[] Letters = EnumFrameLetters(true, false, SelStart - InMacroShift, SelStart, SelStart + InText.Length);
                StringBuilder Builder = new StringBuilder();
                Int32 FrameStart = SelStart - (Letters.Length - InText.Length);

                for (int i = 0; i < Letters.Length; i++) { Builder.Append(Char.ToLower(Letters[i].Value)); }

                for (int i = 0, iSel = FrameStart; i < Letters.Length; i++, iSel++)
                {
                    String Current = Builder.ToString(i, Letters.Length - i);

                    for (int iMacro = 0; iMacro < RepList.Count; iMacro++)
                    {
                        Replacement Rep = RepList[iMacro];

                        if (Current.StartsWith(Rep.Macro))
                        {
                            InternalSelect(CursorPosition.Start, iSel, Rep.Macro.Length);

                            i += Rep.Macro.Length - 1;
                            FrameEnd -= Rep.Macro.Length - 1;

                            InternalInsertFrameworkElement(false, null, Rep.Macro, Rep.Element.CreateInstance());

                            break;
                        }
                    }
                }
            }

            return FrameEnd;
        }

        void RemoveRange(
            Int32 InStart,
            Int32 InLength)
        {
            m_LastModification = DateTime.Now.Ticks;

            if (InStart < 0)
            {
                InLength += InStart;
                InStart = 0;
            }

            for (int i = InStart; i < InStart + InLength; i++)
            {
                if (LineItems[i].IsFocusable)
                    m_Overlay.Children.Remove(LineItems[i]);
                else
                    m_Surface.Children.Remove(LineItems[i]);
            }

            LineItems.RemoveRange(InStart, InLength);

            if ((LineItems.Count == 0) || (LineItems[LineItems.Count - 1].Code != ControlCode.Terminator))
                LineItems.Add(new LineItem(this, ControlCode.Terminator));

            if (SelectionStart >= LineItems.Count)
                Select(CursorPosition.Start, SelectionStart, 0);

            if (SelectionStart + SelectionLength >= LineItems.Count)
                Select(CursorPosition.Start, SelectionStart, LineItems.Count - SelectionStart);

            Invalidate();

            
        }

        /// <summary>
        /// Will update the whole selection and sets SelectionStart to "InStart",
        /// SelectionLength to "InLength" and the cursor to either SelectionStart
        /// or SelectionEnd, depending on the value set for "InCursorPos".
        /// </summary>
        /// <param name="InCursorPos"></param>
        /// <param name="InStart"></param>
        /// <param name="InLength"></param>
        void Select(
            CursorPosition InCursorPos,
            Int32 InStart,
            Int32 InLength)
        {
            InternalSelect(InCursorPos, InStart, InLength);

            UpdateSelection();

            if (OnSelectionChanged != null)
                OnSelectionChanged(this);
        }

        void InternalSelect(
            CursorPosition InCursorPos,
            Int32 InStart,
            Int32 InLength)
        {
            if (InStart < 0)
                InStart = 0;

            if (InLength < 0)
                InLength = 0;

            if (InStart >= LineItems.Count)
                InStart = LineItems.Count - 1;

            if (InStart + InLength >= LineItems.Count)
                InLength = LineItems.Count - InStart;

            m_SelectionStart = InStart;
            m_SelectionEnd = InStart + InLength;

            if (InCursorPos == CursorPosition.Start)
                m_CursorPosition = SelectionStart;
            else
                m_CursorPosition = SelectionEnd;
        }

        /// <summary>
        /// Just moves the cursor. If "Shift" is not pressed during this call, the current
        /// selection length will be set to null. Otherwise the selection will be updated, so
        /// that SelectionStart stays unchanged and SelectionEnd including the cursor will be 
        /// set to the new position. Please note that this is only the internal behavior.
        /// If SelectionEnd is smaller than SelectionStart, the public properties will also
        /// switch so that SelectionStart is always smaller or equal to SelectionEnd.
        /// </summary>
        /// <param name="InPosition"></param>
        bool MoveCursor(Int32 InPosition)
        {
            bool Result = true;

            if (InPosition < 0)
            {
                InPosition = 0;

                Result = false;
            }

            if (InPosition >= LineItems.Count)
            {
                InPosition = LineItems.Count - 1;

                Result = false;
            }

            if (IsShiftDown > 0)
            {
                m_SelectionStart = ShiftCursorPos;
                m_SelectionEnd = InPosition;
                m_CursorPosition = InPosition;
            }
            else
            {
                Select(CursorPosition.Start, InPosition, 0);
            }

            UpdateSelection();

            m_CursorTick = 0;

            if (OnSelectionChanged != null)
                OnSelectionChanged(this);

            return Result;
        }

        internal void ForceValidSnapshot(Int64 InTimestamp)
        {
            if (m_LastModification > InTimestamp)
                throw new InvalidOperationException("Related RichTextEdit has been changed since snapshot creation.");
        }

        internal Int32 SelectionStart
        {
            get
            {
                if (m_SelectionStart <= m_SelectionEnd)
                    return Math.Max(0, m_SelectionStart);
                else
                    return Math.Max(0, m_SelectionEnd);
            }
        }
        internal Int32 SelectionEnd
        {
            get
            {
                if (m_SelectionStart <= m_SelectionEnd)
                    return Math.Min(LineItems.Count, m_SelectionEnd);
                else
                    return Math.Min(LineItems.Count, m_SelectionStart);
            }
        }
        internal Int32 SelectionLength
        {
            get
            {
                if (m_SelectionEnd < m_SelectionStart)
                    return Math.Max(0, m_SelectionStart - m_SelectionEnd);
                else
                    return Math.Max(0, m_SelectionEnd - m_SelectionStart);
            }
        }

        void InternalInsertCtrlCode(ControlCode InCode)
        {
            m_LastModification = DateTime.Now.Ticks;

            if (InCode == ControlCode.Item)
            {
                InsertCtrlCode(ControlCode.Enter);
            }

            LineItem Entry = new LineItem(this, InCode);

            LineItems.Insert(SelectionStart, Entry);

            m_Surface.Children.Add(Entry); 
        }
    }

    public interface IRichTextObject
    {
        Int16 GetTypeID();
        Boolean IsFocusable { get; }
        void Serialize(FrameworkElement InElement, BinaryWriter InTarget);
        FrameworkElement Deserialize(
            Boolean InIgnoreWarnings,
            BinaryReader InSource);
        FrameworkElement CreateInstance();
    }
}

