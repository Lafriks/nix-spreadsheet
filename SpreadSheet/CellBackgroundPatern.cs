/*
 * Library for generating spreadsheet documents.
 * Copyright (C) 2008, Lauris Buk�is-Haberkorns <lauris@nix.lv>
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

namespace Nix.SpreadSheet
{
	/// <summary>
	/// Cell background pattern.
	/// </summary>
	public enum CellBackgroundPattern
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0,
		/// <summary>
		/// Fill.
		/// </summary>
		Fill = 1,
		/// <summary>
		/// Horizontal lines.
		/// </summary>
		HorizontalLines = 5,
		/// <summary>
		/// Vertical lines.
		/// </summary>
		VerticalLines = 6
	}
}
