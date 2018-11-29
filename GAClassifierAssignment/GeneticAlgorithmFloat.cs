using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GAClassifierAssignment
{
    class GeneticAlgorithmFloat
    {
        // GA configuration parameters.
        private int boundryLength;
        private int populationSize;
        private int numberOfRules;
        private int totalGenerations;
        private float crossoverRate;
        private float mutationRate;
        private float mutationRateOriginal;
        private float mutationRange;
        private float mutationRangeOriginal;
        private int tournamentSize;
        private int tournamentSizeOriginal;

        // GA progress tracking.
        private int generationCounter;
        private int totalFitness = 0;
        private int maxFitness = 0;
        private float meanFitness = 0;

        // Our active population of individuals.
        private List<IndividualFP> population;

        // A lookup table for individual's fitness.
        // Helps avoid unessecary expensive fitness calculations.
        private Dictionary<IndividualFP, int> fitnessLookup;

        public GeneticAlgorithmFloat(List<DataFP> dataList, int numberOfRules, int totalGenerations,
            float crossoverRate, int tournamentSize, float mutationRange, float mutationRate, int? populationSize = null)
        {

            // Condition length inferred from supplied data.
            this.boundryLength = dataList[0].cond.Length;
            // Population defaults to the size of the supplied dataList.
            this.populationSize = populationSize ?? dataList.Count;
            this.numberOfRules = numberOfRules;

            this.totalGenerations = totalGenerations;
            this.crossoverRate = crossoverRate;
            this.mutationRate = mutationRate;
            this.mutationRange = mutationRange;
            this.mutationRateOriginal = mutationRate;
            this.mutationRangeOriginal = mutationRange;
            this.tournamentSize = tournamentSize;
            this.tournamentSizeOriginal = tournamentSize;

            // Split up our data into training and evaluation sets.
            IndividualFP.SetTrainingAndEvaluationData(dataList.Take(1000).ToList(),
                dataList.Skip(1000).Take(1000).ToList());
            generationCounter = 1;

            population = new List<IndividualFP>();
            fitnessLookup = new Dictionary<IndividualFP, int>();
        }

        /// <summary>
        /// Generate a selection of randomly generated individuals.
        /// low range 0-0.5, high range 0.5-1.0.
        /// </summary>
        public void InitiatePopulation()
        {
            Random random = new Random();

            for (int i = 0; i < populationSize; i++)
            {
                population.Add(new IndividualFP());

                List<RuleFP> rulesToAdd = new List<RuleFP>();

                for (int r = 0; r < numberOfRules; r++)
                {
                    var currentRule = new RuleFP();
                    for (int c = 0; c < boundryLength; c++)
                    {
                        RuleFPBoundry conditionToAdd = null;
                        var low = random.NextDouble() / 2; // 0 - 0.5
                        var high = 0.5 + random.NextDouble() / 2; // 0.5 - 1.0

                        conditionToAdd = new RuleFPBoundry((float)low, (float)high);

                        currentRule.condBoundry.Add(conditionToAdd);
                    }

                    // Output (0,1)
                    currentRule.output = random.Next(2);
                    rulesToAdd.Add(currentRule);
                }

                population[i].Rulebase = rulesToAdd;
            }
        }

        /// <summary>
        /// We select a number of individuals and compare their fitness.
        /// Fittest individual returned.
        /// </summary>
        /// <param name="tournamentSize"></param>
        /// <returns></returns>
        public IndividualFP TournamentSelection(int tournamentSize)
        {
            Random random = new Random();

            List<IndividualFP> tournamentParticipants = new List<IndividualFP>();

            for (int i = 0; i < tournamentSize; i++)
            {
                tournamentParticipants.Add(population[random.Next(populationSize)]);
            }

            return new IndividualFP(tournamentParticipants.OrderByDescending(r => r.fitness).FirstOrDefault());
        }

        /// <summary>
        /// We generate offspring vis tournament selection.
        /// </summary>
        /// <param name="tournamentSize"></param>
        /// <returns></returns>
        public List<IndividualFP> TournamentSelection(List<IndividualFP> population)
        {
            Random random = new Random();
            var offspring = new List<IndividualFP>();

            for (int i = 0; i < population.Count; i++)
            {
                //Select the parents from the population
                var parents = new List<IndividualFP>();
                for (int j = 0; j < tournamentSize; j++)
                {
                    //Pick random parents from the population
                    parents.Add(population[random.Next(populationSize)]);
                }

                int fitnessMax = 0;
                var bestParent = new IndividualFP(population[random.Next(populationSize)]);
                foreach (var parent in parents)
                {
                    if (fitnessLookup[parent] > fitnessMax)
                    {
                        fitnessMax = fitnessLookup[parent];
                        bestParent = parent;
                    }
                }

                //Select the best parent to be the offspring
                offspring.Add(new IndividualFP(bestParent));
            }

            return offspring;
        }

        /// <summary>
        /// We return the fittest individual from the current population.
        /// </summary>
        /// <returns></returns>
        private IndividualFP FindBestIndividual()
        {
            return fitnessLookup.OrderByDescending(x => x.Value).First().Key;
        }

        /// <summary>
        /// We return the least fit individual from the current population.
        /// </summary>
        /// <returns></returns>
        private IndividualFP FindWorstIndividual()
        {
            return fitnessLookup.OrderByDescending(x => x.Value).Last().Key;
        }

        /// <summary>
        /// We perform a bitwise crossover on the supplied population.
        /// </summary>
        /// <returns></returns>
        public List<IndividualFP> CrossoverSinglePoint(List<IndividualFP> population)
        {
            Random random = new Random();
            var crossoverOutput = new List<IndividualFP>(population);

            for (int i = 0; i < crossoverOutput.Count; i += 2)
            {

                var tempParent1 = new IndividualFP(crossoverOutput[i]);
                var tempParent2 = new IndividualFP(crossoverOutput[i + 1]);

                int crossoverPoint = random.Next(numberOfRules);

                // Check for crossover.
                if (random.NextDouble() < crossoverRate)
                {
                    for (int j = crossoverPoint; j < numberOfRules; j++)
                    {
                        tempParent1.Rulebase[j] = crossoverOutput[i + 1].Rulebase[j];
                        tempParent2.Rulebase[j] = crossoverOutput[i].Rulebase[j];
                    }
                }

                crossoverOutput[i] = tempParent1;
                crossoverOutput[i + 1] = tempParent2;
            }

            return crossoverOutput;
        }

        /// <summary>
        /// We perform a uniform crossover on the supplied population.
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        public List<IndividualFP> CrossoverUniform(List<IndividualFP> population)
        {
            Random random = new Random();
            var crossoverOutput = new List<IndividualFP>(population);

            for (int i = 0; i < crossoverOutput.Count; i += 2)
            {

                var tempParent1 = new IndividualFP(crossoverOutput[i]);
                var tempParent2 = new IndividualFP(crossoverOutput[i + 1]);

                // Check for crossover.
                if (random.NextDouble() < crossoverRate)
                {
                    for (int j = 0; j < numberOfRules; j++)
                    {
                        for (int k = 0; k < boundryLength; k++)
                        {
                            if (random.Next(2) > 0)
                            {
                                if (random.Next(2) > 0)
                                {
                                    tempParent1.Rulebase[j].condBoundry[k].high =
                                        crossoverOutput[i + 1].Rulebase[j].condBoundry[k].high;
                                    tempParent2.Rulebase[j].condBoundry[k].high =
                                        crossoverOutput[i].Rulebase[j].condBoundry[k].high;
                                }
                                else
                                {
                                    tempParent1.Rulebase[j].condBoundry[k].low =
                                        crossoverOutput[i + 1].Rulebase[j].condBoundry[k].low;
                                    tempParent2.Rulebase[j].condBoundry[k].low =
                                        crossoverOutput[i].Rulebase[j].condBoundry[k].low;
                                }
                            }
                        }

                        if (random.Next(2) > 0)
                        {
                            tempParent1.Rulebase[j].output = crossoverOutput[i + 1].Rulebase[j].output;
                            tempParent2.Rulebase[j].output = crossoverOutput[i].Rulebase[j].output;
                        }
                    }
                }

                crossoverOutput[i] = tempParent1;
                crossoverOutput[i + 1] = tempParent2;
            }

            return crossoverOutput;
        }

        /// <summary>
        /// We perform a bitwise mutation on the supplied population.
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        public List<IndividualFP> Mutate(List<IndividualFP> population)
        {

            Random random = new Random();
            double mutationPick = random.NextDouble();

            var outputPopulation = new List<IndividualFP>(population);

            foreach (var individual in outputPopulation)
            {
                for (int i = 0; i < individual.Rulebase.Count; i++)
                {
                    for (int j = 0; j < individual.Rulebase[i].condBoundry.Count; j++)
                    {
                        // Check for mutation.
                        if (random.NextDouble() < mutationRate)
                        {
                            double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
                            double u2 = 1.0 - random.NextDouble();
                            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                                   Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                            double randNormal = mutationRange * randStdNormal;

                            individual.Rulebase[i] = new RuleFP(individual.Rulebase[i]);
                            if (random.Next() % 2 == 0)
                            {
                                double newUpperBound = individual.Rulebase[i].condBoundry[j].high + randNormal;
                                if (newUpperBound > 1)
                                    newUpperBound = 1;
                                if (newUpperBound < 0)
                                    newUpperBound = 0;
                                individual.Rulebase[i].condBoundry[j] =
                                    new RuleFPBoundry(individual.Rulebase[i].condBoundry[j].low,
                                        (float) newUpperBound);
                            }
                            else
                            {
                                double newLowerBound = individual.Rulebase[i].condBoundry[j].low + randNormal;
                                if (newLowerBound > 1)
                                    newLowerBound = 1;
                                if (newLowerBound < 0)
                                    newLowerBound = 0;
                                individual.Rulebase[i].condBoundry[j] =
                                    new RuleFPBoundry((float)newLowerBound,
                                        individual.Rulebase[i].condBoundry[j].low);
                            }

                            if (individual.Rulebase[i].condBoundry[j].low > individual.Rulebase[i].condBoundry[j].high)
                            {
                                double newLowerBound = Math.Max(individual.Rulebase[i].condBoundry[j].high, 0.00f);
                                double newUpperBound = Math.Min(individual.Rulebase[i].condBoundry[j].low, 1.00f);


                                individual.Rulebase[i].condBoundry[j] = new RuleFPBoundry((float)newLowerBound, (float)newUpperBound);
                            }

                        }
                    }
                }
            }

            return outputPopulation;
        }

        /// <summary>
        /// We update the lookup table with our latest generation of chromosomes.
        /// </summary>
        public void fitnessToLookup()
        {
            fitnessLookup.Clear();
            foreach (var individual in population)
            {
                fitnessLookup.Add(individual, individual.fitness);
            }
        }

        /// <summary>
        /// We initiate and evolve our GA here until it meets the termination conditions.
        /// </summary>
        /// <param name="requiredFitness"></param>
        /// <returns></returns>
        public Tuple<List<RuleFP>, int, int, int> RunGA(int requiredFitness)
        {
            // For tracking purposes.
            int[] maximumFitness = new int[totalGenerations + 1];
            double[] averageFitness = new double[totalGenerations + 1];
            List<double> meanToMaxRatioOverTime = new List<double>();

            // These bools are an expansion on the adaptive mutation idea.
            bool stagnation = false;
            bool superStagnation = false;

            // We fill our population with randomly generated chromosomes.
            InitiatePopulation();
            // Each chromosome's fitness is calculated and appended to a lookup table.
            fitnessToLookup();

            // We keep the best individual to ensure it survives the selection process.
            IndividualFP best = new IndividualFP(FindBestIndividual());
            maximumFitness[0] = maxFitness;
            averageFitness[0] = meanFitness;

            // We enter the main generationasl loop of the GA.
            for (int k = 0; k < totalGenerations; k++)
            {
                Random random = new Random();
                // Tournament selection of offspring.
                var offspring = TournamentSelection(population);

                // We switch our crossover strategy to Uniform if gene stagnation is detected.
                if (stagnation && random.Next(100) < 10)
                {
                    offspring = CrossoverUniform(offspring);
                }
                else
                {
                    offspring = CrossoverSinglePoint(offspring);
                }
                // We mutate our offspring.
                offspring = Mutate(offspring);

                // Offspring added to population.
                population.Clear();
                population.AddRange(offspring);
                // Best fitness individual added back into population.
                population[random.Next(populationSize)] = best;

                fitnessToLookup();

                best = new IndividualFP(FindBestIndividual());

                // Check for termination condition.
                if (best.fitness >= requiredFitness || k == totalGenerations - 1)
                {
                    return new Tuple<List<RuleFP>, int, int, int>(best.Rulebase.OrderByDescending(x => x.fitness).ToList(), generationCounter, best.fitness, best.evaluationFitness);
                }

                offspring.Clear();

                generationCounter++;

                // Getting stats.
                totalFitness = 0;
                maxFitness = 0;
                meanFitness = 0;

                foreach (var ind in fitnessLookup)
                {
                    if (ind.Value > maxFitness)
                    { maxFitness = ind.Value; }

                    totalFitness += ind.Value;
                }

                meanFitness = (float)totalFitness / (float)populationSize;

                maximumFitness[k + 1] = maxFitness;
                averageFitness[k + 1] = meanFitness;
                double meanToMaxRatio = (double)meanFitness / (double)maxFitness;


                if (generationCounter % 10 == 0)
                {
                    Console.WriteLine($"Gen = {generationCounter}, mean fitness = {(int)meanFitness}, max fitness = {(int)maxFitness} MR/MRan = {mutationRate:N3}/{mutationRange:N3} S:{stagnation} MToMRatio:{meanToMaxRatio:N2}");
                }

                // Here we set super stagnation to true if the max fitness of the population hasnt increased in 200 generations.
                if (k > 200)
                {

                    if (maxFitness == maximumFitness[k - 200])
                    {
                        superStagnation = true;
                    }
                    else
                    {
                        superStagnation = false;
                    }
                }

                // Here we keep a running average of the change in the mean to max ration over time.
                meanToMaxRatioOverTime.Add(meanToMaxRatio);

                // We wait until we have 60 generations worth of data.
                if (meanToMaxRatioOverTime.Count > 60)
                {
                    var recentChange = meanToMaxRatioOverTime.Skip(meanToMaxRatioOverTime.Count - 30).Take(30).Average();
                    // Has the mean to max ratio not changed much? if so set stagnation to true.
                    // Super stagnation allows for a creater gap between maxfitness and meanfitness for exploration.
                    if (meanToMaxRatio > (recentChange - 0.07) && meanToMaxRatio < (recentChange + 0.07) && meanToMaxRatio > (superStagnation?0.90:0.96))
                    {
                        stagnation = true;
                    }
                    else
                    {
                        stagnation = false;
                    }
                }

                // Adaptive mutation.
                if (stagnation)
                {
                    tournamentSize = 2;
                    mutationRate += 0.0016f;

                    if (random.NextDouble() < 0.95 || !superStagnation)
                    {
                        mutationRange += (mutationRange/10);
                    }
                    else
                    {
                        double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
                        double u2 = 1.0 - random.NextDouble();
                        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                               Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                        double randNormal = 0.3 * randStdNormal;

                        if (randNormal < 0.0)
                        {
                            randNormal = -randNormal;
                        }

                        // We will randomly shift the mutation range to either very low or a random value to encourage exploration.
                        if (random.Next(2) > 0)
                        {
                            mutationRange = mutationRangeOriginal / 50;
                        }
                        else
                        {
                            mutationRate = mutationRateOriginal / 2;
                            mutationRange = (float)(randNormal);
                        }

                    }

                    if (mutationRate > 1.0f)
                    {
                        mutationRate = 1.0f;
                    }

                    if (mutationRange > 1.0f)
                    {
                        mutationRange = 1.0f;
                    }
                }
                else
                {
                    // If superstagnation is active, we use an agressive adaptive mutation.
                    if (superStagnation)
                    {
                        tournamentSize = 2;

                        mutationRange -= (mutationRange/10);
                        mutationRate -= 0.0016f;

                        if (mutationRate < mutationRateOriginal)
                        {
                            mutationRate = mutationRateOriginal/50;
                        }

                        if (mutationRange < mutationRangeOriginal)
                        {
                            mutationRange = mutationRangeOriginal/50;
                        }
                    }
                    // Otherwise its business as usual.
                    else
                    {
                        tournamentSize = tournamentSizeOriginal;
                        mutationRate = mutationRateOriginal;
                        mutationRange = mutationRangeOriginal;
                    }
                }


            }

            return null;
        }
    }
}
