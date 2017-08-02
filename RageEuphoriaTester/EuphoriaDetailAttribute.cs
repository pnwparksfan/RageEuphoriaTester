using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rage.Euphoria
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    internal class EuphoriaDetailAttribute : Attribute
    {
        public string Description;

        public EuphoriaDetailAttribute(string desc)
        {
            this.Description = desc;
        }
    }
}
