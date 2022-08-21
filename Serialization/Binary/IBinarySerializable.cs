using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLD.Serialization.Binary
{
    public interface IBinarySerializable
    {
        void Serialize(BinaryWriter writer);
    }
}
