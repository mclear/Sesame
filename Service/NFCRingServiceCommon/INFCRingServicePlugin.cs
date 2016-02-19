using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCRing.Service.Common
{
    public interface INFCRingServicePlugin
    {
        // token down - id
        void NCFRingDown(string id);
        // token up - id
        void NCFRingUp(string id);
        // data read - id, data
        void NFCRingDataRead(string id, byte[] data);
    }
}
