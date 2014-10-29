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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;

namespace System.Windows.Controls
{
    /// <summary><para>
    /// This shows how to create a custom replace policy for my RichTextEdit...
    /// Keep in mind that a replace policy operates on chronologically ordered
    /// input only. This means if someone is typing a macro from right-to-left or a
    /// macro "would be assembled" in conjunction with text that has been typed earlier,
    /// no replacement would be made. For example:
    /// </para><para>
    /// If someone types ":" in front of "-)", it wouldn't be replaced by an emoticon, even
    /// if it assembles to ":-)", because "-)" has been typed earlier than ":". This is to
    /// prevent situations in which for example the text "dear" is already there and someone
    /// is typing a smiley ":-)" in front of it. This would lead to ":dear" in first place.
    /// If no chronologically order is enforced, the text ":d" would now be replaced by
    /// the Rolling-On-The-Floor smiley, because it maps to ":D". And this is not what we want!
    /// </para>
    /// <para>
    /// Of course that behavior is not enforced by this extension but only by my RichTextEdit.
    /// </para>
    /// </summary>
    public class MSNEmoticons
    {
        static private ImageSource[] m_Sources = new ImageSource[]
        {
            //ResourceManager.LoadImage("EnumIndent.png", 30, 10),
            ResourceManager.LoadImage("Angry.png", 23, 23),
            ResourceManager.LoadImage("brokeheart.png", 23, 23),
            ResourceManager.LoadImage("confused.png", 23, 23),
            ResourceManager.LoadImage("Cool.png", 23, 23),
            ResourceManager.LoadImage("Crying.png", 23, 23),
            ResourceManager.LoadImage("Disappointed.png", 23, 23),
            ResourceManager.LoadImage("Embarressed.png", 23, 23),
            ResourceManager.LoadImage("Happy.png", 23, 23),
            ResourceManager.LoadImage("Hihi.png", 23, 23),
            ResourceManager.LoadImage("kiss.png", 23, 23),
            ResourceManager.LoadImage("love.png", 23, 23),
            ResourceManager.LoadImage("mad.png", 23, 23),
            ResourceManager.LoadImage("nerd.png", 23, 23),
            ResourceManager.LoadImage("Rofl.png", 23, 23),
            ResourceManager.LoadImage("rolleyes.png", 23, 23),
            ResourceManager.LoadImage("roses.png", 23, 23),
            ResourceManager.LoadImage("sad.png", 23, 23),
            ResourceManager.LoadImage("sarcastic.png", 23, 23),
            ResourceManager.LoadImage("secret.png", 23, 23),
            ResourceManager.LoadImage("sick.png", 23, 23),
            ResourceManager.LoadImage("sleepy.png", 23, 23),
            ResourceManager.LoadImage("Surprised.png", 23, 23),
            ResourceManager.LoadImage("thinking.png", 23, 23),
            ResourceManager.LoadImage("whisper.png", 23, 23),
            ResourceManager.LoadImage("wiltedroses.png", 23, 23),
            ResourceManager.LoadImage("Winking.png", 23, 23),
        };

        private class Icon : UserControl, IRichTextObject
        {
            private Int32 Index;
            private ImageSource Source 
            { get { return RepList[Index].Source; } }
            private String Macro
            { get { return RepList[Index].Macros[0]; } }

            public Int16 GetTypeID()
            {
                throw new NotImplementedException();
            }

            public Boolean IsFocusable { get { return false; } }

            public void Serialize(FrameworkElement InElement, BinaryWriter InTarget)
            {
                throw new NotImplementedException();
            }

            public FrameworkElement Deserialize(
                Boolean InIgnoreWarnings,
                BinaryReader InSource)
            {
                throw new NotImplementedException();
            }

            public FrameworkElement CreateInstance()
            {
                return Source.CreateImage();
            }

            public Icon(Int32 InIndex)
            {
                Index = InIndex;
            }
        }

        private class RepEntry
        {
            public String[] Macros;
            public ImageSource Source;
        }

        private static RepEntry[] RepList = 
        {
            new RepEntry(){ Source = m_Sources[0], Macros = new String[] {":@", ":-@", "*angry*",}},
            new RepEntry(){ Source = m_Sources[1], Macros = new String[] {"(U)", "*broken*",}},
            new RepEntry(){ Source = m_Sources[2], Macros = new String[] {":S", ":-S", "*confused*", "*verwirrt*"}},
            new RepEntry(){ Source = m_Sources[3], Macros = new String[] {"(H)", "*cool*",}},
            new RepEntry(){ Source = m_Sources[4], Macros = new String[] {":'(", ":,(", "*cry*", "*wein*", "*heul*"}},
            new RepEntry(){ Source = m_Sources[5], Macros = new String[] {":|", ":-|", "*disapp*"}},
            new RepEntry(){ Source = m_Sources[6], Macros = new String[] {":$", ":-$", "*ashamed*", "*schäm*"}},
            new RepEntry(){ Source = m_Sources[7], Macros = new String[] {":)", ":-)", "*happy*",}},
            new RepEntry(){ Source = m_Sources[8], Macros = new String[] {":P", ":-P", "*hihi*",}},
            new RepEntry(){ Source = m_Sources[9], Macros = new String[] {"(K)", "*kiss*", "*kuss*"}},
            new RepEntry(){ Source = m_Sources[10], Macros = new String[] {"(L)", "*love*", }},
            new RepEntry(){ Source = m_Sources[11], Macros = new String[] {"8o|", ";-(", ";(", "*mad*",}},
            new RepEntry(){ Source = m_Sources[12], Macros = new String[] {"8-|", "*nerd*",}},
            new RepEntry(){ Source = m_Sources[13], Macros = new String[] {":D", ":-D", "*rofl*",}},
            new RepEntry(){ Source = m_Sources[14], Macros = new String[] {"8-)", "*rolleyes*",}},
            new RepEntry(){ Source = m_Sources[15], Macros = new String[] {"(F)", "@--", "*roses*", "@>-->--"}},
            new RepEntry(){ Source = m_Sources[16], Macros = new String[] {":(", ":-(", "*sad*",}},
            new RepEntry(){ Source = m_Sources[17], Macros = new String[] {"^o)", "*sarcastic*",}},
            new RepEntry(){ Source = m_Sources[18], Macros = new String[] {":-#", "*secret*"}},
            new RepEntry(){ Source = m_Sources[19], Macros = new String[] {"+o(", "*sick*", "*kotz*"}},
            new RepEntry(){ Source = m_Sources[20], Macros = new String[] {"|-)", "*sleepy*",}},
            new RepEntry(){ Source = m_Sources[21], Macros = new String[] {":-O", "*surprised*",}},
            new RepEntry(){ Source = m_Sources[22], Macros = new String[] {"*-)", "*thinking*", "*denk*"}},
            new RepEntry(){ Source = m_Sources[23], Macros = new String[] {":-*", "*whisper*",}},
            new RepEntry(){ Source = m_Sources[24], Macros = new String[] {"(W)", "*wilted*",}},
            new RepEntry(){ Source = m_Sources[25], Macros = new String[] {";)", ";-)", "*wink*", "*zwinker*"}},
        };

        public static void Apply(RichTextEdit InEdit)
        {
            for (int i = 0; i < RepList.Length; i++)
            {
                for (int x = 0; x < RepList[i].Macros.Length; x++)
                {
                    InEdit.AddMacro(RepList[i].Macros[x], new Icon(i));
                }
            }
        }
    }
}
