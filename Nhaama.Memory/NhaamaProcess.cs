using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public byte ReadByte(UIntPtr offset)
        {
            return ReadBytes(offset, 1)[0];
        }

        /// <summary>
        /// Read a byte array from the specified offset.
        /// </summary>
        /// <param name="offset">Offset to read from.</param>
        /// <param name="length">Length to read.</param>
        /// <returns></returns>
        public byte[] ReadBytes(UIntPtr offset, uint length)
        {
            var bytes = new byte[length];
            Kernel32.ReadProcessMemory(BaseProcess.Handle,
                offset, bytes, new UIntPtr(length), IntPtr.Zero);

            return bytes;
        }

        public UInt64 ReadUInt64(UIntPtr offset) => BitConverter.ToUInt64(ReadBytes(offset, 8), 0);

        #endregion

        #region Writers

        /// <summary>
        /// Write a value to the specified offset, determined by type.
        /// </summary>
        /// <param name="offset">Offset to write to.</param>
        /// <param name="data">Value to write.</param>
        /// <exception cref="ArgumentException">Gets thrown, when the type to write is unsupported.</exception>
        public void Write(UIntPtr offset, object data)
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
        /// Write a string in UTF-8 Format to the specified offset.
        /// </summary>
        /// <param name="offset">Offset to write to.</param>
        /// <param name="data">Value to write.</param>
        public void WriteStringUTF8(UIntPtr offset, string data)
        {
            WriteBytes(offset, Encoding.UTF8.GetBytes(data));
        }
        
        /// <summary>
        /// Write a string in ASCII Format to the specified offset.
        /// </summary>
        /// <param name="offset">Offset to write to.</param>
        /// <param name="data">Value to write.</param>
        public void WriteStringAscii(UIntPtr offset, string data)
        {
            WriteBytes(offset, Encoding.ASCII.GetBytes(data));
        }
        
        /// <summary>
        /// Write a string in Unicode Format to the specified offset.
        /// </summary>
        /// <param name="offset">Offset to write to.</param>
        /// <param name="data">Value to write.</param>
        public void WriteStringUnicode(UIntPtr offset, string data)
        {
            WriteBytes(offset, Encoding.Unicode.GetBytes(data));
        }

        /// <summary>
        /// Write a byte array to the specified offset.
        /// </summary>
        /// <param name="offset">Offset to write to.</param>
        /// <param name="data">Value to write.</param>
        public void WriteBytes(UIntPtr offset, byte[] data)
        {
            Kernel32.WriteProcessMemory(BaseProcess.Handle, offset, data, new UIntPtr((uint) data.Length), IntPtr.Zero);
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

        #endregion
    }
}