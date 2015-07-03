using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ProjectRogue
{

    public class NameGenerator
    {
        const char suffix = ',';
        const char prefix = '*';

        MarkovChains chains;
        int maxOrder;

        Random r;

        public NameGenerator(List<string> trainingSet, int maxOrder, double prior, bool useStandardAlphabet)
        {
            r = new Random();
            this.maxOrder = maxOrder;

            List<char> alphabet;
            if(useStandardAlphabet)
                alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray().ToList(); //the prefix is NOT in the alphabet, we never want to generate one
            else
                alphabet = new List<char>();

            alphabet.Add(suffix);

            //beautify test set
            for(int i = 0; i < trainingSet.Count; i++)
            {
                foreach (char c in trainingSet[i])
                {
                    if (!alphabet.Contains(c))
                    {
                        alphabet.Add(c);
                        Debug.WriteLine("Added " + c + " to alphabet.");
                    }
                }

                trainingSet[i] = trainingSet[i].PadLeft(trainingSet[i].Length + maxOrder, prefix);
                trainingSet[i] += suffix;
            }

            //generate chains
            chains = new MarkovChains(alphabet, maxOrder, prior, trainingSet);
        }

        public string GenerateName()
        {
            string name = "";
            name = name.PadLeft(maxOrder, prefix); //generate starting condition

            while(name.Last() != suffix) //do until we reach the end
            {
                for (int currentOrder = maxOrder; currentOrder > -1; currentOrder--)
                {
                    string currentChain = "";
                    currentChain = name.Substring(name.Length - currentOrder);

                    Dictionary<char, double> counts = chains.getChain(currentChain);

                    if (counts != null)
                    {
                        name += getRandomCharFromChain(counts);
                        break;
                    }
                }
            }

            name = name.Remove(0, maxOrder);
            name = name.Remove(name.Length - 1);
            name = name[0].ToString().ToUpper() + name.Remove(0, 1);

            return name;
        }

        class MarkovChains
        {
            List<char> alphabet = new List<char>();
            int maxOrder;
            double prior;

            List<Dictionary<string, Dictionary<char, double>>> chains = new List<Dictionary<string, Dictionary<char, double>>>();

            internal MarkovChains(List<char> alphabet, int maxOrder, double prior, List<string> trainingSet)
            {
                this.alphabet = alphabet;
                this.maxOrder = maxOrder;
                this.prior = prior;
                GenerateChains(trainingSet);
            }

            void GenerateChains(List<string> trainingSet)
            {
                //initialize
                for (int i = 0; i <= maxOrder; i++)
                {
                    chains.Add(new Dictionary<string, Dictionary<char, double>>());
                }

                foreach(string s in trainingSet)
                {
                    int currentPosition = -1;
                    char nextChar;

                    do
                    {
                        currentPosition++;
                        string stringToLookAt = ""; //get the chain string
                        for (int i = currentPosition; i < currentPosition + maxOrder; i++)
                        {
                            stringToLookAt += s[i];
                        }

                        nextChar = s[currentPosition + maxOrder]; //get the chain result


                        for (int currentOrder = maxOrder; currentOrder > -1; currentOrder--)//loop through the orders
                        {
                            if (!chains[currentOrder].Keys.Contains(stringToLookAt)) //if we never saw this string before, we need to add it
                            {
                                chains[currentOrder].Add(stringToLookAt, new Dictionary<char, double>());

                                foreach (char c in alphabet)
                                {
                                    chains[currentOrder][stringToLookAt].Add(c, prior); //each char has a starting count equal to the prior
                                }
                            }

                            chains[currentOrder][stringToLookAt][nextChar]++; // we have seen this once more

                            if (currentOrder != 0)
                                stringToLookAt = stringToLookAt.Remove(0, 1);//prepare string for next order
                        }

                    } while (nextChar != suffix); //stop when we reach the end
                }
            }

            internal Dictionary<char, double> getChain(string input)
            {
                if (!chains[input.Length].ContainsKey(input))
                    return null;

                return chains[input.Length][input];
            }
        }

        char getRandomCharFromChain(Dictionary<char, double> counts)
        {
            double rand = r.NextDouble();

            double total = 0;
            foreach(double val in counts.Values)
                total += val;

            rand *= total;

            total = 0;
            foreach (char val in counts.Keys)
            {
                total += counts[val];
                if (rand < total)
                    return val;
            }

            throw new Exception("The random value exceeds all Values!");
        }

        public static List<string> LoadDictionary(string path)
        {
            List<string> dictionary = new List<string>();

            using (FileStream dictionaryStream = File.OpenRead(path))
            {
                using(StreamReader sr = new StreamReader(dictionaryStream))
                {
                    while(!sr.EndOfStream)
                    {
                        dictionary.Add(sr.ReadLine());
                    }
                }
            }

            return dictionary;
        }
    }
}
