using System;
using System.Runtime.InteropServices;
namespace ZoneAgent
{
    //Class for encrypting and decrypting packets
    class Crypt
    {
        [DllImportAttribute("asdecr.dll", EntryPoint = "decrypt_acl", CallingConvention = CallingConvention.Cdecl)]
        public static extern int decrypt_acl(IntPtr acldata, int size, int header);

        [DllImportAttribute("asdecr.dll", EntryPoint = "encrypt_acl", CallingConvention = CallingConvention.Cdecl)]
        public static extern int encrypt_acl(IntPtr acldata, int size, int header);

        //Decrpyt() to decrypt packets
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
        //Encrypt() to encrypt packets
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
