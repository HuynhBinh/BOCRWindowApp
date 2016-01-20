using System;
using System.Collections.Generic;
using System.Text;
using WindowsPreview.Media.Ocr;

namespace Model
{
    class MyLine : IComparable<MyLine>
    {
        public int lineNumber;
        public List<OcrWord> listWords;

        public MyLine()
        {
            listWords = new List<OcrWord>();
        }

        public void Add(OcrWord word)
        {
            if (listWords != null)
            {
                listWords.Add(word);
            }
        }

        public int CompareTo(MyLine other)
        {
            if (this.lineNumber > other.lineNumber) return 1;
            else if (this.lineNumber < other.lineNumber) return -1;
            else return 0;
        }

        private bool Near(OcrWord w1, OcrWord w2)
        {
            int nearRange = 40;
            int a = w1.Left + w1.Width;
            int b = w2.Left;
            if (a + nearRange > b)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        override
        public string ToString()
        {
            if (listWords != null)
            {
                StringBuilder builder = new StringBuilder();

                listWords.Sort(delegate (OcrWord w1, OcrWord w2)
                {
                    return w1.Left.CompareTo(w2.Left);
                });

                //
                OcrWord preViousWord = null;

                for (int i = 0; i < listWords.Count; i++)
                {
                    OcrWord currentWord = listWords[i];

                    if (preViousWord != null)
                    {
                        if (Near(preViousWord, currentWord))
                        {
                            builder.Append(currentWord.Text).Append(" ");
                        }
                        else
                        {
                            builder.Append("[LONGSPACE]").Append(" ").Append(currentWord.Text).Append(" ");
                        }
                    }
                    else
                    {
                        builder.Append(currentWord.Text).Append(" ");
                    }

                    if (i == (listWords.Count - 1))
                    {
                        preViousWord = null;
                    }
                    else
                    {
                        preViousWord = currentWord;
                    }


                }
                //
                builder.Append("\n");

                return builder.ToString();
            }

            return "NULL";
        }
    }
}
