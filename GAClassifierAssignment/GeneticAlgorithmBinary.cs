using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace GAClassifierAssignment
{
    class GeneticAlgorithmBinary
    {
        // GA configuration parameters.
        private int numGenes;
        private int conditionLength;
        private int populationSize;
        private int numberOfRules;
        private int totalGenerations;
        private double crossoverRate;
        private double mutationRate;
        private double mutationRateOriginal;
        private int tournamentSize;

        // The supplied training data.
        private List<Rule> dataList;

        // GA progress tracking.
        private int generationCounter;
        private int totalFitness = 0;
        private int maxFitness = 0;
        private double meanFitness = 0;

        /// <summary>
        /// The individual or Genome for the Binary GA.
        /// </summary>
        public class Individual
        {
            public int[] genes;
            public Rule[] rules;
            public int fitness;

            public Individual(int totalGenes, int fitness)
            {
                genes = new int[totalGenes];
                this.fitness = fitness;
            }

            /// <summary>
            /// For cloning purposes.
            /// </summary>
            /// <param name="individualToClone"></param>
            public Individual(Individual individualToClone)
            {
                this.genes = individualToClone.genes.ToArray();
                this.rules = individualToClone.rules.ToArray();
                this.fitness = individualToClone.fitness;
            }
        }

        // Our active population of individuals.
        private Individual[] population;

        public GeneticAlgorithmBinary(List<Rule>dataList, int numberOfRules, int totalGenerations, double crossoverRate, int tournamentSize, 
            double? mutationRate = null, int? populationSize = null)
        {
            // We infer number of genes required and population size from the provided rule list (add 1 for output).
            this.numGenes = (dataList[0].cond.Length + 1) * numberOfRules;

            // Condition length inferred from supplied data.
            this.conditionLength = dataList[0].cond.Length;

            // Population defaults to the size of the supplied dataList.
            if (populationSize.HasValue)
            {
                this.populationSize = populationSize.Value;
            }
            else
            {
                this.populationSize = dataList.Count;
            }
            this.numberOfRules = numberOfRules;

            this.totalGenerations = totalGenerations;
            this.crossoverRate = crossoverRate;

            // Mutation rate defaults to 1/number of genes.
            if (mutationRate == null)
            {
                this.mutationRate = 1.0 / (double) numGenes;
            }
            else
            {
                this.mutationRate = mutationRate.GetValueOrDefault();
            }

            this.mutationRateOriginal = this.mutationRate;
            this.tournamentSize = tournamentSize;

            this.dataList = dataList;
            generationCounter = 1;

            population = new Individual[this.populationSize];
        }

        /// <summary>
        /// Generate a selection of randomly generated individuals.
        /// </summary>
        public void InitiatePopulation()
        {
            Random random = new Random();

            for (int i = 0; i < populationSize; i++)
            {
                population[i] = new Individual(numGenes, fitness:0);
                int counter = 1;
                for (int j = 0; j < numGenes; j++)
                {
                    // we use random.Next(3) to have encoding for * (wildcards)
                    // we only allow (0,1) for every (conditionlength +1)th gene so that the output isnt a wildcard
                    if (counter % (conditionLength + 1) == 0)
                    {
                        population[i].genes[j] = random.Next(2);
                    }
                    else
                    {
                        population[i].genes[j] = random.Next(3);
                    }

                    counter++;
                }
            }
        }

        /// <summary>
        /// We calculate the total fitness of this individuals rulebase against the supplied training data.
        /// </summary>
        /// <param name="individual"></param>
        public void CalculateFitness(Individual individual)
        {
            individual.fitness = 0;
            individual.rules = new Rule[numberOfRules];
            int index = 0;
            Rule[] ruleBase = new Rule[numberOfRules];

            for (int i = 0; i < numberOfRules; i++)
            {
                ruleBase[i] = new Rule(new int[conditionLength], 0, 0);
            }

            // Here we decode the genes of the individual into a rule base.
            for (int i = 0; i < numberOfRules; i++)
            {
                for (int j = 0; j < conditionLength; j++)
                {
                    ruleBase[i].cond[j] = individual.genes[index++];
                }
                ruleBase[i].output = individual.genes[index++];

                individual.rules[i] = new Rule(ruleBase[i]);
            }

            // Go over each rule, try to match it against data.
            foreach (var data in dataList)
            {
                foreach (var rule in individual.rules)
                {
                    int counter = 0;

                    // Match conditions.
                    for (int k = 0; k < rule.cond.Length; k++)
                    {
                        if (rule.cond[k] == data.cond[k] || rule.cond[k] == 2)
                        {
                            counter++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (counter == rule.cond.Length)
                    {
                        if (rule.output == data.output)
                        {
                            rule.fitness++;
                        }

                        // We move onto the next data item as this rule is predicting the opposite to our desired output.
                        break;
                    }
                }
            }

            foreach (var rule in individual.rules)
            {
                individual.fitness += rule.fitness;
            }
        }

        /// <summary>
        /// We select a number of individuals and compare their fitness.
        /// Fittest individual returned.
        /// </summary>
        /// <param name="tournamentSize"></param>
        /// <returns></returns>
        public Individual TournamentSelection(int tournamentSize)
        {
            Random random = new Random();

            List<Individual> tournamentParticipants = new List<Individual>();

            for (int i = 0; i < tournamentSize; i++)
            {
                tournamentParticipants.Add(population[random.Next(populationSize)]);
            }

            return tournamentParticipants.OrderByDescending(r => r.fitness).FirstOrDefault();
        }

        /// <summary>
        /// We return the fittest individual from the current population.
        /// </summary>
        /// <returns></returns>
        private Individual FindBestIndividual()
        {
            Individual bestIndividual = population[0];
            foreach (var individual in population)
            {
                if (individual.fitness > bestIndividual.fitness)
                {
                    bestIndividual = individual;
                }
            }

            return bestIndividual;
        }

        /// <summary>
        /// We return the least fit individual from the current population.
        /// </summary>
        /// <returns></returns>
        private Individual FindWorstIndividual()
        {
            Individual worstIndividual = population[0];
            foreach (var individual in population)
            {
                if (individual.fitness < worstIndividual.fitness)
                {
                    worstIndividual = individual;
                }
            }

            return worstIndividual;
        }

        /// <summary>
        /// Select 2 rules from individual 1 and 2.
        /// Rule 1A, 2A, 1B, 2B.
        /// Swap 1A with 2B.
        /// Swap 2A with 1B.
        /// </summary>
        /// <param name="individual1"></param>
        /// <param name="individual2"></param>
        /// <returns></returns>
        public Tuple<Individual, Individual> CrossoverSwapRules(Individual individual1, Individual individual2)
        {
            Individual newIndividual1 = Clone(individual1);
            Individual newIndividual2 = Clone(individual2);

            Random random = new Random();

            // Check for crossover.
            if (random.NextDouble() < crossoverRate)
            {
                int ruleSelectionA = random.Next(0, numberOfRules - 1);
                int ruleSelectionB = random.Next(0, numberOfRules - 1);
                int[] tempSelectionA1 = new int[conditionLength + 1];
                int[] tempSelectionA2 = new int[conditionLength + 1];
                int[] tempSelectionB1 = new int[conditionLength + 1];
                int[] tempSelectionB2 = new int[conditionLength + 1];

                int counter = 0;

                for (int selection = ruleSelectionA * (conditionLength + 1);
                    selection < (ruleSelectionA * (conditionLength + 1)) + conditionLength + 1;
                    selection++)
                {
                    // Getting rule 1A and 2A.
                    tempSelectionA1[counter] = newIndividual1.genes[selection];
                    tempSelectionA2[counter] = newIndividual2.genes[selection];
                    counter++;
                }

                counter = 0;

                for (int selection = ruleSelectionB * (conditionLength + 1);
                    selection < (ruleSelectionB * (conditionLength + 1)) + conditionLength + 1;
                    selection++)
                {
                    // Getting rule 1B and 2B
                    tempSelectionB1[counter] = newIndividual1.genes[selection];
                    tempSelectionB2[counter] = newIndividual2.genes[selection];

                    // Setting rule 1A and 2B to 1B and 2B's original position.
                    newIndividual1.genes[selection] = tempSelectionA2[counter];
                    newIndividual2.genes[selection] = tempSelectionA1[counter];
                    counter++;
                }

                counter = 0;

                for (int selection = ruleSelectionA * (conditionLength + 1);
                    selection < (ruleSelectionA * (conditionLength + 1)) + conditionLength + 1;
                    selection++)
                {
                    // Setting rule 1B and 2B to 1A and 2A's original position.
                    newIndividual1.genes[selection] = tempSelectionB2[counter];
                    newIndividual2.genes[selection] = tempSelectionB1[counter];
                    counter++;
                }

            }

            return new Tuple<Individual, Individual>(newIndividual1,newIndividual2);
        }

        /// <summary>
        /// Performs crossover in Davis order.
        /// </summary>
        /// <param name="individual1"></param>
        /// <param name="individual2"></param>
        /// <returns></returns>
        public Tuple<Individual, Individual> CrossoverDavisOrder(Individual individual1, Individual individual2)
        {
            Individual newIndividual1 = Clone(individual1);
            Individual tempIndividualA = Clone(individual1);
            Individual tempIndividualB = Clone(individual2);
            Individual newIndividual2 = Clone(individual2);

            int[] newIndividual1Genes = new int[newIndividual1.genes.Length];
            int[] newIndividual2Genes = new int[newIndividual2.genes.Length];
            int[] tempIndividualAGenes = new int[tempIndividualA.genes.Length];
            int[] tempIndividualBGenes = new int[tempIndividualB.genes.Length];

            Array.Copy(newIndividual1.genes, newIndividual1Genes, newIndividual1Genes.Length);
            Array.Copy(newIndividual2.genes, newIndividual2Genes, newIndividual2Genes.Length);
            Array.Copy(tempIndividualA.genes, tempIndividualAGenes, newIndividual2Genes.Length);
            Array.Copy(tempIndividualB.genes, tempIndividualBGenes, newIndividual2Genes.Length);

            Random random = new Random();

            // Check for crossover.
            if (random.NextDouble() < crossoverRate)
            {

                int ruleSelectionA = random.Next(0, numberOfRules - 1);
                int[] tempSelection = new int[conditionLength + 1];



                for (int selection = ruleSelectionA * (conditionLength + 1);
                    selection < (ruleSelectionA * (conditionLength + 1)) + conditionLength + 1;
                    selection++)
                {
                    // We extract a rule from A and B.
                    tempIndividualAGenes[selection] = newIndividual1Genes[selection];
                    tempIndividualBGenes[selection] = newIndividual2Genes[selection];
                }

                int counter = 0;
                for (int afterSelection = ((ruleSelectionA * (conditionLength + 1)) + conditionLength + 1); afterSelection < tempIndividualA.genes.Length;
                    afterSelection++)
                {
                    // We start the wrap around going from the end of the original rule to the end of the chromosome.
                    tempIndividualAGenes[afterSelection] = newIndividual2Genes[counter];
                    tempIndividualBGenes[afterSelection] = newIndividual1Genes[counter];
                    counter++;
                }

                for (int beforeSelection = 0; beforeSelection < ruleSelectionA * (conditionLength + 1);
                    beforeSelection++)
                {
                    // We finish the wrap around going from the beginning of the chromosome to the start of the original rule.
                    tempIndividualAGenes[beforeSelection] = newIndividual2Genes[counter];
                    tempIndividualBGenes[beforeSelection] = newIndividual1Genes[counter];
                    counter++;
                }

            }

            tempIndividualA.genes = tempIndividualAGenes;
            tempIndividualB.genes = tempIndividualBGenes;

            return new Tuple<Individual, Individual>(tempIndividualA, tempIndividualB);
        }

        /// <summary>
        /// We perform a bitwise crossover.
        /// </summary>
        /// <param name="individual1"></param>
        /// <param name="individual2"></param>
        /// <returns></returns>
        public Tuple<Individual, Individual> CrossoverBitwise(Individual individual1, Individual individual2)
        {
            Individual newIndividual1 = Clone(individual1);
            Individual newIndividual2 = Clone(individual2);

            Random random = new Random();

            // Check for crossover.
            if (random.NextDouble() < crossoverRate)
            {
                // Bitwise crossover point selection.
                int crossoverPoint = random.Next(0, numGenes);

                // We perform the crossover.
                for (int j = crossoverPoint; j < numGenes; j++)
                {
                    int temp = individual1.genes[j];
                    newIndividual1.genes[j] = newIndividual2.genes[j];
                    newIndividual2.genes[j] = temp;
                }
            }

            return new Tuple<Individual, Individual>(newIndividual1, newIndividual2);
        }

        /// <summary>
        /// We perform a uniform crossover.
        /// </summary>
        /// <param name="individual1"></param>
        /// <param name="individual2"></param>
        /// <returns></returns>
        public Tuple<Individual, Individual> CrossoverUniform(Individual individual1, Individual individual2)
        {
            Individual newIndividual1 = Clone(individual1);
            Individual newIndividual2 = Clone(individual2);

            Random random = new Random();

            // Check for crossover.
            if (random.NextDouble() < crossoverRate)
            {
                // Perform crossover.
                for (int i = 0; i < numGenes; i++)
                {
                    if (random.Next(2) > 0)
                    {
                        int temp = newIndividual1.genes[i];
                        newIndividual1.genes[i] = newIndividual2.genes[i];
                        newIndividual2.genes[i] = temp;
                    }
                }
            }

            return new Tuple<Individual, Individual>(newIndividual1, newIndividual2);
        }

        /// <summary>
        /// We clone an individual so that it is not linked by reference anymore.
        /// </summary>
        /// <param name="individual"></param>
        /// <returns></returns>
        private Individual Clone(Individual individual)
        {
            Individual clone = new Individual(numGenes, 0);

            Array.Copy(individual.genes, clone.genes, individual.genes.Length);

            //for (int i = 0; i < individual.genes.Length; i++)
            //{
            //    clone.genes[i] = individual.genes[i];
            //}

            clone.fitness = individual.fitness;
            return clone;
        }

        /// <summary>
        /// Perform a bitwise mutation.
        /// </summary>
        /// <param name="individual"></param>
        /// <returns></returns>
        public Individual Mutate(Individual individual)
        {
            Random random = new Random();
            double mutationPick = random.NextDouble();

            var newIndividual = Clone(individual);

            bool mutateSuccess = true;
            
            // We check for mutation against a randomly selected gene.
            while (mutateSuccess)
            {
                if (mutationPick < mutationRate)
                {
                    mutationPick = random.NextDouble();
                    var picker = random.Next(numGenes);

                    // we use random.Next(3) to have encoding for * (wildcards)
                    // we only allow (0,1) for every (conditionlength +1)th gene so that the output isnt a wildcard
                    int counter = 1 + picker;
                    if (counter % (conditionLength + 1) == 0)
                    {
                        newIndividual.genes[picker] = random.Next(2);
                    }
                    else
                    {
                        newIndividual.genes[picker] = random.Next(3);
                    }
                }
                else
                {
                    mutateSuccess = false;
                }
            }

            return newIndividual;
        }

        /// <summary>
        /// We Initiate and evolve our GA here until it meets the termination conditions.
        /// </summary>
        /// <param name="requiredFitness"></param>
        /// <returns></returns>
        public Tuple<Rule[], int> RunGA(int requiredFitness)
        {
            // For tracking purposes.
            int[] maximumFitness = new int[totalGenerations + 1];
            double[] averageFitness = new double[totalGenerations + 1];

            // We fill our population with randomly generated chromosomes.
            InitiatePopulation();

            // Each chromosome's fitness is calculated.
            foreach (var individual in population)
            {
                CalculateFitness(individual);
            }

            maximumFitness[0] = maxFitness;
            averageFitness[0] = meanFitness;

            var random = new Random();

            // We enter the main generationasl loop of the GA.
            for (int k = 0; k < totalGenerations; k++)
            {
                Individual[]offspring = new Individual[populationSize];
                // Ensure best Individual survives the selection process.
                Individual bestIndividual = FindBestIndividual();

                // Check for termination condition.
                if (bestIndividual.fitness >= requiredFitness || k == totalGenerations - 1)
                {
                    return (new Tuple<Rule[], int>(bestIndividual.rules, generationCounter));
                }

                List<Individual> breedingStock = new List<Individual>();
                List<Individual> potentialOffspring = new List<Individual>();
                // We fill half the new generations population with children, the other half from parents.
                for (int child = 0; child < populationSize / 2; child++)
                {
                    // The arduous trials of sexual selection.
                    breedingStock.Add(TournamentSelection(tournamentSize));
                }

                // We perform crossover and mutation.
                for (int breeder = 0; breeder < breedingStock.Count - 1; breeder = breeder + 2)
                {
                    var individual1 = breedingStock[random.Next(breedingStock.Count - 1)];
                    var individual2 = breedingStock[random.Next(breedingStock.Count - 1)];

                    // Crossover.
                    var temp = CrossoverBitwise(individual1, individual2);
                    var newIndividual1 = temp.Item1;
                    var newIndividual2 = temp.Item2;

                    // Mutation.
                    newIndividual1 = Mutate(newIndividual1);
                    newIndividual2 = Mutate(newIndividual2);
                    // Fitness calculation.
                    CalculateFitness(newIndividual1);
                    CalculateFitness(newIndividual2);
                    potentialOffspring.Add(newIndividual1);
                    potentialOffspring.Add(newIndividual2);
                }

                // We add offspring to population and fill the remainder with tournament selected parents.
                for (int pop = 0; pop < populationSize; pop++)
                {
                    if (pop < potentialOffspring.Count)
                    {
                        offspring[pop] = potentialOffspring[pop];
                    }
                    else
                    {
                        offspring[pop] = TournamentSelection(tournamentSize);
                    }
                }

                // Replace population with offspring.
                population = offspring;

                // Replace worst individual with prev generation best individual if there isnt a better one in the pop.
                if (bestIndividual.fitness > FindBestIndividual().fitness)
                {
                    var worst = FindWorstIndividual();

                    for (int i = 0; i < populationSize; i++)
                    {
                        if (population[i] == worst)
                        {
                            population[i] = bestIndividual;
                        }
                    }
                }

                generationCounter++;

                // Getting stats.
                totalFitness = 0;
                maxFitness = 0;
                meanFitness = 0;

                foreach (Individual ind in population)
                {
                    if (ind.fitness > maxFitness)
                    { maxFitness = ind.fitness; }

                    totalFitness += ind.fitness;
                }

                meanFitness = (double)totalFitness / (double)populationSize;

                maximumFitness[k + 1] = maxFitness;
                averageFitness[k + 1] = meanFitness;

                // Inform user whats going on with their GA.
                if (generationCounter % 1000 == 0)
                {
                    Console.WriteLine($"Generation: {generationCounter} total fitness = {totalFitness}, mean fitness = {meanFitness}, max fitness = {maxFitness}");
                }

                // Simple adaptive mutation.
                if ((double)meanFitness / (double)maxFitness > 0.95)
                {
                    mutationRate += 0.001f;
                    if (mutationRate > 1.0f)
                    {
                        mutationRate = 1.0f;
                    }
                }
                else
                {
                    mutationRate -= 0.001f;
                    if (mutationRate < mutationRateOriginal)
                    {
                        mutationRate = mutationRateOriginal;
                    }
                }
            }
            return null;
        }
    }
}
