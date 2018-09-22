using PeNet.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeNet.Parser
{
    /// <summary>
    /// Relcoation directory serializer
    /// </summary>
    public class ImageBaseRelocationSerialiazer
    {

        public void Seriazlize(byte[] buffer, IMAGE_BASE_RELOCATION[] relocationDirectory)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (relocationDirectory == null) throw new ArgumentNullException(nameof(relocationDirectory));

        }
    }
}
