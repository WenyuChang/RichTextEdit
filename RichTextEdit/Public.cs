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
    public delegate void NotificationHandler(object sender);
    
    public partial class RichTextEdit
    {
        public Int32 MaxUndoCount
        {
            get
            {
                return m_MaxUndoCount;
            }

            set
            {
                if ((value < 10) || (value > 200))
                    throw new ArgumentOutOfRangeException("Only values between 10 and 200 are supported.");

                m_MaxUndoCount = value;
            }
        }

        public event NotificationHandler OnSelectionChanged;
        public event NotificationHandler OnContentChanged;
        public Color SelectionBackground
        {
            get { return m_SelectionBackground; }
            set
            {
                m_SelectionBackground = value;

                for (int i = 0; i < LineItems.Count; i++)
                {
                    LineItems[i].UpdateSelection();
                }
            }
        }
        public Color SelectionForeground
        {
            get { return m_SelectionForeground; }
            set
            {
                m_SelectionForeground = value;

                for (int i = 0; i < LineItems.Count; i++)
                {
                    LineItems[i].UpdateSelection();
                }
            }
        }
        public Boolean HasMaxTextWidth
        { get { return !Double.IsNaN(MaxTextWidth); } }
        public Double MaxTextWidth
        {
            get { return m_MaxTextWidth; }
            set
            {
                if (value < 0)
                    value = 0;

                m_MaxTextWidth = value;

                Invalidate();
            }
        }
        public CornerRadius BorderRadius
        {
            get { return m_Border.CornerRadius; }
            set
            {
                m_Border.CornerRadius = value;

                UpdateSize();
            }
        }
        public Thickness BorderThickness
        {
            get { return m_Border.BorderThickness; }
            set
            {
                m_Border.BorderThickness = value;

                UpdateSize();
            }
        }
        public Brush BorderBrush
        {
            get { return m_Border.BorderBrush; }
            set{m_Border.BorderBrush = value;}
        }
        public Double VerticalScrollBarWidth { get; set; }
        public Double HorizontalScrollBarHeight { get; set; }
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return m_Scroll.HorizontalScrollBarVisibility; }
            set { 
                m_Scroll.HorizontalScrollBarVisibility = value;

                Invalidate();
            }
        }
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return m_Scroll.VerticalScrollBarVisibility; }
            set
            {
                m_Scroll.VerticalScrollBarVisibility = value;

                UpdateSize();
            }
        }

        public new Brush Background
        {
            get { return new SolidColorBrush(m_BackgroundColor); }
        }

        private Color m_BackgroundColor = Colors.White;

        public Color BackgroundColor
        {
            get { return m_BackgroundColor; }
            set
            {
                if (value == Colors.Transparent)
                    throw new InvalidOperationException("Since Silverlight 2 Beta 2, a transparent background is not supported anymore!");

                m_Border.Background = new SolidColorBrush(value);
                m_Surface.Background = new SolidColorBrush(value);
                m_BackgroundColor = BackgroundColor;
            }
        }
        public Color CursorColor { get; set; }
        public RichTextRestriction Restriction { get; set; }
        public FontFamily FontFamily
        {
            get
            {
                return m_FontFamily;
            }
            set
            {
                EnumSelectedLetters((Letter obj) => { obj.Label.FontFamily = value; });

                m_FontFamily = value;

                Invalidate();

                
            }
        }
        public FontStyle FontStyle
        {
            get
            {
                return m_FontStyle;
            }
            set
            {
                EnumSelectedLetters((Letter obj) => { obj.Label.FontStyle = value; });

                m_FontStyle = value;

                Invalidate();

                
            }
        }
        public FontWeight FontWeight
        {
            get
            {
                return m_FontWeight;
            }
            set
            {
                EnumSelectedLetters((Letter obj) => { obj.Label.FontWeight = value; });

                m_FontWeight = value;

                Invalidate();

                
            }
        }
        public FontStretch FontStretch
        {
            get
            {
                return m_FontStretch;
            }
            set
            {
                EnumSelectedLetters((Letter obj) => { obj.Label.FontStretch = value; });

                m_FontStretch = value;

                Invalidate();

                
            }
        }
        public Double FontSize
        {
            get
            {
                return m_FontSize;
            }
            set
            {
                EnumSelectedLetters((Letter obj) => { obj.FontSize = value; });

                m_FontSize = value;

                Invalidate();

                
            }
        }
        public Color FontForeground
        {
            get
            {
                return m_FontForeground;
            }
            set
            {
                EnumSelectedObjects((LineItem obj) => { obj.Foreground = value; });

                m_FontForeground = value;

                Invalidate();

                
            }
        }
        public Color FontBackground
        {
            get
            {
                return m_FontBackground;
            }
            set
            {
                EnumSelectedObjects((LineItem obj) => { obj.Background = value; });

                m_FontBackground = value;

                Invalidate();

                
            }
        }
        public TextAttributes TextAttributes
        {
            get
            {
                return m_TextAttributes;
            }
            set
            {

                EnumSelectedObjects((LineItem obj) =>
                {
                    obj.TextAttributes = value;
                });

                m_TextAttributes = value;

                Invalidate();

                
            }
        }
        public TextAlign TextAlignment
        {
            get
            {
                return m_TextAlignment;
            }
            set
            {
                EnumSelectedLetters((Letter obj) => { obj.Alignment = value; });
                EnumFrameLetters(true, true, 0, SelectionStart, LineItems.Count, (Letter obj) =>{obj.Alignment = value;});
                EnumFrameLetters(true, true, 0, SelectionStart + SelectionLength, LineItems.Count, (Letter obj) => { obj.Alignment = value; });

                m_TextAlignment = value;

                Invalidate();

                
            }
        }
        public Boolean HasClipboardAccess 
        { get 
        {
            NegotiateClipboardAccess();
            return m_HasClipboardAccess > 0;
        }
        }
        public Boolean AutoFocus { get; set; }
        public Boolean IsReadOnly
        {
            get
            {
                return m_IsReadOnly;
            }

            set
            {
                if (!value)
                    m_Cursor.Visibility = Visibility.Visible;
                else
                    m_Cursor.Visibility = Visibility.Collapsed;

                Select(CursorPosition.Start, 0, 0);

                m_IsReadOnly = value;
            }
        }

        public void AddMacro(
            String InMacro,
            IRichTextObject InReplacement)
        {
            if ((InMacro == null) || (InReplacement == null))
                throw new ArgumentNullException();

            if (InMacro.Length < 2)
                throw new ArgumentException("At least two characters are required for a macro!");

            InMacro = InMacro.ToLower();

            for (int i = 0; i < RepList.Count; i++)
            {
                if (RepList[i].Macro.CompareTo(InMacro) == 0)
                {
                    RepList[i].Element = InReplacement;

                    return;
                }

                if (RepList[i].Macro.StartsWith(InMacro) || InMacro.StartsWith(RepList[i].Macro))
                    throw new ArgumentException("The given macro conflicts with at least one existing macro.");
            }

            RepList.Add(new Replacement()
            {
                Macro = InMacro,
                Element = InReplacement,
            });

            m_MaxMacroLen = Math.Max(m_MaxMacroLen, InMacro.Length);
        }

        public void RemoveMacro(String InMacro)
        {
            InMacro = InMacro.ToLower();

            for (int i = 0; i < RepList.Count; i++)
            {
                if (RepList[i].Macro.CompareTo(InMacro) == 0)
                {
                    RepList.RemoveAt(i);

                    break;
                }
            }
        }

        public void InsertObject(
            Boolean InFocusable,
            FrameworkElement InElement)
        {
            InternalInsertFrameworkElement(InFocusable, null, null, InElement);

            Update();

            Select(CursorPosition.End, SelectionStart, 0);

            if (OnContentChanged != null)
                OnContentChanged(this);

            
        }

        public void InsertObject(IRichTextObject InObject)
        {
            InternalInsertFrameworkElement(InObject.IsFocusable, InObject, null, InObject.CreateInstance());

            Update();

            Select(CursorPosition.End, SelectionStart, 0);

            if (OnContentChanged != null)
                OnContentChanged(this);

            
        }

        public void InsertString(String InText)
        {
            Int32 SelStart = SelectionStart;
            Int32 FrameEnd;

            // insert letters...
            RemoveRange(SelectionStart, SelectionLength);

            FrameEnd = InternalInsertString(true, m_MaxMacroLen, InText);
            
            Update();

            Select(CursorPosition.End, FrameEnd, 0);

            if (OnContentChanged != null)
                OnContentChanged(this);

            
        }

        public void InsertCtrlCode(ControlCode InCode)
        {
            Int32 SelStart = SelectionStart;
            Int32 SelLength = SelectionLength;

            switch (InCode)
            {
                case ControlCode.Enter:
                    {
                        RemoveSelection();
                        SelLength = 0;
                    } break;

                case ControlCode.Indent:
                case ControlCode.Item:
                case ControlCode.Unindent:
                    {
                        Select(CursorPosition.End, SelectionStart, 0);
                    } break;

                default:
                    throw new ArgumentException("Unknown control code.");
            }

            InternalInsertCtrlCode(InCode);

            Update();

            Select(CursorPosition.End, SelStart + 1, SelLength);

            if (OnContentChanged != null)
                OnContentChanged(this);

            
        }

        public void SelectAll()
        {
            Select(CursorPosition.End, 0, LineItems.Count - 1);
        }

        public void RemoveSelection()
        {
            RemoveRange(SelectionStart, SelectionLength);

            Select(CursorPosition.Start, SelectionStart, 0);

            if (OnContentChanged != null)
                OnContentChanged(this);

            
        }
    }
}
