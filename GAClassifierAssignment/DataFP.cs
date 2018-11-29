using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAClassifierAssignment
{
    /// <summary>
    /// Represents an individual item of data in data-set3
    /// </summary>
    class DataFP
    {

        public float[] cond;
        public int output;

        public DataFP(float[] cond, int output)
        {
            this.cond = cond;
            this.output = output;
        }


    }
}
