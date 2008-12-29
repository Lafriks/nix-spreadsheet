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
using System.Collections.Generic;
using System.Text;
using Nix.CompoundFile;

namespace Nix.SpreadSheet.Provider.Excel.BIFF
{
	/// <summary>
	/// BIFF record base class.
	/// </summary>
	internal abstract class BIFFRecord
	{
		/// <summary>
		/// Record OPCODE.
		/// </summary>
		protected abstract int OPCODE
		{
			get;
		}

		#region Header
		/// <summary>
		/// Writes record header to the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="nRecLen">The record length.</param>
        protected void WriteHeader (EndianStream stream, int recordLength)
        {
			stream.Write2(this.OPCODE);
			stream.Write2(recordLength);
        }

        /// <summary>
        /// Read record header from the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The record length.</returns>
        protected int ReadHeader (EndianStream stream)
        {
        	// Read OPCODE
        	int OPCODE = stream.Read2();
        	// Check if reading correct opcode
        	if ( this.OPCODE != OPCODE )
        		throw new InvalidCastException("Invalid BIFF record");
        	// Return BIFF record length
        	return stream.Read2();
        }
        #endregion
        
        public abstract void Write(EndianStream stream);
        
        public abstract void Read(EndianStream stream);
	}
}