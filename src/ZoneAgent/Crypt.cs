using System;
using System.Runtime.InteropServices;
namespace ZoneAgent
{
    /// <summary>
    /// Class for encrypting and decrypting packets
    /// </summary>
    class Crypt
    {
        [DllImportAttribute("asdecr.dll", EntryPoint = "decrypt_acl", CallingConvention = CallingConvention.Cdecl)]
        public static extern int decrypt_acl(IntPtr acldata, int size, int header);

        [DllImportAttribute("asdecr.dll", EntryPoint = "encrypt_acl", CallingConvention = CallingConvention.Cdecl)]
        public static extern int encrypt_acl(IntPtr acldata, int size, int header);

        /// <summary>
        /// To decrypt packet data
        /// </summary>
        /// <param name="packet">data</param>
        /// <returns>returns decrypted data</returns>
        public static byte[] Decrypt(byte[] packet)
        {
            var length = packet.Length;
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(length);
            Marshal.Copy(packet, 0, unmanagedPointer, length);
            Crypt.decrypt_acl(unmanagedPointer, length, 0);
            Marshal.Copy(unmanagedPointer, packet, 0, length);
            Marshal.FreeHGlobal(unmanagedPointer);
            return packet;
        }
        /// <summary>
        /// To encrypt packet data
        /// </summary>
        /// <param name="packet">data</param>
        /// <returns>returns encrypted packet</returns>
        public static byte[] Encrypt(byte[] packet)
        {
            var length = packet.Length;
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(length);
            Marshal.Copy(packet, 0, unmanagedPointer, length);
            Crypt.encrypt_acl(unmanagedPointer, length, 0);
            Marshal.Copy(unmanagedPointer, packet, 0, length);
            Marshal.FreeHGlobal(unmanagedPointer);
            return packet;
        }
    }
}
