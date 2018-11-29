using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAClassifierAssignment
{
    /// <summary>
    /// Represents a binary rule for data set 1 and 2.
    /// </summary>
    class Rule
    {
        // Input parameters
        public int[] cond;
        // Output
        public int output;
        // A rule's individual fitness for tracking purposes.
        public int fitness;

        public Rule(int[] cond, int output, int fitness)
        {
            this.cond = cond;
            this.output = output;
            this.fitness = fitness;
        }

        /// <summary>
        /// Used for cloning purposes.
        /// </summary>
        /// <param name="cloneRule"></param>
        public Rule(Rule cloneRule)
        {
            this.cond = cloneRule.cond.ToArray();
            this.output = cloneRule.output;
            this.fitness = cloneRule.fitness;
        }


    }
}
