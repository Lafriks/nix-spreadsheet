﻿/*
 * Library for generating spreadsheet documents.
 * Copyright (C) 2008, Lauris Bukšis-Haberkorns <lauris@nix.lv>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace Nix.SpreadSheet
{
	/// <summary>
	/// Spreadsheet document.
	/// </summary>
	public class SpreadSheetDocument : IEnumerable
	{
		#region Sheets
		private Dictionary<string, Sheet> sheets = new Dictionary<string,Sheet>();

		/// <summary>
		/// Add new sheet to document.
		/// </summary>
		/// <returns>Added sheet.</returns>
		public Sheet AddSheet ()
		{
			string name = string.Empty;
			for ( int i = 1; ; i++ )
			{
				name = "Sheet" + i.ToString();
				if ( !this.sheets.ContainsKey(name) )
					break;
			}
			this.sheets.Add(name, new Sheet(this, name));
			return this.sheets[name];
		}

		/// <summary>
		/// Add new sheet to document.
		/// </summary>
		/// <param name="name">Sheet name.</param>
		/// <returns>Added sheet.</returns>
		public Sheet AddSheet (string name)
		{
			if (this.sheets.ContainsKey(name))
				throw new ArgumentException("Sheet with such name already exists.");
			this.sheets.Add(name, new Sheet(this, name));
			return this.sheets[name];
		}

		internal void ChangeSheetName ( string oldName, string newName )
		{
			if (this.sheets.ContainsKey(newName))
				throw new ArgumentException("Sheet with such name already exists.");
			Sheet s = this.sheets[oldName];
			this.sheets.Remove(oldName);
			this.sheets.Add(newName, s);
		}

		/// <summary>
		/// Gets the sheet count.
		/// </summary>
		/// <value>The sheet count.</value>
		public int SheetCount
		{
			get { return this.sheets.Count; }
		}
		#endregion

		#region Loading and saving spreadsheet documents
		/// <summary>
		/// Loads a spreadsheet document from the specified file.
		/// </summary>
		/// <param name="fileName">The file name.</param>
		/// <param name="format">The format.</param>
		/// <returns>Spreadsheet document.</returns>
		public static SpreadSheetDocument Load ( string fileName, Provider.IFileFormatProvider format )
		{
			StreamReader s = new StreamReader(fileName);
			try
			{
				return Load(s.BaseStream, format);
			}
			finally
			{
				s.Close();
			}
		}

		/// <summary>
		/// Loads a spreadsheet document from the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="format">The format.</param>
		/// <returns>Spreadsheet document.</returns>
		public static SpreadSheetDocument Load ( Stream stream, Provider.IFileFormatProvider format )
		{
			return new SpreadSheetDocument();
		}

		/// <summary>
		/// Saves spreadsheet document to the specified file.
		/// </summary>
		/// <param name="fileName">The file name.</param>
		/// <param name="format">The format.</param>
		public void Save ( string fileName, Provider.IFileFormatProvider format )
		{
			StreamWriter s = new StreamWriter(fileName);
			try
			{
				this.Save(s.BaseStream, format);
			}
			finally
			{
				s.Close();
			}
		}

		/// <summary>
		/// Saves the spreadsheet document to the stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="format">The format.</param>
		public void Save ( Stream stream, Provider.IFileFormatProvider format )
		{
			format.Save(this, stream);
		}
		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return this.sheets.Values.GetEnumerator();
		}

		#endregion

		#region Custom properties
        private MergedCellsBehaviour mergedCellsBehaviour = MergedCellsBehaviour.ThrowExceptionOnAccess;

        /// <summary>
        /// Merged cells behaviour.
        /// </summary>
        public MergedCellsBehaviour MergedCellsBehaviour
        {
            get { return mergedCellsBehaviour; }
            set { mergedCellsBehaviour = value; }
        }

		private RegionInfo locale = RegionInfo.CurrentRegion;

		/// <summary>
		/// Document locale.
		/// </summary>
		public RegionInfo Locale
		{
			get { return locale; }
			set { locale = value; }
		}
		#endregion
	}
}
