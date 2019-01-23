using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nhaama.Memory.Native;
using Nhaama.Memory.Serialization;

namespace Nhaama.Memory
{
    public class NhaamaProcess
    {
        public readonly Process BaseProcess;

        /// <summary>
        /// Creates a new NhaamaProcess from a process.
        /// </summary>
        /// <param name="process">The Process to wrap.</param>
        /// <exception cref="Exception">Gets thrown, when the Process is inaccessible.</exception>
        public NhaamaProcess(Process process)
        {
            this.BaseProcess = process;

            // Check if we can access the process
            if (!Environment.Is64BitProcess && Is64BitProcess())
                throw new Exception(
                    "Cannot access 64bit process from within 32bit process - please build Nhaama and your app as 64bit.");
        }

        #region Readers

        /// <summary>
        /// Read a byte from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <returns></returns>
        public byte ReadByte(ulong offset)
        {
            return ReadBytes(offset, 1)[0];
        }

        /// <summary>
        /// Read a byte array from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <param name="length">Length to read.</param>
        /// <returns>Read byte array.</returns>
        public byte[] ReadBytes(ulong offset, uint length)
        {
            var bytes = new byte[length];
            Kernel32.ReadProcessMemory(BaseProcess.Handle,
                new UIntPtr(offset), bytes, new UIntPtr(length), IntPtr.Zero);

            return bytes;
        }

        /// <summary>
        /// Read a UInt64 from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <returns>Read UInt64.</returns>
        public ulong ReadUInt64(ulong offset) => BitConverter.ToUInt64(ReadBytes(offset, 8), 0);
        
        /// <summary>
        /// Read a UInt32 from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <returns>Read UInt32.</returns>
        public uint ReadUInt32(ulong offset) => BitConverter.ToUInt32(ReadBytes(offset, 4), 0);
        
        /// <summary>
        /// Read a UInt16 from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <returns>Read UInt16.</returns>
        public ushort ReadUInt16(ulong offset) => BitConverter.ToUInt16(ReadBytes(offset, 2), 0);

		/// <summary>
		/// Read a float from the specified offset.
		/// </summary>
		/// <param name="offset">Offset to read from.</param>
		/// <returns>Read Float.</returns>
		public float ReadFloat(ulong offset) => BitConverter.ToSingle(ReadBytes(offset, 4), 0);

        /// <summary>
        /// Read a string from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <param name="encodingType">Encoding, default: UTF-8</param>
        /// <returns>Read string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Gets thrown when an unknown string encoding is provided.</exception>
        public string ReadString(ulong offset, StringEncodingType encodingType = StringEncodingType.Utf8)
        {
            var bytes = new List<byte>();

            do
            {
                bytes.Add(ReadByte(offset));
                offset++;
            } while (bytes[bytes.Count - 1] != 0x0);

            bytes = bytes.Take(bytes.Count - 1).ToList();
            
            switch (encodingType)
            {
                case StringEncodingType.ASCII:
                    return Encoding.UTF8.GetString(bytes.ToArray());
                case StringEncodingType.Unicode:
                    return Encoding.UTF8.GetString(bytes.ToArray());
                case StringEncodingType.Utf8:
                    return Encoding.UTF8.GetString(bytes.ToArray());
                default:
                    throw new ArgumentOutOfRangeException(nameof(encodingType), encodingType, null);
            }
            
        }

        #endregion

        #region Writers

        /// <summary>
        /// Write a value to the specified offset, determined by type.
        /// </summary>
        /// <param name="offset">Offset to write to.</param>
        /// <param name="data">Value to write.</param>
        /// <exception cref="ArgumentException">Gets thrown, when the type to write is unsupported.</exception>
        public void Write(ulong offset, object data)
        {
            var @writeMethods = new Dictionary<Type, Action>
            {
                {typeof(byte[]), () => WriteBytes(offset, (byte[]) data)},
                {typeof(byte), () => WriteBytes(offset, new byte[] {(byte) data})},
                
                {typeof(char), () => WriteBytes(offset, new byte[] {(byte) data})},
                {typeof(short), () => WriteBytes(offset, BitConverter.GetBytes((short) data))},
                {typeof(ushort), () => WriteBytes(offset, BitConverter.GetBytes((ushort) data))},
                {typeof(int), () => WriteBytes(offset, BitConverter.GetBytes((int) data))},
                {typeof(uint), () => WriteBytes(offset, BitConverter.GetBytes((uint) data))},
                {typeof(long), () => WriteBytes(offset, BitConverter.GetBytes((long) data))},
                {typeof(ulong), () => WriteBytes(offset, BitConverter.GetBytes((ulong) data))},
                {typeof(float), () => WriteBytes(offset, BitConverter.GetBytes((float) data))},
                {typeof(double), () => WriteBytes(offset, BitConverter.GetBytes((double) data))},
            };

            if (@writeMethods.ContainsKey(data.GetType()))
                @writeMethods[data.GetType()]();
            else
                throw new ArgumentException("Unsupported type.");
        }

        /// <summary>
        /// Write a string to the specified offset.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="data"></param>
        /// <param name="encodingType"></param>
        /// <param name="zeroTerminated"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void WriteString(ulong offset, string data, StringEncodingType encodingType = StringEncodingType.Utf8,
            bool zeroTerminated = true)
        {
            if (zeroTerminated)
                data += "\0";
            
            byte[] stringBytes;
            switch (encodingType)
            {
                case StringEncodingType.ASCII:
                    stringBytes = Encoding.ASCII.GetBytes(data);
                    break;
                case StringEncodingType.Unicode:
                    stringBytes = Encoding.Unicode.GetBytes(data);
                    break;
                case StringEncodingType.Utf8:
                    stringBytes = Encoding.UTF8.GetBytes(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(encodingType), encodingType, null);
            }
            
            WriteBytes(offset, stringBytes);
        }

        /// <summary>
        /// Write a byte array to the specified offset.
        /// </summary>
        /// <param name="offset">Offset to write to.</param>
        /// <param name="data">Value to write.</param>
        public void WriteBytes(ulong offset, byte[] data)
        {
            Kernel32.WriteProcessMemory(BaseProcess.Handle, new UIntPtr(offset), data, new UIntPtr((uint) data.Length), IntPtr.Zero);
        }

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Method for checking if the Process is running as 64bit.
        /// </summary>
        /// <returns>Returns true, if the Process is running as 64bit.</returns>
        public bool Is64BitProcess()
        {
            return Environment.Is64BitOperatingSystem && Kernel32.IsWow64Process(BaseProcess.Handle, out var ret) &&
                   !ret;
        }

        /// <summary>
        /// Get a JSON Serializer for this NhaamaProcess.
        /// </summary>
        /// <returns>Serializer.</returns>
        public NhaamaSerializer GetSerializer() => new NhaamaSerializer(this);

        public ulong GetModuleBasedOffset(string moduleName, ulong offset) =>
            (ulong)BaseProcess.Modules.Cast<ProcessModule>().First(x => x.ModuleName == moduleName).BaseAddress.ToInt64() + offset;

        #endregion
    }
}