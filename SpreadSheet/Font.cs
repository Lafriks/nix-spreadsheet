/*
 * Library for creating Microsoft Excel files.
 * This library is free software; you can redistribute it and/or
 * This library is distributed in the hope that it will be useful,
 * You should have received a copy of the GNU Lesser General Public

using System;
using System.Drawing;

namespace SpreadSheet
{
    public enum UnderlineStyle
    {
        None,
        Single,
        Double,
        SingleAccounting,
        DoubleAccounting
    }

    public enum ScriptPosition
    {
        Normal,
        Superscript,
        Subscript
    }

	public class Font
	{
        public const int NormalWeight = 400;

        public const int BoldWeight = 700;

        public const int MaxWeight = 1000;

        public const int MinWeight = 100;
        
        #region Color
        private Color color = Color.Black;

        public Color Color
        {
            get
            {
                return this.color;
            }
            set
            {
                this.color = value;
            }
        }
        #endregion
        
        #region Italic, strikeout etc
        private bool italic = false;
        public bool Italic
        {
            get
            {
                return this.italic;
            }
            set
            {
                this.italic = value;
            }
        }

        private bool strikeout = false;
        public bool Strikeout
        {
            get
            {
                return this.strikeout;
            }
            set
            {
                this.strikeout = value;
            }
        }
        #endregion

        #region Weight
        private int weight = NormalWeight;
        public int Weight
        {
            get
            {
                return this.weight;
            }
            set
            {
                this.weight = value;
            }
        }
        #endregion

        #region Name
        private string name = "Arial";
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
        #endregion

        #region Size
        private int size = 200;
        public int Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
            }
        }
        #endregion

        #region Underline style
        private UnderlineStyle underline = UnderlineStyle.None;
        public UnderlineStyle UnderlineStyle
        {
            get
            {
                return this.underline;
            }
            set
            {
                this.underline = value;
            }
        }
        #endregion

        #region Script position
        private ScriptPosition scriptpos = ScriptPosition.Normal;
        public ScriptPosition ScriptPosition
        {
            get
            {
                return this.scriptpos;
            }
            set
            {
                this.scriptpos = value;
            }
        }
        #endregion
    }
}