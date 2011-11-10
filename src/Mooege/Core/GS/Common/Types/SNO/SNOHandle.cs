/*
 * Copyright (C) 2011 mooege project
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System.Text;
using CrystalMpq;
using Gibbed.IO;
using Mooege.Common.MPQ;
using Mooege.Net.GS.Message;
using System;

namespace Mooege.Core.GS.Common.Types.SNO
{
    public class InvalidSnoException : Exception { public InvalidSnoException(string message):base(message) {} }

    public class SNOHandle
    {
        /// <summary>
        /// Gets the group of the referenced object or null if the handle was
        /// initialized without group and the id is not found in mpq storage
        /// </summary>
        public SNOGroup? Group
        {
            get
            {
                if (!_isInitialized) Initialize();
                return _group.Value;
            }
        }

        /// <summary>
        /// The id of the referenced object
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The target object this handle refers to.
        /// </summary>
        public FileFormat Target
        {
            get
            {
                if (!_isInitialized) Initialize();
                return _target;
            }
        }

        /// <summary>
        /// Gets whether the handle is valid
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (!_isInitialized) Initialize();
                return _isValid;
            }
        }

        /// <summary>
        /// Get the name of the asset containing the target object
        /// </summary>
        public string Name
        {
            get
            {
                if (!_isInitialized) Initialize();
                return _name;              
            }
        }

        private string _name = "";
        private FileFormat _target = null;      
        private SNOGroup? _group = null;
        private bool _isValid = false;
        private bool _isInitialized = false;


        public SNOHandle() { }

        public SNOHandle(SNOGroup group, int id)
        {
            _group = group;
            Id = id;
        }

        public SNOHandle(int snoId)
        {
            Id = snoId;
        }


        /// <summary>
        /// Lazy initialization of handle fields. They are initialized on the first access.
        /// because the mpq storage may not have finished loading and thus, the referenced sno may not be found
        /// </summary>
        private void Initialize()
        {
            // Look up the group if it is not set. Maybe one big
            // asset dictionary would be more convenient here
            _isInitialized = true;
            if (!_group.HasValue)
            {
                foreach (var pair in MPQStorage.Data.Assets)
                    if (pair.Value.ContainsKey(Id))
                    {
                        _group = pair.Key;
                        break;
                    }
            }

            if (_group.HasValue)
                if (MPQStorage.Data.Assets.ContainsKey(_group.Value))
                    if (MPQStorage.Data.Assets[_group.Value].ContainsKey(Id))
                    {
                        _isValid = true;
                        _target = MPQStorage.Data.Assets[_group.Value][Id].Data;
                        _name = MPQStorage.Data.Assets[_group.Value][Id].Name;
                    }
        }


        /// <summary>
        /// Reads SNOName from given MPQFileStream.
        /// </summary>
        /// <param name="stream">The MPQFileStream to read from.</param>
        public SNOHandle(MpqFileStream stream)
        {
            _group = (SNOGroup)stream.ReadValueS32();
            Id = stream.ReadValueS32();
        }

        /// <summary>
        /// Parses SNOName from given GameBitBuffer.
        /// </summary>
        /// <param name="buffer">The GameBitBuffer to parse from.</param>
        public void Parse(GameBitBuffer buffer)
        {
            _group = (SNOGroup)buffer.ReadInt(32);
            Id = buffer.ReadInt(32);
        }

        /// <summary>
        /// Encodes SNOName to given GameBitBuffer.
        /// </summary>
        /// <param name="buffer">The GameBitBuffer to write.</param>
        public void Encode(GameBitBuffer buffer)
        {
            buffer.WriteInt(32, (int)Group);
            buffer.WriteInt(32, Id);
        }

        public void AsText(StringBuilder b, int pad)
        {
            b.Append(' ', pad);
            b.AppendLine("SNOHandle:");
            b.Append(' ', pad++);
            b.AppendLine("{");
            b.Append(' ', pad);
            b.AppendLine("Group: 0x" + ((int)Group).ToString("X8"));
            b.Append(' ', pad);
            b.AppendLine("Id: 0x" + Id.ToString("X8"));
            b.Append(' ', --pad);
            b.AppendLine("}");
        }

        public override string ToString()
        {
            if(IsValid)
                return string.Format("[{0}] {1} - {2}", this.Group, this.Id, this.Name);
            else
                return string.Format("[{0}] {1} - Invalid handle", _group, this.Id);
        }
    }
}
