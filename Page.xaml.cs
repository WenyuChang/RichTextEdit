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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Browser;


namespace RichText
{
    public partial class Page : UserControl
    {
        private Boolean IsUpdating = false;

        public Page()
        {
            InitializeComponent();

            RichEdit.AutoFocus = true;
            //RichEdit.InsertString("Jhfakdh fajh sdlkj fag lah lasgkjf shfdgga dfgdjs hgak sas  hfs dfg adhfga skj g fhgsd  h sfa sd f dsf as d fshfg sd f  sdhf asdh gfs dfh akshdgsd gasdh gsdgadfk ghf aj agjgfa kdf sfga fa akjdsf Jhfakdh fajh sdlkjfag lah lasgkjf shfdg gadfg djshgak sas  hfsgdfgsadhfgaskj g fhgsd  h sfa sd f dsf as d fshfg sd f  sdhf asdh gfs dfh akshdgsd gasdh gsdgadfk ghf aj agjgfa kdf sfga fa akjdsf Jhfakdh fajh sdlkjfag lah lasgkjf.");
            RichEdit.OnSelectionChanged += new NotificationHandler(RichEdit_OnSelectionChanged);
            RichEdit.OnContentChanged += new NotificationHandler(RichEdit_OnContentChanged);

            MSNEmoticons.Apply(RichEdit);

            LIST_Align.Items.Add("Left");
            LIST_Align.Items.Add("Center");
            LIST_Align.Items.Add("Block");

            LIST_Color.Items.Add("Transparent");
            LIST_Color.Items.Add("White");
            LIST_Color.Items.Add("Yellow");
            LIST_Color.Items.Add("Orange");
            LIST_Color.Items.Add("Red");
            LIST_Color.Items.Add("Green");
            LIST_Color.Items.Add("Magenta");
            LIST_Color.Items.Add("Blue");
            LIST_Color.Items.Add("Brown");
            LIST_Color.Items.Add("Gray");
            LIST_Color.Items.Add("Black");

            for (int i = 10; i < 26; i += 2)
            {
                LIST_Size.Items.Add(i.ToString());
            }

            LIST_Stretch.Items.Add("Normal");
            LIST_Stretch.Items.Add("Condensed");
            LIST_Stretch.Items.Add("Expanded");
            LIST_Stretch.Items.Add("ExtraCondensed");
            LIST_Stretch.Items.Add("ExtraExpanded");

            LIST_Weight.Items.Add("Thin");
            LIST_Weight.Items.Add("Normal");
            LIST_Weight.Items.Add("Bold");
            LIST_Weight.Items.Add("ExtraBold");

            List_Family.Items.Add("Portable");
            List_Family.Items.Add("Arial");
            List_Family.Items.Add("Arial Black");
            List_Family.Items.Add("Comic Sans MS");
            List_Family.Items.Add("Courier New");
            List_Family.Items.Add("Georgia");
            List_Family.Items.Add("Lucida Grande");
            List_Family.Items.Add("Lucida Sans Unicode");
            List_Family.Items.Add("Times New Roman");
            List_Family.Items.Add("Trebuchet MS");
            List_Family.Items.Add("Verdana");
            List_Family.Items.Add("Webdings");

            LIST_Border.Items.Add("None");
            LIST_Border.Items.Add("Normal");
            LIST_Border.Items.Add("Bold");
            LIST_Border.Items.Add("Crazy");
            LIST_Border.Items.Add("Colored");

            // register custom rich text object
            RichEdit.RegisterObject(new SerializableButton(null));
        }

        void RichEdit_OnContentChanged(object sender)
        {
            BTN_FindNext.IsEnabled = false;
            BTN_Replace.IsEnabled = false;
        }

        void RichEdit_OnSelectionChanged(object sender)
        {
            IsUpdating = true;

            RBN_ColorSelection_Click(null, null);

            CHK_Italic.IsChecked = (RichEdit.FontStyle == FontStyles.Italic);
            CHK_Sup.IsChecked = (RichEdit.TextAttributes & TextAttributes.Sup) != 0;
            CHK_Sub.IsChecked = (RichEdit.TextAttributes & TextAttributes.Sub) != 0;
            CHK_Underlined.IsChecked = (RichEdit.TextAttributes & TextAttributes.Underlined) != 0;

            if (RichEdit.FontWeight == FontWeights.Thin) LIST_Weight.SelectedIndex = 0;
            if (RichEdit.FontWeight == FontWeights.Normal) LIST_Weight.SelectedIndex = 1;
            if (RichEdit.FontWeight == FontWeights.Bold) LIST_Weight.SelectedIndex = 2;
            if (RichEdit.FontWeight == FontWeights.ExtraBold) LIST_Weight.SelectedIndex = 3;

            if (RichEdit.FontStretch == FontStretches.Normal) LIST_Stretch.SelectedIndex = 0;
            if (RichEdit.FontStretch == FontStretches.Condensed) LIST_Stretch.SelectedIndex = 1;
            if (RichEdit.FontStretch == FontStretches.Expanded) LIST_Stretch.SelectedIndex = 2;
            if (RichEdit.FontStretch == FontStretches.ExtraCondensed) LIST_Stretch.SelectedIndex = 3;
            if (RichEdit.FontStretch == FontStretches.ExtraExpanded) LIST_Stretch.SelectedIndex = 4;

            switch (RichEdit.FontFamily.Source)
            {
                case "Portable": List_Family.SelectedIndex = 0; break;
                case "Arial": List_Family.SelectedIndex = 1; break;
                case "Arial Black": List_Family.SelectedIndex = 2; break;
                case "Comic Sans MS": List_Family.SelectedIndex = 3; break;
                case "Courier New": List_Family.SelectedIndex = 4; break;
                case "Georgia": List_Family.SelectedIndex = 5; break;
                case "Lucida Grande": List_Family.SelectedIndex = 6; break;
                case "Lucida Sans Unicode": List_Family.SelectedIndex = 7; break;
                case "Times New Roman": List_Family.SelectedIndex = 8; break;
                case "Trebuchet MS": List_Family.SelectedIndex = 9; break;
                case "Verdana": List_Family.SelectedIndex = 10; break;
                case "Webdings": List_Family.SelectedIndex = 11; break;

                default:
                    throw new Exception();
            }

            CHK_Italic.IsChecked = (RichEdit.FontStyle == FontStyles.Italic);
            LIST_Size.SelectedIndex = (Int32)((RichEdit.FontSize - 10) / 2);

            if (RichEdit.TextAlignment == TextAlign.Left) LIST_Align.SelectedIndex = 0;
            if (RichEdit.TextAlignment == TextAlign.Center) LIST_Align.SelectedIndex = 1;
            if (RichEdit.TextAlignment == TextAlign.Block) LIST_Align.SelectedIndex = 2;

            IsUpdating = false;
        }

        private void LIST_Weight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsUpdating) return;

            switch ((String)LIST_Weight.SelectedItem)
            {
                case "Thin": RichEdit.FontWeight = FontWeights.Thin; break;
                case "Normal": RichEdit.FontWeight = FontWeights.Normal; break;
                case "Bold": RichEdit.FontWeight = FontWeights.Bold; break;
                case "ExtraBold": RichEdit.FontWeight = FontWeights.ExtraBold; break;
                default:
                    throw new Exception();
            }
        }

        private void LIST_Align_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsUpdating) return;
            switch ((String)LIST_Align.SelectedItem)
            {
                case "Left": RichEdit.TextAlignment = TextAlign.Left; break;
                case "Center": RichEdit.TextAlignment = TextAlign.Center; break;
                case "Block": RichEdit.TextAlignment = TextAlign.Block; break;
                default:
                    throw new Exception();
            }
        }

        private void LIST_Border_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsUpdating) return;

            switch ((String)LIST_Border.SelectedItem)
            {
                case "None":
                    {
                        RichEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                        RichEdit.BorderRadius = new CornerRadius(0);
                        RichEdit.BorderBrush = new SolidColorBrush(Colors.Transparent);
                        RichEdit.BackgroundColor = Colors.White;
                    } break;
                case "Normal":
                    {
                        RichEdit.BorderThickness = new Thickness(1, 1, 1, 1);
                        RichEdit.BorderRadius = new CornerRadius(3);
                        RichEdit.BorderBrush = new SolidColorBrush(Colors.Black);
                        RichEdit.BackgroundColor = Colors.White;
                    } break;
                case "Bold":
                    {
                        RichEdit.BorderThickness = new Thickness(3, 3, 3, 3);
                        RichEdit.BorderRadius = new CornerRadius(6);
                        RichEdit.BorderBrush = new SolidColorBrush(Colors.Black);
                        RichEdit.BackgroundColor = Colors.White;
                    } break;
                case "Crazy":
                    {
                        RichEdit.BorderThickness = new Thickness(1, 2, 3, 4);
                        RichEdit.BorderRadius = new CornerRadius(3, 6, 9, 9);
                        RichEdit.BorderBrush = new SolidColorBrush(Colors.Black);
                        RichEdit.BackgroundColor = Colors.White;
                    } break;
                case "Colored":
                    {
                        LinearGradientBrush Lin = new LinearGradientBrush();

                        RichEdit.BorderThickness = new Thickness(3, 3, 3, 3);
                        RichEdit.BorderRadius = new CornerRadius(6);
                        RichEdit.BackgroundColor = Colors.LightGray;
                        RichEdit.Background.Opacity = 0.75;
                        RichEdit.BorderBrush = Lin;

                        Lin.StartPoint = new Point(0, 0);
                        Lin.EndPoint = new Point(0, 1);

                        GradientStop StopA = new GradientStop();
                        GradientStop StopB = new GradientStop();

                        StopA.Color = Colors.White;
                        StopB.Offset = 0.0;

                        StopA.Color = Colors.Magenta;
                        StopB.Offset = 1.0;

                        Lin.GradientStops.Add(StopA);
                        Lin.GradientStops.Add(StopB);
                    } break;
                default:
                    throw new Exception();
            }
        }

        private void LIST_Size_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsUpdating) return;

            Int32 Size = (Int32)Convert.ToInt32((String)LIST_Size.SelectedItem);

            RichEdit.FontSize = Size;
        }

        private void List_Family_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsUpdating) return;

            RichEdit.FontFamily = new FontFamily((String)List_Family.SelectedItem);
        }

        private void LIST_Color_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsUpdating) return;

            if (LIST_Color.SelectedItem != null)
            {
                Color Color;

                switch ((String)LIST_Color.SelectedItem)
                {
                    case "Transparent": Color = Colors.Transparent; break;
                    case "White": Color = Colors.White; break;
                    case "Yellow": Color = Colors.Yellow; break;
                    case "Orange": Color = Colors.Orange; break;
                    case "Red": Color = Colors.Red; break;
                    case "Green": Color = Colors.Green; break;
                    case "Magenta": Color = Colors.Magenta; break;
                    case "Blue": Color = Colors.Blue; break;
                    case "Brown": Color = Colors.Brown; break;
                    case "Gray": Color = Colors.Gray; break;
                    case "Black": Color = Colors.Black; break;

                    default:
                        throw new Exception();
                }

                if (RBN_CurFore.IsChecked.Value) RichEdit.CursorColor = Color;
                if (RBN_SelFore.IsChecked.Value) RichEdit.SelectionForeground = Color;
                if (RBN_SelBack.IsChecked.Value) RichEdit.SelectionBackground = Color;
                if (RBN_TextFore.IsChecked.Value) RichEdit.FontForeground = Color;
                if (RBN_TextBack.IsChecked.Value) RichEdit.FontBackground = Color;
            }
        }

        private void RBN_ColorSelection_Click(object sender, RoutedEventArgs e)
        {
            Color Color;

            if (RBN_CurFore.IsChecked.Value) Color = RichEdit.CursorColor;
            else if (RBN_SelFore.IsChecked.Value) Color = RichEdit.SelectionForeground;
            else if (RBN_SelBack.IsChecked.Value) Color = RichEdit.SelectionBackground;
            else if (RBN_TextFore.IsChecked.Value) Color = RichEdit.FontForeground;
            else if (RBN_TextBack.IsChecked.Value) Color = RichEdit.FontBackground;
            else
                throw new Exception();

            if (Colors.Transparent == Color) LIST_Color.SelectedIndex = 0;
            if (Colors.White == Color) LIST_Color.SelectedIndex = 1;
            if (Colors.Yellow == Color) LIST_Color.SelectedIndex = 2;
            if (Colors.Orange == Color) LIST_Color.SelectedIndex = 3;
            if (Colors.Red == Color) LIST_Color.SelectedIndex = 4;
            if (Colors.Green == Color) LIST_Color.SelectedIndex = 5;
            if (Colors.Magenta == Color) LIST_Color.SelectedIndex = 6;
            if (Colors.Blue == Color) LIST_Color.SelectedIndex = 7;
            if (Colors.Brown == Color) LIST_Color.SelectedIndex = 8;
            if (Colors.Gray == Color) LIST_Color.SelectedIndex = 9;
            if (Colors.Black == Color) LIST_Color.SelectedIndex = 10;
        }

        private void BTN_Replace_Click(object sender, RoutedEventArgs e)
        {
            if ((REGEX_Matches == null) || (REGEX_Matches.Count == 0))
                return;

            if (REGEX_Matches.Count <= REGEX_Index)
                REGEX_Index = 1;

            // replace selection
            Match m = REGEX_Matches[REGEX_Index - 1];
            Int32 iStart = m.Index;
            Int32 iLen = m.Length;

            REGEX_Snapshot.Remove(ref iStart, ref iLen);
            REGEX_Snapshot.Select(CursorPosition.Start, iStart, 0);
            REGEX_Snapshot.InsertString(EDIT_Replace.Text);
            REGEX_Snapshot.Select(CursorPosition.Start,
                REGEX_Snapshot.SelectionStart - EDIT_Replace.Text.Length, EDIT_Replace.Text.Length);
        }

        private void BTN_Deserialize_Click(object sender, RoutedEventArgs e)
        {
            MemoryStream Buffer = new MemoryStream(Convert.FromBase64String(LABEL_Binary.Text));

            RichEdit.InsertDeserialization(false, Buffer);
        }

        private void BTN_Close_Click(object sender, RoutedEventArgs e)
        {
            RichEdit.InsertCtrlCode(ControlCode.Unindent);
        }

        private void BTN_Serialize_Click(object sender, RoutedEventArgs e)
        {
            MemoryStream Buffer = new MemoryStream();
            RichTextEdit.Snapshot Snapshot = RichEdit.QuerySelectionText();

            Snapshot.Serialize(false, Buffer);

            // convert to base64
            LABEL_Binary.Text = Convert.ToBase64String(Buffer.GetBuffer(), 0, (int)Buffer.Length);
            BTN_Deserialize.IsEnabled = true;
        }

        private MatchCollection REGEX_Matches;
        private Int32 REGEX_Index = 0;
        private RichTextEdit.Snapshot REGEX_Snapshot;

        private void BTN_Find_Click(object sender, RoutedEventArgs e)
        {
            REGEX_Snapshot = RichEdit.QueryText();

            Regex Exp = new Regex(EDIT_Find.Text, RegexOptions.IgnoreCase |
                RegexOptions.Multiline | RegexOptions.ECMAScript);

            REGEX_Matches = Exp.Matches(REGEX_Snapshot.Text);
            REGEX_Index = 0;

            BTN_Replace.IsEnabled = true;
            BTN_FindNext.IsEnabled = true;
            BTN_FindNext_Click(null, null);
        }

        private void BTN_FindNext_Click(object sender, RoutedEventArgs e)
        {
            if ((REGEX_Matches == null) || (REGEX_Matches.Count == 0))
            {
                BTN_FindNext.IsEnabled = false;
                return;
            }

            if (REGEX_Matches.Count <= REGEX_Index)
                REGEX_Index = 0;

            // select match
            Match m = REGEX_Matches[REGEX_Index++];

            REGEX_Snapshot.Select(CursorPosition.End, m.Index, m.Length); 
        }

        private void BTN_Item_Click(object sender, RoutedEventArgs e)
        {
            RichEdit.InsertCtrlCode(ControlCode.Item);
        }

        private void BTN_Enter_Click(object sender, RoutedEventArgs e)
        {
            RichEdit.InsertCtrlCode(ControlCode.Enter);
        }

        private void BTN_Indent_Click(object sender, RoutedEventArgs e)
        {
            RichEdit.InsertCtrlCode(ControlCode.Indent);
        }

        private void CHK_Attributes_Click(object sender, RoutedEventArgs e)
        {
            if (IsUpdating) return;

            RichEdit.TextAttributes = TextAttributes.None;

            RichEdit.TextAttributes = RichEdit.TextAttributes | (!CHK_Sup.IsChecked.Value? TextAttributes.None : TextAttributes.Sup);
            RichEdit.TextAttributes = RichEdit.TextAttributes | (!CHK_Sub.IsChecked.Value ? TextAttributes.None : TextAttributes.Sub);
            RichEdit.TextAttributes = RichEdit.TextAttributes | (!CHK_Underlined.IsChecked.Value ? TextAttributes.None : TextAttributes.Underlined);
        }

        private void CHK_Italic_Click(object sender, RoutedEventArgs e)
        {
            if (IsUpdating) return;

            RichEdit.FontStyle = CHK_Italic.IsChecked.Value ? FontStyles.Italic : FontStyles.Normal;
        }

        private void LIST_Stretch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsUpdating) return;

            switch ((String)LIST_Stretch.SelectedItem)
            {
                case "Normal": RichEdit.FontStretch = FontStretches.Normal; break;
                case "Condensed": RichEdit.FontStretch = FontStretches.Condensed; break;
                case "Expanded": RichEdit.FontStretch = FontStretches.Expanded; break;
                case "ExtraCondensed": RichEdit.FontStretch = FontStretches.ExtraCondensed; break;
                case "ExtraExpanded": RichEdit.FontStretch = FontStretches.ExtraExpanded; break;
                default:
                    throw new Exception();
            }
        }

        private void BTN_Paste_Click(object sender, RoutedEventArgs e)
        {
            RichEdit.ClipboardPaste();
        }

        private void BTN_Copy_Click(object sender, RoutedEventArgs e)
        {
            RichEdit.ClipboardCopy();
        }

        private void BTN_Insert_TextBox_Click(object sender, RoutedEventArgs e)
        {
            TextBox Element = new TextBox();

            Element.Width = 100;
            Element.Height = 25;

            RichEdit.InsertObject(true, Element);
        }

        private void BTN_Insert_Button_Click(object sender, RoutedEventArgs e)
        {
            Button Element = new Button();

            Element.Width = 100;
            Element.Height = 25;

            RichEdit.InsertObject(true, Element);
        }

        private void BTN_Insert_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox Element = new CheckBox();

            Element.Width = 100;
            Element.Height = 25;

            RichEdit.InsertObject(true, Element);
        }

        private void BTN_Insert_Link_Click(object sender, RoutedEventArgs e)
        {
            TextBlock Element = new TextBlock();

            Element.Width = 100;
            Element.Height = 25;
            Element.Text = "This is a link";
            Element.Foreground = new SolidColorBrush(Colors.Blue);
            Element.MouseEnter += new MouseEventHandler(Element_MouseEnter);
            Element.MouseLeave += new MouseEventHandler(Element_MouseLeave);
            Element.MouseLeftButtonUp += new MouseButtonEventHandler(Element_MouseLeftButtonUp);

            RichEdit.InsertObject(true, Element);
        }

        void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HtmlPage.Window.Eval("window.open('http://www.google.de', '_blank', '');");
        }

        void Element_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock Element = (TextBlock)sender;

            Element.Foreground = new SolidColorBrush(Colors.Blue);
            Element.TextDecorations = null; 
        }

        void Element_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock Element = (TextBlock)sender;

            Element.Cursor = Cursors.Hand;
            Element.Foreground = new SolidColorBrush(Colors.Blue);
            Element.TextDecorations = TextDecorations.Underline; 
        }

        private void CHK_ReadOnly_Click(object sender, RoutedEventArgs e)
        {
            RichEdit.IsReadOnly = CHK_ReadOnly.IsChecked.Value;
        }

class SerializableButton : UserControl, IRichTextObject
{
    private String m_Caption;
    private Button m_Instance;

    private SerializableButton() : base() { }

    public SerializableButton(String InCaption)
    {
        m_Caption = InCaption;
    }

    public Int16 GetTypeID()
    {
        return 0x100;
    }
    public Boolean IsFocusable
    {
        get
        {
            return true;
        }
    }

    public void Serialize(FrameworkElement InElement, BinaryWriter InTarget)
    {
        SerializableButton Button = (SerializableButton)InElement;

        InTarget.Write((String)Button.m_Instance.Content);
    }

    public FrameworkElement Deserialize(
        Boolean InIgnoreWarnings,
        BinaryReader InSource)
    {
        return CreateInstance(InSource.ReadString());
    }

    public FrameworkElement CreateInstance()
    {
        return CreateInstance(m_Caption);
    }

    private FrameworkElement CreateInstance(String InCaption)
    {
        if (InCaption == null)
            throw new InvalidOperationException();

        SerializableButton Result = new SerializableButton();

        Result.m_Instance = new Button();
        Result.m_Instance.Content = InCaption;
        Result.Content = Result.m_Instance;
        Result.Width = 100;
        Result.Height = 25;

        return Result;
    }
}

        private void BTN_Insert_RTO_Click(object sender, RoutedEventArgs e)
        {
            RichEdit.InsertObject(new SerializableButton("MyButton"));
        }
    }
}
