using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAClassifierAssignment
{
    /// <summary>
    /// Represents a Boundry condition for a rule.
    /// A boundry condition has a range of values between low and high with which to match against a DataFP object.
    /// </summary>
    class RuleFPBoundry
    {
        // Upper boundry for the condition
        public float high;
        // Lower boundry fo the condition
        public float low;

        public RuleFPBoundry(float low, float high)
        {
            this.high = high;
            this.low = low;
        }

        public RuleFPBoundry(RuleFPBoundry clonedBoundry)
        {
            this.high = clonedBoundry.high;
            this.low = clonedBoundry.low;
        }
    }
}
