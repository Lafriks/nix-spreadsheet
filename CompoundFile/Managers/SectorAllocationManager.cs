﻿/*
 * Library for writing OLE 2 Compount Document file format.
 * Copyright (C) 2007, Lauris Bukšis-Haberkorns <lauris@nix.lv>
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
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Nix.CompoundFile.Managers
{
    internal delegate void SectorEventHandler(object sender, int index);

    /// <summary>
    /// Description of SectorAllocationManager.
    /// </summary>
    internal class SectorAllocationManager
    {
        #region Private variables
        private int SectorSize = 0;
		private List<int> Sectors = new List<int>();
		private List<ISector> SectorsData = new List<ISector>();

        private bool sync = false;
        private int[] allocations;
        #endregion

        #region Events
        public event SectorEventHandler Allocation;

        public void OnAllocation (object sender, int index)
        {
            if (this.Allocation != null)
            {
                this.Allocation(sender, index);
            }
        }
        #endregion

        #region Constructor
        public SectorAllocationManager(int size)
        {
            this.SectorSize = size;
        }
        #endregion

        #region Allocate methds
        private int FindFirstFree()
        {
            return this.FindFirstFree(0);
        }

        private int FindFirstFree(int from)
        {
            for (int i = from; i < this.Sectors.Count; i++)
            {
            	if (this.Sectors[i] == -1)
                    return i;
            }
            // No free sectors found
    		// Keep in sync arrays
    		this.SectorsData.Add(null);
			this.Sectors.Add(-1);
			return this.Sectors.Count - 1;
        }

        public int Allocate(uint size)
        {
            return this.Allocate(size, -1);
        }

        public int Allocate(uint size, int val)
        {
			// If size is zero, do not allocate any sectors.
			if (size == 0)
				return -1;
			this.sync = false;
            // Allocate first record
            int first = this.FindFirstFree();
            int current = first;
            int count = Convert.ToInt32(Math.Ceiling((double)size / this.SectorSize)) - 1;
            // If we need more record alocate them
            while ( count-- > 0 )
            {
            	int next = this.FindFirstFree(current + 1);
            	this.Sectors[current] = ( val == -1 ? next : val );
            	this.OnAllocation(this, current);
            	current = next;
            }
            // Last sector
            this.Sectors[current] = ( val == -1 ? -2 : val );
            this.OnAllocation(this, current);
            return first;
        }

		public int FindNextSpecialSector(int start, int val)
		{
			for (int i = start; i < this.Sectors.Count; i++)
			{
				if (this.Sectors[i] == val)
					return i;
			}
			return -2;
		}
		#endregion

		#region Allocations
		public int[] Allocations
        {
            get
            {
                if ( ! this.sync)
                {
                    this.allocations = this.Sectors.ToArray();
                    this.sync = true;
                }
                return this.allocations;
            }
        }
        #endregion

        #region Allocate stream data
		public void AllocateStream ( int start, Stream stream )
        {
			this.AllocateStream(start, stream, 0);
        }

		public void AllocateStream ( int start, Stream stream, byte def )
        {
            int offset = 0;
            int steamSize = (int)stream.Length;
            while ((start > -1 || start == -3 || start == -4) && start < this.Sectors.Count)
            {
                this.SectorsData[start] = new SectorStream(stream, offset, this.SectorSize, def);
                offset += this.SectorSize;
                // Go to next sector
				start = (this.Sectors[start] == -3 || this.Sectors[start] == -4 ? FindNextSpecialSector(start + 1, this.Sectors[start]) : this.Sectors[start]);
            }
        }
        #endregion

        #region Write sectors to stream
        public void WriteData(Stream writer)
        {
            foreach(ISector sector in this.SectorsData)
            {
				sector.Write(writer);
            }
        }
        #endregion
    }
}
