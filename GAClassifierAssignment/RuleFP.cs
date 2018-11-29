using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAClassifierAssignment
{
    /// <summary>
    /// Represents a floating point rule for data set 3. 
    /// </summary>
    class RuleFP
    {
        // List of conditions.
        public List<RuleFPBoundry> condBoundry;
        // Rule output
        public int output;
        // A rule's individual fitness for tracking purposes.
        public int fitness;

        public RuleFP()
        {
            condBoundry = new List<RuleFPBoundry>();
        }

        public RuleFP(List<RuleFPBoundry> condBoundry, int output)
        {
            this.condBoundry = condBoundry;
            this.output = output;
        }

        /// <summary>
        /// Used for cloning purposes.
        /// </summary>
        /// <param name="clonedRule"></param>
        public RuleFP(RuleFP clonedRule)
        {
            this.condBoundry = new List<RuleFPBoundry>();
            this.output = clonedRule.output;
            this.fitness = clonedRule.fitness;

            foreach (var condBoundry in clonedRule.condBoundry)
            {
                this.condBoundry.Add(new RuleFPBoundry(condBoundry));
            }
        }

    }
}
