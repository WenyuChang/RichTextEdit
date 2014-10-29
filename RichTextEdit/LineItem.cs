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
using System.Windows.Controls;
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
    public partial class RichTextEdit
    {
        class LineItem : Grid
        {
            private static ImageSource EnumIcon = ResourceManager.LoadImage("EnumIndent.png", 30, 10);

            private Boolean m_IsInvalidated = true;
            private Double OldFastActualWidth;
            private Double OldFastActualHeight;
            private Double OldFastWidth;
            private Double OldFastHeight;
            private Double OldFastLeft;
            private Double OldFastTop;
            private Double OldFastItemTop;
            private Boolean OldFastIsRenderable;
            private Boolean OldIsSelected;
            private TextAlign m_Alignment;
            private Double OldLineHeight;
            private Color m_Background = Colors.Transparent;
            private Boolean m_IsSelected = false;
            private FrameworkElement m_Item;
            protected TextAttributes m_TextAttributes = TextAttributes.None;
            private Line m_Line = null;
            private Color m_Foreground = Colors.Black;

            public Color Foreground
            {
                get { return m_Foreground; }
                set
                {
                    if (m_Foreground.Equals(value))
                        return;

                    m_Foreground = value;

                    m_RichText.Invalidate();

                    UpdateSelection();
                }
            }
            public Boolean IsSub
            {
                get { return (m_TextAttributes & TextAttributes.Sub) != 0; }
            }
            public Boolean IsSup
            {
                get { return (m_TextAttributes & TextAttributes.Sup) != 0; }
            }
            public IRichTextObject RTO = null;
            public String Macro = null;
            public Boolean IsSelected
            {
                get { return m_IsSelected; }
                set
                {
                    if (value == m_IsSelected)
                        return;

                    m_IsSelected = value;

                    m_RichText.Invalidate();
                }
            }
            public new Color Background
            {
                get { return m_Background; }
                set
                {
                    if (m_Background.Equals(value))
                        return;

                    m_Background = value;

                    m_RichText.Invalidate();

                    UpdateSelection();
                }
            }
            public RichTextEdit m_RichText;
            public TextAlign Alignment
            {
                get { return m_Alignment; }
                set
                {
                    if (m_Alignment == value)
                        return;

                    m_Alignment = value;

                    m_RichText.Invalidate();
                }
            }
            public Boolean IsVisible
            {
                get { return Visibility == Visibility.Visible; }
                set
                {
                    if (value)
                        Visibility = Visibility.Visible;
                    else
                        Visibility = Visibility.Collapsed;
                }
            }
            public Double Left
            {
                get { return Margin.Left; }
                set { Margin = new Thickness(value, Margin.Top, 0, 0); }
            }
            public Double Top
            {
                get { return Margin.Top; }
                set { Margin = new Thickness(Margin.Left, value, 0, 0); }
            }
            public Double FastItemTop { get; set; }
            public Double FastActualWidth { get; private set; }
            public Double FastActualHeight { get; private set; }
            public Double FastWidth { get; set; }
            public Double FastHeight { get; set; }
            public Double FastLeft { get; set; }
            public Double FastTop { get; set; }
            public Boolean FastIsRenderable { get; set; }
            public ControlCode Code { get; private set; }
            public Double LineHeight { get; set; }
            public Boolean IsFocusable { get; set; }
            public Int32 LineIndex { get; set; }
            public Int32 Index = -1;
            public FrameworkElement Item
            {
                get { return m_Item; }
                set
                {
                    if (m_Item != null)
                        throw new InvalidOperationException();

                    Children.Add(value);

                    m_Item = value;
                    m_Item.SizeChanged += new SizeChangedEventHandler(m_Item_SizeChanged);
                }
            }
            public virtual Boolean IsSpace { get; set; }
            public Boolean IsCtrlCode
            {
                get { return Code != ControlCode.None; }
            }
            public Boolean HasChanged = false;
            public Boolean IsUnderlined
            {
                get { return (m_TextAttributes & TextAttributes.Underlined) != 0; }
            }
            public virtual TextAttributes TextAttributes
            {
                get { return m_TextAttributes; }
                set
                {
                    if (m_TextAttributes == value)
                        return;

                    m_TextAttributes = value;

                    if (IsUnderlined)
                    {
                        SetUnderline(ActualWidth);
                    }
                    else
                    {
                        if (m_Line != null)
                            Children.Remove(m_Line);

                        m_Line = null;
                    }

                    Invalidate();

                    m_RichText.Invalidate();
                }
            }

            public virtual void ReadState()
            {
                if (!m_IsInvalidated)
                    return;

                m_IsInvalidated = false;

                // read width
                FastActualWidth = FastWidth = (Item != null) ? Item.ActualWidth : 0;

                if (!Double.IsNaN(Width))
                    FastWidth = Width;

                if (!IsCtrlCode || (Code == ControlCode.Item))
                {
                    // read height
                    FastActualHeight = FastHeight = (Item != null) ? Item.ActualHeight : 0;

                    if (!Double.IsNaN(Height))
                        FastHeight = Height;
                }
                else
                    FastActualHeight = FastHeight = Height;

                FastIsRenderable = IsVisible;

                // read margin
                if (Item != null)
                {
                    FastItemTop = Item.Margin.Top;
                    FastLeft = Item.Margin.Left;
                    FastTop = Item.Margin.Top;
                }

                OldFastActualWidth = FastActualWidth;
                OldFastActualHeight = FastActualHeight;
                OldFastWidth = FastWidth;
                OldFastHeight = FastHeight;
                OldFastLeft = FastLeft;
                OldFastTop = FastTop;
                OldFastIsRenderable = FastIsRenderable;
            }

            public virtual void WriteState()
            {
                HasChanged = false;

                    if (OldLineHeight != LineHeight)
                        HasChanged = true;

                    if (OldFastWidth != FastWidth)
                    {
                        Width = FastWidth;

                        HasChanged = true;
                    }

                    if (!OldFastHeight.Equals(FastHeight))
                    {
                        if (IsCtrlCode)
                            Height = LineHeight;
                        else
                            Height = FastHeight;

                        HasChanged = true;
                    }

                if (Item != null)
                {
                    if (OldFastItemTop != FastItemTop)
                        Item.Margin = new Thickness(0, FastItemTop, 0, 0);
                }

                if ((OldFastLeft != FastLeft) || (OldFastTop != FastTop))
                    Margin = new Thickness(FastLeft, FastTop, 0, 0);

                if (FastIsRenderable != OldFastIsRenderable)
                {
                    IsVisible = FastIsRenderable;

                    HasChanged = true;
                }

                OldLineHeight = LineHeight;
                OldFastItemTop = FastItemTop;
                OldFastWidth = FastWidth;
                OldFastHeight = FastHeight;
                OldFastLeft = FastLeft;
                OldFastTop = FastTop;
                OldFastIsRenderable = FastIsRenderable;

                if (HasChanged && IsUnderlined)
                {
                    SetUnderline(Double.IsNaN(Width) ? Item.ActualWidth : Width);
                }

                UpdateSelection();
            }

            void SetUnderline(Double InWidth)
            {
                if (!IsVisible)
                {
                    if (m_Line != null)
                        Children.Remove(m_Line);

                    m_Line = null;

                    return;
                }

                if (m_Line == null)
                {
                    m_Line = new Line();

                    m_Line.Margin = new Thickness(0, 0, 0, 0);
                    m_Line.HorizontalAlignment = HorizontalAlignment.Left;
                    m_Line.VerticalAlignment = VerticalAlignment.Top;
                    m_Line.X1 = 0;

                    Children.Add(m_Line);
                }

                m_Line.Stroke = new SolidColorBrush(m_Foreground);
                m_Line.StrokeThickness = 1;
                m_Line.Y1 = m_Line.Y2 = LineHeight - m_Line.StrokeThickness;
                m_Line.X2 = InWidth;
                m_Line.Width = InWidth;
            }

            public LineItem(RichTextEdit InParent)
            {
                HorizontalAlignment = HorizontalAlignment.Left;
                VerticalAlignment = VerticalAlignment.Top;
                Margin = new Thickness(0, 0, 0, 0);

                ReadState();

                m_RichText = InParent;
                SizeChanged += new SizeChangedEventHandler(LineItem_SizeChanged);
                TextAttributes = InParent.TextAttributes;

                Foreground = InParent.FontForeground;
                Background = InParent.FontBackground;

                InternalUpdateSelection();
            }

            void LineItem_SizeChanged(object sender, SizeChangedEventArgs e)
            {
                ReadState();

                if (IsUnderlined)
                {
                    SetUnderline(Double.IsNaN(FastWidth)?FastActualWidth:FastWidth);
                }

                Invalidate();
            }

            void m_Item_SizeChanged(object sender, SizeChangedEventArgs e)
            {
                Invalidate();
            }

            protected void Invalidate()
            {
                m_IsInvalidated = true;

                ReadState();

                m_RichText.Invalidate();
            }

            public virtual void UpdateSelection()
            {
                if (OldIsSelected == m_IsSelected)
                    return;

                InternalUpdateSelection();
            }
            
            public void InternalUpdateSelection()
            {
                if (m_IsSelected)
                {
                    ((Grid)this).Background = new SolidColorBrush(m_RichText.SelectionBackground);

                    if (m_Line != null)
                        m_Line.Stroke = new SolidColorBrush(m_RichText.SelectionForeground);
                }
                else
                {
                    ((Grid)this).Background = new SolidColorBrush(m_Background);

                    if (m_Line != null)
                        m_Line.Stroke = new SolidColorBrush(m_Foreground);
                }

                OldIsSelected = m_IsSelected;
            }

            public LineItem(
                RichTextEdit InParent,
                ControlCode InCode)
                : this(InParent)
            {
                Code = InCode;

                if (Code == ControlCode.Item)
                {
                    Item = EnumIcon.CreateImage();
                }
                else
                    Height = 20;
            }
        }
    }
}
