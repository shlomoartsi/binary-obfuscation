using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationTransform.Transformation.Junk
{
    public class JunkBytesProvider : IJunkBytesProvider
    {
        private byte[][] m_junkBytesArray = new byte[][] {
        //the command 'and eax, 0xf2f10403' 
        //is represented by {0x81 ,0xe0 ,0x03, 0x04, 0xf1, 0xf2}   
        new byte[] { 0x81, 0xe0, 0x03, 0x04, 0xf1 },
        //c7 45 e0 03 12 a3 f7    mov    DWORD PTR [ebp-0x20],0xf7a31203
        new byte[] { 0xc7, 0x45, 0xe0, 0x03, 0x12, 0xa3 },
        //81 7c 30 fc ba 75 84    cmp    DWORD PTR [eax+esi*1-0x4],0xe28475ba
        new byte[] { 0x81, 0x7C, 0x30, 0xFC, 0xBA, 0x75 },
        //f2 0f 2c 04 24                 cvttsd2si eax, qword [esp]
        new byte[] {0xf2, 0x0f, 0x2c, 0x04 },
        //0f 84 0a 80 3c 7d       je     0x7d3c8010
        new byte[] { 0x0F, 0x84, 0x0A, 0x80, 0x3C },
        //8d 8d ec fb 3d f6       lea    ecx,[ebp - 0x9c20414]
        new byte[] { 0x8D, 0x8D, 0xEC, 0xFB, 0x3D },
        //66 8c 05 3c a4 41 00           mov [0x41a43c], es
        new byte[] {0x66, 0x8c, 0x05, 0x3c, 0xa4, 0x41} };

        private Random m_random = new Random(DateTime.UtcNow.Millisecond * 
            (DateTime.UtcNow.Minute + DateTime.UtcNow.Second + 11) / 19);
        
        public byte[] GetJunkBytes()
        {
            var index = m_random.Next(0, m_junkBytesArray.Length - 1);
            return m_junkBytesArray[index];
        }
    }
}
