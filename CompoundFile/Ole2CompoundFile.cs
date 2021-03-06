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
using System.Text;
using System.Collections;

using Nix.CompoundFile.Managers;

namespace Nix.CompoundFile
{
    /// <summary>
    /// Summary description for Ole2CompoundFile.
    /// </summary>
    public sealed class Ole2CompoundFile
    {
        #region Private variables
        private Ole2Storage root = null;

        private ByteOrder byteOrder = ByteOrder.LittleEndian;

        internal DirectoryManager DirectoryManager = new DirectoryManager();

        private SectorAllocationManager SSAT;

        private SectorAllocationManager SAT;

        private MasterSectorAllocationManager MSAT;

        private EndianStream ewriter;
        private EndianStream ereader;

        private ushort sectorSizeC = 9;
        private ushort sectorSize = 512;
        private ushort shortSectorSizeC = 6;
        private ushort shortSectorSize = 64;
        private uint minStreamSize = 4096;
        #endregion

        #region Write header to stream
        private void WriteHeader(Stream output, uint SATCount, int RootStart, int SSATStart, uint SSATCount, int MSATStart, uint MSATCount)
        {
            //this.CalculateSizes();
            // File identifier
            this.ewriter.Write(new byte[]{0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1}, 0, 8);
            // UID
            this.ewriter.WriteBytes(0x00, 16);
            // Revision number an version of file format
            this.ewriter.Write(new byte[]{0x3E, 0x00}, 0, 2);
            this.ewriter.Write(new byte[]{0x03, 0x00}, 0, 2);
            // Byte order
            if (this.byteOrder == ByteOrder.LittleEndian)
                this.ewriter.Write(new byte[]{0xFE, 0xFF}, 0, 2);
            else
                this.ewriter.Write(new byte[]{0xFF, 0xFE}, 0, 2);
            // Sector size (deflaut is 9 -> 512 bytes)
            this.ewriter.WriteUInt16(this.sectorSizeC);
            // Short sector size (deflaut is 6 -> 64 bytes)
            this.ewriter.WriteUInt16(this.shortSectorSizeC);
            // Not used
            this.ewriter.WriteBytes(0x00, 10);
            // Total number of sectors used for sector allocation table
            this.ewriter.WriteUInt32(SATCount);
            // Directory stream first SID
            this.ewriter.WriteInt32(RootStart);
            // Not used
            this.ewriter.WriteBytes(0x00, 4);
            // Minimum size of standart stream
            this.ewriter.WriteUInt32(this.minStreamSize);
            // SID of short-sector allocation table
            this.ewriter.WriteInt32(SSATStart);
            // Total number of sectors of short-sector allocation table
            this.ewriter.WriteUInt32(SSATCount);
            // SID of master sector allocation table
            this.ewriter.WriteInt32(MSATStart);
            // Total number of sectors of master sector allocation table
            this.ewriter.WriteUInt32(MSATCount);
            // First part of the master sector allocation table (MSAT) - max 109
            for (int i = 0; i < Math.Min(this.MSAT.Count, 109); i++)
            {
                this.ewriter.WriteInt32(this.MSAT[i]);
            }
			// Fill left space with 0xFF
			if (this.MSAT.Count < 109)
                this.ewriter.WriteBytes(0xFF, 436 - (this.MSAT.Count * 4));
        }
        #endregion

        #region Write additional MSAT, SAT and SSAT
		private void WriteAdditionalMSAT(EndianStream writer, int MSATStart)
		{
			int start = MSATStart;
			for (int i = 109; i < this.MSAT.Count; i++)
			{
				writer.WriteInt32(this.MSAT[i]);
				if ((writer.Position % this.sectorSize) == (this.sectorSize - 4) || i == this.MSAT.Count - 1)
				{
					start = this.SAT.FindNextSpecialSector(start + 1, -4);
					writer.WriteInt32(start);
				}
			}
		}

		private void WriteSAT(EndianStream writer)
        {
            foreach (int x in this.SAT.Allocations)
            {
                writer.WriteInt32(x);
            }
        }

        private void WriteSSAT(EndianStream writer)
        {
            foreach (int x in this.SSAT.Allocations)
            {
                writer.WriteInt32(x);
            }
        }
        #endregion

        #region Write directory entries
        private void WriteDES(EndianStream writer)
        {
            foreach (Ole2DirectoryEntry entr in this.DirectoryManager)
            {
                this.WriteDirectoryEntry(writer, entr);
            }
        }

        private void WriteDirectoryEntry(EndianStream writer, Ole2DirectoryEntry node)
        {
        	int c = writer.WriteString(node.Name);
            writer.WriteBytes(0x00, 64 - c);
            writer.WriteUInt16((ushort)(c + 2));
            writer.WriteByte((byte)node.Type);
            writer.WriteByte((byte)node.Color);
            //writer.Write4((node.Parent != null && node.Parent.Left != null ? node.Parent.Left.DID : -1));
            //writer.Write4((node.Parent != null && node.Parent.Right != null ? node.Parent.Right.DID : -1));
            writer.WriteInt32((node.Left != null ? node.Left.DID : -1));
            writer.WriteInt32((node.Right != null ? node.Right.DID : -1));
            writer.WriteInt32((node is Ole2Storage && node.Root != null ? node.Root.DID : -1));
            // Unique ID
            writer.WriteBytes(0x00, 16);
            // User flags
            writer.WriteBytes(0x00, 4);
            // Time stamps
            writer.WriteBytes(0x00, 8);
            writer.WriteBytes(0x00, 8);
            // Sid
            writer.WriteInt32(node.Sector);
            // Size in bytes
            writer.WriteUInt32(node.Size);
            // Not used
            writer.WriteBytes(0x00, 4);
        }
        #endregion

        #region Save methods
        public EndianStream CreateEndianStream ( Stream baseStream )
        {
            if (this.byteOrder == ByteOrder.LittleEndian)
                return new LittleEndianStream(baseStream);
            else
            	return new BigEndianStream(baseStream);
        }

        public void Save(Stream output)
        {
        	this.ewriter = this.CreateEndianStream(output);

            #region Calculate sizes, allocate SAT, SSAT, MSAT, write header
            // Create sector allocation table (SAT)
            this.SAT = new SectorAllocationManager(this.sectorSize);
            // Create master sector allocation table (MSAT)
            this.MSAT = new MasterSectorAllocationManager(this.SAT);

            // Create short sector allocation table (SSAT)
            this.SSAT = new SectorAllocationManager(this.shortSectorSize);
            uint ShortStreamSize = this.AllocateShortStreams();
            uint ShortStreamSectorCount = Convert.ToUInt32(Math.Ceiling((double)ShortStreamSize / this.sectorSize));

            // Count how many sectors do we need
            //int LongStreamSize = this.CalculateLongStreamSize(true);
            uint LongStreamSectorCount = Convert.ToUInt32(Math.Ceiling((double)this.CalculateLongStreamSize() / this.sectorSize));
            uint SSATSize = (uint)this.SSAT.Allocations.GetLongLength(0) * 4;
            uint SSATSectorCount = Convert.ToUInt32(Math.Ceiling((double)SSATSize / this.sectorSize));
            uint DirectorySteamSize = this.DirectoryManager.Count * 128;
            uint DirectorySteamSectorCount = Convert.ToUInt32(Math.Ceiling((double)DirectorySteamSize / this.sectorSize));

            uint SectorCount = ShortStreamSectorCount + LongStreamSectorCount + SSATSectorCount + DirectorySteamSectorCount;

            // Calculate SAT size
            uint SATSize = (SectorCount * 4);
            //int SATLastSectorFreeBlocks = SATSize % (this.sectorSize / 4);
            uint SATSectorCount = Convert.ToUInt32(Math.Ceiling((double)SATSize / this.sectorSize));

            int MSATNextSector = -2;

            uint MSATSectorCount = 0;

            // First 109 SIDs of SAT are stored in header, check if we need more
            if (SATSectorCount > 109)
            {
				// TODO: Debug!!! Could be wrong
				// Count how much do we need
                uint sect = SATSectorCount - 109;
                uint frsect = (sect % (ushort)(this.sectorSize / 4));
                sect = Convert.ToUInt32(Math.Ceiling((double)sect / (this.sectorSize / 4)));
                // Do we need one more sector for MSAT?
                if (frsect < sect)
                    sect++;
                // Increase total sector count
                SectorCount += sect;
                // Recalculate SAT size and sector count
                SATSize += (sect * 4);
                SATSectorCount += Convert.ToUInt32(Math.Ceiling((double)SATSize / this.sectorSize));

                // MSAT sector count
                MSATSectorCount = sect;
                // Allocate MSAT
                MSATNextSector = this.SAT.Allocate(sect * this.sectorSize, -4);
            }

            // Allocate SSAT
            int SSATStart = this.SAT.Allocate(SSATSize);
            if (SSATStart == -1)
                SSATStart = -2;

            // Allocate Short stream
            int ShortStreamStart = this.SAT.Allocate(ShortStreamSize);
			if (ShortStreamStart == -1)
				ShortStreamStart = -2;

            // Allocate long streams
            this.AllocateLongStreams();

            // Allocate Directory steam
            int RootStart = this.SAT.Allocate(DirectorySteamSize);

            this.Root.Sector = ShortStreamStart;
            this.Root.SetSize(ShortStreamSize);

            // Allocate SAT (through MSAT)
            int SATStart = this.MSAT.Allocate(SATSize, -3);

            // At this point we have enaught information to write header
            this.WriteHeader(output, SATSectorCount, RootStart, SSATStart, SSATSectorCount, MSATNextSector, MSATSectorCount);
            #endregion

            #region Create and allocate streams
			// Create additional MSAT stream
			MemoryStream mmem = new MemoryStream();
			EndianStream mwriter = this.CreateEndianStream(mmem);
			this.WriteAdditionalMSAT(mwriter, MSATNextSector);
			this.SAT.AllocateStream(MSATNextSector, mmem, 0xFF);

            // Create SAT stream
            MemoryStream smem = new MemoryStream();
            EndianStream swriter = this.CreateEndianStream(smem);
            this.WriteSAT(swriter);
            this.SAT.AllocateStream(SATStart, smem, 0xFF);

            // Create directory stream (DES)
            MemoryStream mem = new MemoryStream();
            EndianStream writer = this.CreateEndianStream(mem);
            this.WriteDES(writer);
            this.SAT.AllocateStream(RootStart, mem);

            // Create SSAT stream
            MemoryStream ssmem = new MemoryStream();
            EndianStream sswriter = this.CreateEndianStream(ssmem);
            this.WriteSSAT(sswriter);
            this.SAT.AllocateStream(SSATStart, ssmem, 0xFF);

            // Create short stream
            MemoryStream shmem = new MemoryStream();
            this.SSAT.WriteData(shmem);
            this.SAT.AllocateStream(ShortStreamStart, shmem);

            // Create long stream
            this.AllocateLongStreamsData();
            #endregion

            // Write sectors to the stream
            this.SAT.WriteData(output);
			smem.Close();
			mem.Close();
			ssmem.Close();
			shmem.Close();
		}

        public void Save(string filename)
        {
            StreamWriter file = new StreamWriter(filename);
            this.Save(file.BaseStream);
            file.Close();
        }
        #endregion

        #region Load methods
        public void Load(Stream input)
        {
        	this.ereader = this.CreateEndianStream(input);
        	//this.ReadHeader();
        	// TODO: Reading OLE 2 compound file
        }

        public void Load(string filename)
        {
            StreamReader file = new StreamReader(filename);
            this.Load(file.BaseStream);
            file.Close();
        }
        #endregion

        #region Allocate streams
        private uint AllocateShortStreams()
        {
            uint sum = 0;
            foreach (Ole2DirectoryEntry e in this.DirectoryManager)
            {
                if (e is Ole2Stream)
                {
                    if (((Ole2Stream)e).Size < this.minStreamSize && ((Ole2Stream)e).Sector == -2)
                    {
                        int fsect = this.SSAT.Allocate(((Ole2Stream)e).Size);
                        ((Ole2Stream)e).Sector = fsect;
                        this.SSAT.AllocateStream(fsect, ((Ole2Stream)e).BaseStream);
						sum += (uint)Math.Ceiling((double)((Ole2Stream)e).Size / this.shortSectorSize) * this.shortSectorSize;
					}
                }
            }
            return sum;
        }

        private void AllocateLongStreams()
        {
            foreach (Ole2DirectoryEntry e in this.DirectoryManager)
            {
                if (e is Ole2Stream && ((Ole2Stream)e).Size >= this.minStreamSize && ((Ole2Stream)e).Sector == -2)
                {
                        ((Ole2Stream)e).Sector = this.SAT.Allocate(((Ole2Stream)e).Size);
                }
            }
        }

        private void AllocateLongStreamsData()
        {
            foreach (Ole2DirectoryEntry e in this.DirectoryManager)
            {
                if (e is Ole2Stream && ((Ole2Stream)e).Size >= this.minStreamSize)
                {
                    this.SAT.AllocateStream(((Ole2Stream)e).Sector, ((Ole2Stream)e).BaseStream);
                }
            }
        }
        #endregion

        #region Calculate long stream size
        private int CalculateLongStreamSize()
        {
            //sum += (real ? ((Ole2Stream)e).Size : (int)Math.Ceiling((double)((Ole2Stream)e).Size / this.sectorSize) * this.sectorSize);
            int sum = 0;
            foreach (Ole2DirectoryEntry e in this.DirectoryManager)
            {
                // Calculate in full sectors
                if (e is Ole2Stream && ((Ole2Stream)e).Size >= this.minStreamSize)
                    sum += (int)Math.Ceiling((double)((Ole2Stream)e).Size / this.sectorSize) * this.sectorSize;
            }
            return sum;
        }
        #endregion

        #region Root element
        public Ole2Storage Root
        {
            get
            {
                if (this.root == null)
                    this.root = new Ole2Storage(this);
                return this.root;
            }
        }
        #endregion
    }

    public enum ByteOrder
    {
        LittleEndian,
        BigEndian
    }
}

