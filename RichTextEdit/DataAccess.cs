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
using System.Collections;
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

    /// <summary>
    /// Exported methods and properties...
    /// </summary>
    public partial class RichTextEdit
    {
        RichTextEdit.Snapshot QueryText(
            Int32 InStart,
            Int32 InLength)
        {
            StringBuilder Builder = new StringBuilder();
            List<Int32> Indices = new List<int>(LineItems.Count);

            for (int i = InStart; i < InStart + InLength; i++)
            {
                LineItem Item = LineItems[i];

                if (Item.Macro != null)
                {
                    Builder.Append(Item.Macro);

                    for (int x = 0; x < Item.Macro.Length; x++)
                    {
                        Indices.Add(i);
                    }

                    continue;
                }

                if (Item is Letter)
                {
                    Builder.Append((Item as Letter).Value);

                    Indices.Add(i);
                }

                if (Item.IsCtrlCode)
                {
                    switch (Item.Code)
                    {
                        case ControlCode.Enter:
                        case ControlCode.Indent:
                        case ControlCode.Item:
                            {
                                Builder.Append("\r\n");

                                Indices.Add(i);
                                Indices.Add(i);
                            } break;
                    }
                }
            }

            if (Builder.Length != Indices.Count)
                throw new InvalidProgramException("Internal exception.");

            return new RichTextEdit.Snapshot(this, Builder.ToString(), Indices);
        }

        public RichTextEdit.Snapshot QuerySelectionText()
        {
            return QueryText(SelectionStart, SelectionLength);
        }

        public RichTextEdit.Snapshot QueryText()
        {
            return QueryText(0, LineItems.Count);
        }

        public void RegisterObject(IRichTextObject InType)
        {
            if (InType == null)
                throw new ArgumentNullException();

            for (int i = 0; i < m_RTOList.Count; i++)
            {
                if (m_RTOList[i].GetTypeID() == InType.GetTypeID())
                    throw new InvalidOperationException("An object with the same type ID does already exist.");
            }

            m_RTOList.Add(InType);
        }

        public void UnRegisterObject(IRichTextObject InType)
        {
            if (InType == null)
                throw new ArgumentNullException();

            for (int i = 0; i < m_RTOList.Count; i++)
            {
                if (m_RTOList[i].GetTypeID() == InType.GetTypeID())
                {
                    m_RTOList.Remove(InType);

                    return;
                }
            }

            throw new KeyNotFoundException();
        }

        private void InternalInsertDeserialization(
            Boolean InIgnoreWarnings,
            Stream InStream)
        {
            FontFamily Family = new FontFamily("Arial");
            FontStyle Style = FontStyles.Normal;
            FontStretch Stretch = FontStretches.Normal;
            FontWeight Weight = FontWeights.Normal;
            Double Size = 14;
            Color Foreground = Colors.Black;
            Color Background = Colors.Transparent;
            TextAlign Alignment = TextAlign.Block;
            TextAttributes Attributes = TextAttributes.None;
            BinaryReader Reader = new BinaryReader(InStream);

            RemoveSelection();

            // deserialize data...
            Int32 RestorePoint = SelectionStart;

            try
            {
                while (InStream.Position < InStream.Length)
                {
                    Int32 SelStart = SelectionStart;

                    switch (Reader.ReadByte())
                    {
                        case 0x01: // control code
                            {
                                ControlCode Code = (ControlCode)Reader.ReadByte();

                                InternalInsertCtrlCode(Code);

                                if(Code == ControlCode.Item)
                                    InternalSelect(CursorPosition.End, SelStart + 2, 0);
                                else
                                    InternalSelect(CursorPosition.End, SelStart + 1, 0);
                            } break;
                        case 0x02: // letter
                            {
                                // TODO: check font restrictions

                                // apply font settings
                                m_FontFamily = Family;
                                m_FontStyle = Style;
                                m_FontWeight = Weight;
                                m_FontSize = Size;
                                m_FontStretch = Stretch;
                                m_FontForeground = Foreground;
                                m_FontBackground = Background;
                                m_TextAlignment = Alignment;
                                m_TextAttributes = Attributes;

                                InternalInsertString(false, 0, new String(Reader.ReadChar(), 1));

                                InternalSelect(CursorPosition.End, SelStart + 1, 0);
                            } break;
                        case 0x03: // custom object
                            {
                                Int16 RTOIndex = Reader.ReadInt16();

                                for (int i = 0; i < m_RTOList.Count; i++)
                                {
                                    if (m_RTOList[i].GetTypeID() == RTOIndex)
                                    {
                                        InternalInsertFrameworkElement(
                                            m_RTOList[i].IsFocusable,
                                            m_RTOList[i],
                                            null,
                                            m_RTOList[i].Deserialize(InIgnoreWarnings, Reader));

                                        InternalSelect(CursorPosition.End, SelStart + 1, 0);

                                        break;
                                    }
                                }
                            } break;
                        case 0x04: // macro
                            {
                                // TODO: check font restrictions

                                // apply font settings
                                m_FontFamily = Family;
                                m_FontStyle = Style;
                                m_FontWeight = Weight;
                                m_FontSize = Size;
                                m_FontStretch = Stretch;
                                m_FontForeground = Foreground;
                                m_FontBackground = Background;
                                m_TextAlignment = Alignment;
                                m_TextAttributes = Attributes;

                                InternalInsertString(true, 0, Reader.ReadString());

                                InternalSelect(CursorPosition.End, SelStart + 1, 0);
                            } break;

                        #region Font switches

                        case 0x10: // font family
                            {
                                Family = new FontFamily(Reader.ReadString());
                            } break;

                        case 0x11: // font style
                            {
                                switch (Reader.ReadByte())
                                {
                                    case 0x01: Style = FontStyles.Italic; break;
                                    case 0x02: Style = FontStyles.Normal; break;
                                    default:
                                        if (!InIgnoreWarnings)
                                            throw new NotSupportedException("Unsupported font style.");
                                        break;
                                }
                            } break;

                        case 0x12: // font weight
                            {
                                switch (Reader.ReadByte())
                                {
                                    case 0x01: Weight = FontWeights.Black; break;
                                    case 0x02: Weight = FontWeights.Bold; break;
                                    case 0x03: Weight = FontWeights.ExtraBlack; break;
                                    case 0x04: Weight = FontWeights.ExtraBold; break;
                                    case 0x05: Weight = FontWeights.ExtraLight; break;
                                    case 0x06: Weight = FontWeights.Light; break;
                                    case 0x07: Weight = FontWeights.Medium; break;
                                    case 0x08: Weight = FontWeights.Normal; break;
                                    case 0x09: Weight = FontWeights.SemiBold; break;
                                    case 0x0A: Weight = FontWeights.Thin; break;
                                    default:
                                        if (!InIgnoreWarnings)
                                            throw new NotSupportedException("Unsupported font weight.");
                                        break;
                                }
                            } break;

                        case 0x13: // font size
                            {
                                Size = Reader.ReadDouble();
                            } break;

                        case 0x14: // font foreground
                            {
                                Int32 c = Reader.ReadInt32();
                                Foreground = Color.FromArgb(
                                    (Byte)((c >> 24) & 0xFF), (Byte)((c >> 16) & 0xFF), (Byte)((c >> 8) & 0xFF), (Byte)(c & 0xFF));
                            } break;

                        case 0x15: // font background
                            {
                                Int32 c = Reader.ReadInt32();
                                Background = Color.FromArgb(
                                    (Byte)((c >> 24) & 0xFF), (Byte)((c >> 16) & 0xFF), (Byte)((c >> 8) & 0xFF), (Byte)(c & 0xFF));
                            } break;

                        case 0x16: // alignment
                            {
                                Int32 Align = Reader.ReadByte();

                                switch (Align)
                                {
                                    case (Int32)TextAlign.Left: Alignment = TextAlign.Left; break;
                                    case (Int32)TextAlign.Center: Alignment = TextAlign.Center; break;
                                    case (Int32)TextAlign.Block: Alignment = TextAlign.Block; break;
                                    default:
                                        if (!InIgnoreWarnings)
                                            throw new NotSupportedException("Unsupported text alignment.");
                                        break;
                                }
                            } break;

                        case 0x17: // attributes
                            {
                                TextAttributes Attr = (TextAttributes)Reader.ReadByte();

                                Attributes = 0;

                                if ((Attr & TextAttributes.Underlined) == TextAttributes.Underlined)
                                    Attributes |= TextAttributes.Underlined;
                                if ((Attr & TextAttributes.StrikeThrough) == TextAttributes.StrikeThrough)
                                    Attributes |= TextAttributes.StrikeThrough;
                                if ((Attr & TextAttributes.Sub) == TextAttributes.Sub)
                                    Attributes |= TextAttributes.Sub;
                                if ((Attr & TextAttributes.Sup) == TextAttributes.Sup)
                                    Attributes |= TextAttributes.Sup;

                                if (((Attr & ~TextAttributes.Mask) != 0) && !InIgnoreWarnings)
                                    throw new NotSupportedException("Unsupported text attributes.");

                            } break;

                        case 0x18: // font stretch
                            {
                                switch (Reader.ReadByte())
                                {
                                    case 0x01: Stretch = FontStretches.Condensed; break;
                                    case 0x02: Stretch = FontStretches.Expanded; break;
                                    case 0x03: Stretch = FontStretches.ExtraCondensed; break;
                                    case 0x04: Stretch = FontStretches.ExtraExpanded; break;
                                    case 0x05: Stretch = FontStretches.Normal; break;
                                    case 0x06: Stretch = FontStretches.SemiCondensed; break;
                                    case 0x07: Stretch = FontStretches.SemiExpanded; break;
                                    case 0x08: Stretch = FontStretches.UltraCondensed; break;
                                    case 0x09: Stretch = FontStretches.UltraExpanded; break;
                                    default:
                                        if (!InIgnoreWarnings)
                                            throw new NotSupportedException("Unsupported font stretch.");
                                        break;
                                }
                            } break;

                        #endregion

                        default:
                            throw new ArgumentException("The given stream contains invalid serialization information.");
                    }
                }
            }
            catch (Exception e)
            {
                // remove items...
                RemoveRange(RestorePoint, SelectionStart - RestorePoint);

                InternalSelect(CursorPosition.Start, RestorePoint, 0);

                throw e;
            }


            InternalSelect(CursorPosition.Start, SelectionStart, 0);
        }

        public void InsertDeserialization(
            Boolean InIgnoreWarnings,
            Stream InStream)
        {
            InternalInsertDeserialization(InIgnoreWarnings, InStream);

            Update();

            if (OnContentChanged != null)
                OnContentChanged(this);

            
        }

        public partial class Snapshot :
            IComparable,
            IComparable<String>,
            IComparable<Snapshot>,
            IEquatable<String>
        {
            private Int64 Timestamp = DateTime.Now.Ticks;
            private List<Int32> Indices = new List<int>();

            public RichTextEdit Parent { get; private set; }
            public String Text { get; private set; }

            int IComparable<Snapshot>.CompareTo(Snapshot InComperand)
            {
                return this.CompareTo(InComperand);
            }

            public int CompareTo(Snapshot InComperand)
            {
                int Res = Text.CompareTo(InComperand.Text);

                if (Res != 0)
                    return Res;

                for (int i = 0; i < InComperand.Indices.Count; i++)
                {
                    if (InComperand.Indices[i] < Indices[i])
                        return -1;

                    if (InComperand.Indices[i] > Indices[i])
                        return 1;
                }

                return 0;
            }

            internal Snapshot(
                RichTextEdit InParent,
                String InText,
                List<Int32> InIndices)
            {
                Text = InText;
                Parent = InParent;
                Indices = new List<int>(InIndices);
            }

            public void Remove(
                ref Int32 InIndex,
                ref Int32 InLength)
            {
                Int32 OldIndex = Indices[InIndex];

                for (int i = InIndex - 1; i >= 0; i--)
                {
                    if (OldIndex == Indices[i])
                    {
                        InLength++;
                        InIndex--;
                    }
                    else
                        break;
                }

                OldIndex = Indices[InIndex + InLength];

                for (int i = InIndex + InLength + 1; i < Indices.Count; i++)
                {
                    if (OldIndex == Indices[i])
                    {
                        InLength++;
                    }
                    else
                        break;
                }

                // remove items
                Parent.RemoveRange(Indices[InIndex], Indices[InIndex + InLength] - Indices[InIndex]);
                Text = Text.Remove(InIndex, InLength);

                if (Parent.OnContentChanged != null)
                    Parent.OnContentChanged(Parent);

                Timestamp = DateTime.Now.Ticks;
            }

            public void InsertObject(IRichTextObject InObject)
            {
                Parent.ForceValidSnapshot(Timestamp);

                // is in snapshot?
                Int32 SelStart = Parent.SelectionStart;

                if ((SelStart < Indices[0]) || (SelStart > Indices[Indices.Count - 1]))
                    throw new ArgumentOutOfRangeException("Current selection does not (or only partially) refer to snapshot content.");

                if (SelectionLength != 0)
                    throw new InvalidOperationException("For snapshots it's not supported to implicitly overwrite selection. Please remove selection explicitly!");

                // remove selection
                Parent.InternalInsertFrameworkElement(InObject.IsFocusable, InObject, null, InObject.CreateInstance());

                // update snapshot...
                Snapshot s = Parent.QueryText(Indices[0], Indices[Indices.Count - 1] - Indices[0] + 1);

                this.Timestamp = s.Timestamp;
                this.Indices = s.Indices;
                this.Text = s.Text;

                Parent.Select(CursorPosition.End, SelStart + 1, 0);

            }

            public void InsertObject(
                Boolean InFocusable,
                FrameworkElement InElement)
            {
                Parent.ForceValidSnapshot(Timestamp);

                // is in snapshot?
                Int32 SelStart = Parent.SelectionStart;

                if ((SelStart < Indices[0]) || (SelStart > Indices[Indices.Count - 1]))
                    throw new ArgumentOutOfRangeException("Current selection does not (or only partially) refer to snapshot content.");

                if (SelectionLength != 0)
                    throw new InvalidOperationException("For snapshots it's not supported to implicitly overwrite selection. Please remove selection explicitly!");

                // remove selection
                Parent.InternalInsertFrameworkElement(InFocusable, null, null, InElement);

                // update snapshot...
                Snapshot s = Parent.QueryText(Indices[0], Indices[Indices.Count - 1] - Indices[0] + 1);

                this.Timestamp = s.Timestamp;
                this.Indices = s.Indices;
                this.Text = s.Text;

                Parent.Select(CursorPosition.End, SelStart + 1, 0);
            }

            public Int32 SelectionStart
            {
                get
                {
                    Int32 Result = Parent.SelectionStart;

                    for (int i = 0; i < Indices.Count - 1; i++)
                    {
                        if ((Result >= Indices[i]) && (Result < Indices[i + 1]))
                            return i;

                        if ((Indices[i] == Indices[i + 1]) && (Indices[i] == Result))
                            return i;
                    }

                    if ((Result < Indices[0]) || (Result > Indices[Indices.Count - 1]))
                        throw new ArgumentOutOfRangeException();

                    return Indices.Count - 1;
                }
            }
            public Int32 SelectionLength
            {
                get { return Parent.SelectionLength; }
            }

            public void InsertString(String InText)
            {
                Parent.ForceValidSnapshot(Timestamp);

                // is in snapshot?
                Int32 SelStart = Parent.SelectionStart;

                if ((SelStart < Indices[0]) || (SelStart > Indices[Indices.Count - 1]))
                    throw new ArgumentOutOfRangeException("Current selection does not (or only partially) refer to snapshot content.");

                if (SelectionLength != 0)
                    throw new InvalidOperationException("For snapshots it's not supported to implicitly overwrite selection. Please remove selection explicitly!");

                // remove selection
                Int32 FrameEnd = Parent.InternalInsertString(true, 0, InText);

                // update snapshot...
                Snapshot s = Parent.QueryText(Indices[0], Indices[Indices.Count - 1] - Indices[0] + (FrameEnd - SelStart));

                this.Timestamp = s.Timestamp;
                this.Indices = s.Indices;
                this.Text = s.Text;

                Parent.Select(CursorPosition.End, FrameEnd, 0);

                Parent.Update();

            }

            public void Select(
                CursorPosition InCursorPos,
                Int32 InIndex,
                Int32 InLength)
            {
                Parent.ForceValidSnapshot(Timestamp);

                Parent.Select(InCursorPos, Indices[InIndex], Indices[InIndex + InLength] - Indices[InIndex]);
            }

            public void MoveCursor(Int32 InPosition)
            {
                Parent.ForceValidSnapshot(Timestamp);

                Parent.MoveCursor(Indices[InPosition]);
            }

            public void Serialize(
                Boolean InIgnoreWarnings,
                Stream InStream)
            {
                Serialize(InIgnoreWarnings, 0, Text.Length, InStream);
            }

            public void Serialize(
                Boolean InIgnoreWarnings,
                Int32 InStart,
                Int32 InLength,
                Stream InStream)
            {
                Parent.ForceValidSnapshot(Timestamp);

                if (InLength == 0)
                    return;

                FontFamily Family = new FontFamily("Arial");
                FontStyle Style = FontStyles.Normal;
                FontStretch Stretch = FontStretches.Normal;
                FontWeight Weight = FontWeights.Normal;
                Double Size = 14;
                Color Foreground = Colors.Black;
                Color Background = Colors.Transparent;
                TextAlign Alignment = TextAlign.Block;
                TextAttributes Attributes = TextAttributes.None;
                Int32 StartIndex = Indices[InStart];
                Int32 Length = Indices[InStart + InLength - 1] - StartIndex + 1;
                BinaryWriter Writer = new BinaryWriter(InStream, Encoding.UTF8);
                Boolean IsFirstPass = true;

                // determine letter before selection...
                for (int i = StartIndex - 1; i >= 0; i--)
                {
                    Letter Item = Parent.LineItems[i] as Letter;

                    if (Item == null)
                        continue;

                    Family = Item.Label.FontFamily;
                    Style = Item.Label.FontStyle;
                    Weight = Item.Label.FontWeight;
                    Size = Item.FontSize;
                    Stretch = Item.Label.FontStretch;
                    Foreground = Item.Foreground;
                    Background = Item.Background;
                    Attributes = Item.TextAttributes;
                    Alignment = Item.Alignment;

                    break;
                }

                for (int i = StartIndex; i < StartIndex + Length; i++)
                {
                    LineItem Value = Parent.LineItems[i];

                    if (Value == null)
                        throw new InvalidCastException("The given item is not derived from LineItem or null.");

                    if (Value.IsCtrlCode)
                    {
                        if (i < Parent.LineItems.Count - 1)
                        {
                            if (Parent.LineItems[i + 1].Code != ControlCode.Item)
                            {
                                Writer.Write((Byte)0x01);
                                Writer.Write((Byte)Value.Code);
                            }
                        }
                        else
                        {
                            Writer.Write((Byte)0x01);
                            Writer.Write((Byte)Value.Code);
                        }
                    }
                    else if (Value.Macro != null)
                    {
                        if (!Foreground.Equals(Value.Foreground) || IsFirstPass)
                        {
                            Foreground = Value.Foreground;

                            Writer.Write((Byte)0x14);
                            Writer.Write((((Int32)Foreground.A) << 24) | (((Int32)Foreground.R) << 16) |
                                (((Int32)Foreground.G) << 8) | (((Int32)Foreground.B) << 0));
                        }

                        if (!Background.Equals(Value.Background) || IsFirstPass)
                        {
                            Background = Value.Background;

                            Writer.Write((Byte)0x15);
                            Writer.Write((((Int32)Background.A) << 24) | (((Int32)Background.R) << 16) |
                                (((Int32)Background.G) << 8) | (((Int32)Background.B) << 0));
                        }

                        if ((Attributes != Value.TextAttributes) || IsFirstPass)
                        {
                            Attributes = Value.TextAttributes;

                            Writer.Write((Byte)0x17);
                            Writer.Write((Byte)Attributes);
                        }

                        // serialize macro...
                        Writer.Write((Byte)0x04);
                        Writer.Write((String)Value.Macro);
                    }
                    else if (Value is Letter)
                    {
                        Letter Letter = Value as Letter;

                        #region Font serialization...
                        // check if formatting has changed...
                        if (!Family.Equals(Letter.Label.FontFamily) || IsFirstPass)
                        {
                            Family = Letter.Label.FontFamily;

                            Writer.Write((Byte)0x10);
                            Writer.Write((String)Family.Source);
                        }

                        if (!Style.Equals(Letter.Label.FontStyle) || IsFirstPass)
                        {
                            Style = Letter.Label.FontStyle;

                            if (Style.Equals(FontStyles.Italic)) Writer.Write((Int16)0x0111);
                            else if (Style.Equals(FontStyles.Normal)) Writer.Write((Int16)0x0211);
                            else if (!InIgnoreWarnings)
                                throw new NotSupportedException("Only predefined font styles are supported!");
                        }

                        if (!Weight.Equals(Letter.Label.FontWeight) || IsFirstPass)
                        {
                            Weight = Letter.Label.FontWeight;

                            if (Weight.Equals(FontWeights.Black)) Writer.Write((Int16)0x0112);
                            else if (Weight.Equals(FontWeights.Bold)) Writer.Write((Int16)0x0212);
                            else if (Weight.Equals(FontWeights.ExtraBlack)) Writer.Write((Int16)0x0312);
                            else if (Weight.Equals(FontWeights.ExtraBold)) Writer.Write((Int16)0x0412);
                            else if (Weight.Equals(FontWeights.ExtraLight)) Writer.Write((Int16)0x0512);
                            else if (Weight.Equals(FontWeights.Light)) Writer.Write((Int16)0x0612);
                            else if (Weight.Equals(FontWeights.Medium)) Writer.Write((Int16)0x0712);
                            else if (Weight.Equals(FontWeights.Normal)) Writer.Write((Int16)0x0812);
                            else if (Weight.Equals(FontWeights.SemiBold)) Writer.Write((Int16)0x0912);
                            else if (Weight.Equals(FontWeights.Thin)) Writer.Write((Int16)0x0A12);
                            else if (!InIgnoreWarnings)
                                throw new NotSupportedException("Only predefined font weights are supported!");
                        }

                        if ((Size != Letter.FontSize) || IsFirstPass)
                        {
                            Size = Letter.FontSize;

                            Writer.Write((Byte)0x13);
                            Writer.Write((Double)Size);
                        }

                        if (!Foreground.Equals(Letter.Foreground) || IsFirstPass)
                        {
                            Foreground = Letter.Foreground;

                            Writer.Write((Byte)0x14);
                            Writer.Write((((Int32)Foreground.A) << 24) | (((Int32)Foreground.R) << 16) |
                                (((Int32)Foreground.G) << 8) | (((Int32)Foreground.B) << 0));
                        }

                        if (!Background.Equals(Letter.Background) || IsFirstPass)
                        {
                            Background = Letter.Background;

                            Writer.Write((Byte)0x15);
                            Writer.Write((((Int32)Background.A) << 24) | (((Int32)Background.R) << 16) |
                                (((Int32)Background.G) << 8) | (((Int32)Background.B) << 0));
                        }

                        if ((Alignment != Letter.Alignment)  || IsFirstPass)
                        {
                            Alignment = Letter.Alignment;

                            Writer.Write((Byte)0x16);
                            Writer.Write((Byte)Alignment);
                        }

                        if ((Attributes != Letter.TextAttributes) || IsFirstPass)
                        {
                            Attributes = Letter.TextAttributes;

                            Writer.Write((Byte)0x17);
                            Writer.Write((Byte)Attributes);
                        }

                        if (!Stretch.Equals(Letter.Label.FontStretch) || IsFirstPass)
                        {
                            Stretch = Letter.Label.FontStretch;

                            if (Stretch.Equals(FontStretches.Condensed)) Writer.Write((Int16)0x0118);
                            else if (Stretch.Equals(FontStretches.Expanded)) Writer.Write((Int16)0x0218);
                            else if (Stretch.Equals(FontStretches.ExtraCondensed)) Writer.Write((Int16)0x0318);
                            else if (Stretch.Equals(FontStretches.ExtraExpanded)) Writer.Write((Int16)0x0418);
                            else if (Stretch.Equals(FontStretches.Normal)) Writer.Write((Int16)0x0518);
                            else if (Stretch.Equals(FontStretches.SemiCondensed)) Writer.Write((Int16)0x0618);
                            else if (Stretch.Equals(FontStretches.SemiExpanded)) Writer.Write((Int16)0x0718);
                            else if (Stretch.Equals(FontStretches.UltraCondensed)) Writer.Write((Int16)0x0818);
                            else if (Stretch.Equals(FontStretches.UltraExpanded)) Writer.Write((Int16)0x0918);
                            else if (!InIgnoreWarnings)
                                throw new NotSupportedException("Only predefined font styles are supported!");
                        }

                        #endregion

                        IsFirstPass = false;

                        // serialize char
                        Writer.Write((Byte)0x02);
                        Writer.Write((Char)Letter.Value);
                    }
                    else
                    {
                        // serialize custom object...
                        if (Value.RTO != null)
                        {
                            Writer.Write((Byte)0x03);
                            Writer.Write((Int16)Value.RTO.GetTypeID());
                            Value.RTO.Serialize(Value.Item, Writer);
                        }
                    }
                }
            }

            int System.IComparable.CompareTo(object InComperand)
            {
                if (InComperand is String)
                    return Text.CompareTo(InComperand);
                else if (InComperand is Snapshot)
                    return Text.CompareTo((InComperand as Snapshot).Text);
                else
                    throw new InvalidCastException("The given comperand type is neither of String or RichTextEdit.Snapshot!");
            }
            int System.IComparable<String>.CompareTo(String InComperand)
            {
                return Text.CompareTo(InComperand);
            }
            bool System.IEquatable<String>.Equals(String InComperand)
            {
                return Text.Equals(InComperand);
            }
        }
    }
}
