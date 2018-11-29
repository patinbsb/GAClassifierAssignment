using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAClassifierAssignment
{
    /// <summary>
    /// Represents an individual (chromosome) for data set 3.
    /// </summary>
    class IndividualFP
    {
        // The rules which the individual is evaluated against.
        public List<RuleFP> Rulebase { get; set; }

        // Data used in training the rulebase of the individual.
        private static List<DataFP> _TrainingData;

        // Data for which to evaluate a trained individual against.
        private static List<DataFP> _EvaluationData;

        /// <summary>
        /// We use this to calculate the sum of fitness for each rule against the trainingData.
        /// </summary>
        public int fitness
        {
            get
            {
                foreach (var rule in Rulebase)
                {
                    rule.fitness = 0;
                }
                int trainingFitness = 0;

                foreach (var data in _TrainingData)
                {
                    foreach (var rule in Rulebase)
                    {
                        if (ConditionsMatch(rule, data))
                        {
                            if (data.output == rule.output)
                            {
                                trainingFitness++;
                                rule.fitness++;
                            }
                            break;
                        }
                    }
                }

                return trainingFitness;
            }
        }

        /// <summary>
        /// We use this to calculate the sum of fitness for each rule against the evaluationData.
        /// </summary>
        public int evaluationFitness
        {
            get
            {
                int evalFitness = 0;

                foreach (var data in _EvaluationData)
                {
                    foreach (var rule in Rulebase)
                    {
                        if (ConditionsMatch(rule, data))
                        {
                            if (data.output == rule.output)
                            {
                                evalFitness++;
                            }
                            break;
                        }
                    }
                }

                return evalFitness;
            }
        }

        public IndividualFP()
        {
            Rulebase = new List<RuleFP>();
        }

        /// <summary>
        /// Used for cloning purposes.
        /// </summary>
        /// <param name="clonedIndividual"></param>
        public IndividualFP(IndividualFP clonedIndividual)
        {
            Rulebase = new List<RuleFP>();
            foreach (var rule in clonedIndividual.Rulebase)
            {
                Rulebase.Add(new RuleFP(rule));
            }
        }

        /// <summary>
        /// Checks that Each RuleFPBoundry's range is inclusive of the supplied Data's condition.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="inputData"></param>
        /// <returns></returns>
        private bool ConditionsMatch(RuleFP rule, DataFP inputData)
        {
            for (int i = 0; i < rule.condBoundry.Count; i++)
            {
                if (rule.condBoundry[i].low > inputData.cond[i] || rule.condBoundry[i].high < inputData.cond[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static void SetTrainingAndEvaluationData(List<DataFP> trainingData, List<DataFP> evaluationData)
        {
            _TrainingData = trainingData;
            _EvaluationData = evaluationData;
        }
    }
}
