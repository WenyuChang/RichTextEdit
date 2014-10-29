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
        class Letter : LineItem
        {
            private Double m_FontSize = 0;

            public Char Value { get; private set; }
            public TextBlock Label { get; private set; }
            public readonly Int64 Timestamp = DateTime.Now.Ticks;
            public Double FontSize
            {
                get
                {
                    return m_FontSize;
                }
                set
                {
                    m_FontSize = value;

                    if (IsSub || IsSup)
                        Label.FontSize = value * 0.7;
                    else
                        Label.FontSize = value;
                }
            }
            public override TextAttributes TextAttributes
            {
                set
                {
                    if (m_TextAttributes == value)
                        return;

                    if (Label != null)
                    {
                        if (((value & TextAttributes.Sup) != 0) || ((value & TextAttributes.Sub) != 0))
                            Label.FontSize = m_FontSize * 0.7;
                        else
                            Label.FontSize = m_FontSize;
                    }
                    base.TextAttributes = value;
                }
            }

            public override void UpdateSelection()
            {
                base.UpdateSelection();

                if (Label != null)
                {
                    if (IsSelected)
                    {
                        Label.Foreground = new SolidColorBrush(m_RichText.SelectionForeground);
                    }
                    else
                    {
                        Label.Foreground = new SolidColorBrush(Foreground);
                    }
                }
            }

            public Letter(
                RichTextEdit InParent,
                Char InValue) :
                base(InParent)
            {
                Label = new TextBlock();

                Item = Label;

                Label.VerticalAlignment = VerticalAlignment.Top;
                Label.HorizontalAlignment = HorizontalAlignment.Left;
                Label.Margin = new Thickness(0, 0, 0, 0);

                Value = InValue;

                Label.FontFamily = InParent.FontFamily;
                Label.FontStyle = InParent.FontStyle;
                Label.FontWeight = InParent.FontWeight;
                FontSize = InParent.FontSize;
                Alignment = InParent.TextAlignment;
                Foreground = InParent.FontForeground;
                Background = InParent.FontBackground;

                Label.Text = new String(Value, 1);
                IsSpace = Char.IsSeparator(Value);
                TextAttributes = InParent.TextAttributes;
            }
        }
    }
}
