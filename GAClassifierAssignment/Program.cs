using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GAClassifierAssignment
{
    class ArrayComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(int[] obj)
        {
            return string.Join(",", obj).GetHashCode();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // We get the Users choice for which dataset to Run the GA against.
            bool keyEntered = false;
            // which data file to select (1,2,3)
            int dataSelection = 0;

            while (!keyEntered)
            {
                Console.WriteLine("Type 1 to train against dataset1, 2 for dataset2 and 3 for dataset3.");
                var recievedKey = Console.ReadKey();

                switch (recievedKey.KeyChar)
                {
                    case '1':
                    {
                        dataSelection = 1;
                        keyEntered = true;
                        break;
                    }
                    case '2':
                    {
                        dataSelection = 2;
                        keyEntered = true;
                        break;
                    }
                    case '3':
                    {
                        dataSelection = 3;
                        keyEntered = true;
                        break;
                    }
                    default:
                    {
                        Console.WriteLine($"Incorrect key entered ({recievedKey.KeyChar}).");
                        break;
                    }
                }
            }

            // Number of times the GA will run against configured settings. Used for testing parameter settings.
            int numberOfTrials = 1;

            // Load the data into memory
            var text = File.ReadAllLines($"..\\..\\data{dataSelection}.txt");


            if (dataSelection == 3) // FloatingPointGA using Dataset 3
            {
                var ruleListFP = new List<DataFP>();

                // We parse the data to a list of DataFP objects.
                foreach (var s in text)
                {
                    float[] cond = new float[7];
                    int counter = 0;
                    bool hitOutput = false;
                    string floatFromText = "";

                    foreach (var character in s)
                    {
                        if (hitOutput)
                        {
                            ruleListFP.Add(new DataFP(cond, int.Parse(character.ToString())));
                            break;
                        }
                        else if (character == ' ' && counter == 6)
                        {
                            hitOutput = true;
                            cond[counter] = float.Parse(floatFromText);
                            floatFromText = "";
                        }
                        else if (character == ' ')
                        {
                            cond[counter] = float.Parse(floatFromText);
                            floatFromText = "";
                            counter++;
                        }
                        else
                        {
                            floatFromText += character;
                        }
                    }
                }
                //ENDPARSE

                // Configuring data which the GA will output to.
                List<RuleFP> dataRuleFps = new List<RuleFP>();
                List<int> generationCountPerRun = new List<int>();
                int bestFitness = 0;
                int bestEvaluationFitness = 0;

                // GA requirements.
                int requiredFitness = 750;
                int totalGenerations = 10000;

                Console.WriteLine($"Evolving against dataset1: required fitness = {requiredFitness}, generation limit = {totalGenerations}");

                // We run the GA for the required number of trials.
                for (int i = 0; i < numberOfTrials; i++)
                {
                    Console.WriteLine("Trial " + i);
                    // Configure GA parameters here.
                    var GeneticAlgorithmFP = new GeneticAlgorithmFloat(ruleListFP, numberOfRules: 8, totalGenerations: totalGenerations,
                        crossoverRate: 0.8f, tournamentSize:15, mutationRate:0.005f, mutationRange:0.1f, populationSize: 150);
                    var runResult = GeneticAlgorithmFP.RunGA(requiredFitness);

                    if (runResult == null)
                    {
                        Console.WriteLine($"GA failed to find solution of fitness: {requiredFitness} after {totalGenerations} generations.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }

                    dataRuleFps = runResult.Item1;
                    generationCountPerRun.Add(runResult.Item2);
                    bestFitness = runResult.Item3;
                    bestEvaluationFitness = runResult.Item4;
                }

                Console.WriteLine($"Found individual of training fitness: {bestFitness}, evaluation fitness {bestEvaluationFitness}. Required fitness was {requiredFitness}");
                string printOut = "";

                // We print out the best ruleset generated by the GA.
                foreach (var rule in dataRuleFps)
                {
                    printOut = "";
                    foreach (var condition in rule.condBoundry)
                    {
                        printOut += $"{condition.low:N5}-{condition.high:N5}| ";
                    }

                    printOut += "Output: " + rule.output + " Fitness: " + rule.fitness;
                    Console.WriteLine(printOut);
                }

                Console.WriteLine("Average number of generations: " + generationCountPerRun.Average());
                Console.ReadKey();
            }
            else // Binary GA using Dataset 1 or 2
            {
                var ruleList = new List<Rule>();


                // We parse the data to a list of Rule objects.
                foreach (var s in text)
                {
                    int[] cond = null;

                    // Setting up condition length to match the loaded dataset.
                    if (text[0].Length == 7)
                    {
                        cond = new int[5];
                    }
                    else
                    {
                        cond = new int[7];
                    }

                    bool hitOutput = false;
                    var counter = 0;
                    foreach (var character in s)
                    {
                        if (character == ' ')
                        {
                            hitOutput = true;
                        }
                        else if (hitOutput)
                        {
                            ruleList.Add(new Rule(cond, int.Parse(character.ToString()), 0));
                        }
                        else
                        {
                            cond[counter] = int.Parse(character.ToString());
                            counter++;
                        }
                    }
                }
                // ENDPARSE

                // Now we initialise our genetic algorithm with the relevant info.

                // Configuring data which the GA will output to.
                int numberOfRules = 0;
                int requiredFitness = 0;
                int totalGenerations = 150000;
                List<Rule[]> dataOut = new List<Rule[]>();
                List<int> generationCountPerRun = new List<int>();
                Tuple<Rule[], int> runResult = null;

                switch (dataSelection)
                {
                    case 1:
                    {
                        requiredFitness = 24;
                        break;
                    }
                    case 2:
                    {
                        requiredFitness = 48;
                        break;
                    }
                }

                Console.WriteLine($"Evolving against dataset{dataSelection}: required fitness = {requiredFitness}, generation limit = {totalGenerations}");

                // We run the GA for the required number of trials.
                for (int i = 0; i < numberOfTrials; i++)
                {
                    Console.WriteLine("Trial " + i);
                    if (dataSelection == 1)
                    {
                        numberOfRules = 14;
                        // Configure GA parameters here.
                        var geneticalgorithm = new GeneticAlgorithmBinary(ruleList, numberOfRules: numberOfRules, totalGenerations: totalGenerations,
                            crossoverRate: 0.7, tournamentSize:5, mutationRate: 0.4, populationSize: 120);
                        runResult = geneticalgorithm.RunGA(requiredFitness);

                    }
                    else
                    {
                        numberOfRules = 5;
                        // Configure GA parameters here.
                        var geneticalgorithm = new GeneticAlgorithmBinary(ruleList, numberOfRules: numberOfRules, totalGenerations: totalGenerations,
                            crossoverRate: 0.7, tournamentSize:3);
                        runResult = geneticalgorithm.RunGA(requiredFitness);
                    }

                    if (runResult == null)
                    {
                        Console.WriteLine($"GA failed to find solution of fitness: {requiredFitness} after {totalGenerations} generations.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }

                    dataOut.Add(runResult.Item1);
                    generationCountPerRun.Add(runResult.Item2);

                }

                string printOut = "";

                // We print out the best ruleset generated by the GA.
                List<Rule> rulesOut = new List<Rule>();
                foreach (var rules in dataOut)
                {
                    foreach (var rule in rules)
                    {
                        rulesOut.Add(rule);
                    }
                }
                var rulesDescDistinct = rulesOut.OrderByDescending(rule => rule.fitness);

                int ruleCounter = 0;
                foreach (var rule in rulesDescDistinct)
                {
                    if (ruleCounter == numberOfRules)
                    {
                        break;
                    }

                    printOut = "Rule: ";
                    foreach (var condition in rule.cond)
                    {
                        printOut += condition;
                    }

                    printOut += " Output: " + rule.output;
                    printOut += " fitness: " + rule.fitness;
                    Console.WriteLine(printOut);

                    ruleCounter++;
                }

                Console.WriteLine("Average number of generations: " + generationCountPerRun.Average());

                Console.ReadKey();
            }
        }
    }
}
