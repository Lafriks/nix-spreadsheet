/*
 * Library for creating Microsoft Excel files.
 * This library is free software; you can redistribute it and/or
 * This library is distributed in the hope that it will be useful,
 * You should have received a copy of the GNU Lesser General Public

using System;

namespace Nix.SpreadSheet
{
    /// <summary>
    /// Comment.
    /// </summary>
    public class Comment
    {
        bool visible = false;
        string comment = string.Empty;

        /// <summary>
        /// Gets or sets comment text assigned to cell.
        /// </summary>
        public string Text
        {
            get
            {
                return this.comment;
            }
            set
            {
                this.comment = value;
            }
        }

        /// <summary>
        /// Gets or sets value indicating whether comment is visible.
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.visible;
            }
            set
            {
                this.visible = value;
            }
        }
    }
}