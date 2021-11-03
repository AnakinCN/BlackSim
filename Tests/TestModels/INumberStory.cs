using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// the interface that supports simple Number plus and minus test
    /// </summary>
    public interface INumberStory
    {
        /// <summary>
        /// the Number being modified
        /// </summary>
        public int Number { get; set; }
    }
}
