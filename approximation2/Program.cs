using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace approximation2
{
    public class Program
    {
        public static string filepath = "C:/Users/werka/Desktop/Algorithms/examples_densities/";
        public static string importFilename1 = "example_12_0.5_A.csv";
        public static string importFilename2 = "example_12_0.5_B.csv";
        public static string exportFilename1 = "results/result_graph1.csv";
        public static string exportFilename2 = "results/result_graph2.csv";
        public static int size1;
        public static int[,] adjacencyMatrix1;
        public static Graph G1;
        public static int size2;
        public static int[,] adjacencyMatrix2;
        public static Graph G2;
        public static Vertex maxDegG1;
        public static Vertex maxDegG2;
        public static List<Vertex> subgraphVertices1;
        public static List<Vertex> subgraphVertices2;
        public static int[,] subgraphAdjacencyMatrix1;
        public static int[,] subgraphAdjacencyMatrix2;
        public static int maxDegree; //for both graphs
        public static Vertex root1;
        public static Vertex root2;
        public static Vertex candidate1;
        public static Vertex candidate2;
        public static int[] cyclesVector1;
        public static int[] cyclesVector2;
        public static List<List<Vertex>> pathsFromVertexWithMaxDegreeList1 = new List<List<Vertex>>();
        public static List<List<Vertex>> pathsFromVertexWithMaxDegreeList2 = new List<List<Vertex>>();
        public static List<Vertex> rootNeighbours1 = new List<Vertex>();
        public static List<Vertex> rootNeighbours2 = new List<Vertex>();

        public static void Main(string[] args)
        {
            graphsInititalization();
            maxDegree = 0;

            maxDegG1 = G1.findVertexWithMaxDegree();
            maxDegG2 = G2.findVertexWithMaxDegree();

            //Console.WriteLine("Max degree in G1: id:" + maxDegG1.id + ", deg:" + maxDegG1.degree);
            //Console.WriteLine("Max degree in G2: id:" + maxDegG2.id + ", deg:" + maxDegG2.degree);

            //DateTime begining = DateTime.Now;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            maxDegree = findMaxDegreeForBothGraphs(G1, G2);
            if (maxDegree != 0)
            {
                //Console.WriteLine("Max degree in both: " + maxDegree);
                root1 = G1.degreesList.Find(elem => elem.degree == maxDegree);
                root2 = G2.degreesList.Find(elem => elem.degree == maxDegree);
                //Console.WriteLine("Root in G1: id:" + root1.id + ", deg:" + root1.degree);
                //Console.WriteLine("Root in G2: id:" + root2.id + ", deg:" + root2.degree);

                rootNeighbours1 = G1.findNeighbours(root1, root1.id);
                rootNeighbours2 = G2.findNeighbours(root2, root2.id);

                //Console.WriteLine("Neighbours root1:");
                //printListofVertices(rootNeighbours1);
                //Console.WriteLine("Neighbours root2:");
                //printListofVertices(rootNeighbours2);

                foreach (Vertex v1 in rootNeighbours1)
                {
                    foreach (Vertex v2 in rootNeighbours2)
                    {
                        subgraphVertices1.Add(root1);
                        subgraphVertices2.Add(root2);
                        subgraphVertices1.Add(v1);
                        subgraphVertices2.Add(v2);

                        //Console.WriteLine("Subgraph vertices 1:");
                        //printListofVertices(subgraphVertices1);
                        //Console.WriteLine("Subgraph vertices 2:");
                        //printListofVertices(subgraphVertices2);

                        algorithm(G1, G2, v1, v2);

                        pathsFromVertexWithMaxDegreeList1.Add(copyOfSubgraph(subgraphVertices1));
                        pathsFromVertexWithMaxDegreeList2.Add(copyOfSubgraph(subgraphVertices2));

                        //Console.WriteLine("--------------BACKTRACKING------------");
                        subgraphVertices1.Clear();
                        subgraphVertices2.Clear();
                    }
                }
            }
            //Console.WriteLine("------------------------");
            //Console.WriteLine("------------------------");
            //Console.WriteLine("All paths in G1:");
            //Console.WriteLine("------------------------");
            //foreach (List<Vertex> l in pathsFromVertexWithMaxDegreeList1)
            //{
            //    printListofVertices(l);
            //    Console.WriteLine("------------------------");
            //}
            //Console.WriteLine("------------------------");
            //Console.WriteLine("------------------------");
            //Console.WriteLine("All paths in G2:");
            //Console.WriteLine("------------------------");
            //foreach (List<Vertex> l in pathsFromVertexWithMaxDegreeList2)
            //{
            //    printListofVertices(l);
            //    Console.WriteLine("------------------------");
            //}

            //DateTime finish = DateTime.Now;
            //Console.WriteLine("Start: " + begining);
            //Console.WriteLine("End: " + finish);
            //Console.WriteLine("Total time: " + (finish - begining));
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Total time: " + elapsedMs);

            var paths = getMaxSubgraph();
            List<Vertex> maxPath1 = new List<Vertex>();
            List<Vertex> maxPath2 = new List<Vertex>();
            if (paths != null)
            {
                maxPath1 = paths.Item1;
                maxPath2 = paths.Item2;
            }
            else
            {
                maxPath1.Add(G1.degreesList.First());
                maxPath2.Add(G2.degreesList.First());
            }
            Console.WriteLine("------------RESULT-------------");
            Console.WriteLine("Max subgraph in G1:");
            printListofVertices(maxPath1);
            Console.WriteLine("Max subgraph in G2:");
            printListofVertices(maxPath2);

            int[,] subAdjMatrix1 = createAdjacencyMatrixForSubgraph(G1.adjacencyMatrix, maxPath1);
            int[,] subAdjMatrix2 = createAdjacencyMatrixForSubgraph(G2.adjacencyMatrix, maxPath2);
            Console.WriteLine("Max subgraph matrix in G1:");
            printMatrix(subAdjMatrix1, subAdjMatrix1.GetLength(0), subAdjMatrix1.GetLength(1));
            Console.WriteLine("Max subgraph matrix in G2:");
            printMatrix(subAdjMatrix2, subAdjMatrix2.GetLength(0), subAdjMatrix2.GetLength(1));

            saveMatrixToCSV(subAdjMatrix1, filepath + exportFilename1);
            saveMatrixToCSV(subAdjMatrix2, filepath + exportFilename2);

            Console.ReadKey();
        }

        public static Tuple<List<Vertex>, List<Vertex>> getMaxSubgraph()
        {
            List<Vertex> path1 = new List<Vertex>();
            List<Vertex> path2 = new List<Vertex>();
            int maxId = 0;
            int maxDeg = 0;

            foreach (var list in pathsFromVertexWithMaxDegreeList1)
            {
                if (list.Count() > maxDeg)
                {
                    maxId = pathsFromVertexWithMaxDegreeList1.IndexOf(list);
                    maxDeg = list.Count();
                }
            }

            if (maxId != 0)
            {
                path1 = pathsFromVertexWithMaxDegreeList1.ElementAt(maxId);
                path2 = pathsFromVertexWithMaxDegreeList2.ElementAt(maxId);
                return Tuple.Create(path1, path2);
            }
            else
                return null;
        }

        public static int[,] createAdjacencyMatrixForSubgraph(int[,] graph, List<Vertex> subgraph)
        {
            int[,] matrix = new int[graph.GetLength(0), graph.GetLength(1)];

            for (int i = 0; i < graph.GetLength(0); i++)
            {
                for (int j = 0; j < graph.GetLength(1); j++)
                {
                    if (subgraph.Exists(v => v.id == i) && subgraph.Exists(v => v.id == j))
                    {
                        matrix[i, j] = graph[i, j];
                    }
                    else
                    {
                        matrix[i, j] = 0;
                    }
                }
            }

            return matrix;
        }

        private static void saveMatrixToCSV(int[,] matrix, string path)
        {
            var enumerator = matrix.Cast<int>()
                            .Select((s, i) => (i + 1) % matrix.GetLength(0) == 0 ? string.Concat(s, Environment.NewLine) : string.Concat(s, ","));

            var item = String.Join("", enumerator.ToArray<string>());
            File.WriteAllText(path, item);
        }

        private static int[,] getMultidimFromJaggedArray(int[][] jaggedArray)
        {
            int size = jaggedArray[0].Length;
            int[,] multidimArray = new int[size, size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    multidimArray[y, x] = jaggedArray[y][x];
                }
            }
            return (multidimArray);
        }

        private static Boolean validateInputData(int[][] matrix)
        {
            int lastRowLen = -1;
            int rowCount = 0;
            foreach (int[] row in matrix)
            {
                if (lastRowLen != -1 && row.Length != lastRowLen)
                {
                    throw new Exception(String.Format("Uneven row lengths at row {0}!", rowCount.ToString()));
                }

                lastRowLen = row.Length;

                if (rowCount >= lastRowLen)
                {
                    throw new Exception(String.Format("Row count exceeds array width at row {0}!", (rowCount).ToString()));
                }

                foreach (int cell in row)
                {
                    if (cell != 1 && cell != 0)
                    {
                        throw new Exception(String.Format("Values different than 0 or 1 in array at row {0}!", rowCount.ToString()));
                    }
                }
                rowCount++;
            }

            return (true);
        }

        private static int[,] getGraphFromCsv(string filePath = "./test_graph.csv")
        {
            StreamReader sr = new StreamReader(filePath);
            string[] stringSeparators = new string[] { ",", ";" };
            var lines = new List<int[]>();
            int row = 0;
            while (!sr.EndOfStream)
            {
                string[] lineStr = sr.ReadLine().Split(stringSeparators, StringSplitOptions.None);

                int[] lineInt = null;
                try
                {
                    lineInt = Array.ConvertAll<string, int>(lineStr, s => Int32.Parse(s));
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Bad input data at row {0}. Parsing to int error.", row.ToString()));
                }
                lines.Add(lineInt);
                row++;
            }

            var data = lines.ToArray();
            Boolean inputValid = validateInputData(data);

            return (getMultidimFromJaggedArray(data));
        }

        public static void graphsInititalization()
        {
            adjacencyMatrix1 = getGraphFromCsv(filepath + importFilename1);
            adjacencyMatrix2 = getGraphFromCsv(filepath + importFilename2);
            G1 = new Graph(adjacencyMatrix1.GetLength(0), adjacencyMatrix1);
            G2 = new Graph(adjacencyMatrix2.GetLength(0), adjacencyMatrix2);
            subgraphVertices1 = new List<Vertex>();
            subgraphVertices2 = new List<Vertex>();

            subgraphAdjacencyMatrix1 = new int[adjacencyMatrix1.GetLength(0), adjacencyMatrix1.GetLength(0)];
            subgraphAdjacencyMatrix2 = new int[adjacencyMatrix2.GetLength(0), adjacencyMatrix2.GetLength(0)];
        }

        public static int findMaxDegreeForBothGraphs(Graph _g1, Graph _g2)
        {
            int maxDeg = 0;
            Vertex max1 = _g1.findVertexWithMaxDegree();
            Vertex max2 = _g2.findVertexWithMaxDegree();
            int i = 0;
            if (max1.degree == max2.degree)
            {
                maxDeg = max1.degree;
            }
            else
            {
                if (max1.degree > max2.degree)
                {
                    while (_g1.degreesList.ElementAt(i).degree > max2.degree && i < _g1.numberOfVertices - 1 && i < _g2.numberOfVertices - 1)
                    {
                        i++;
                    }
                    maxDeg = _g1.degreesList.ElementAt(i).degree;
                }
                else
                {
                    while (_g1.degreesList.ElementAt(i).degree < max2.degree && i < _g1.numberOfVertices - 1 && i < _g2.numberOfVertices - 1)
                    {
                        i++;
                    }
                    maxDeg = _g1.degreesList.ElementAt(i).degree;
                }
            }
            return maxDeg;
        }

        public static void printListofVertices(List<Vertex> list)
        {
            foreach (Vertex v in list)
            {
                Console.WriteLine("id: " + v.id + " deg: " + v.degree);
            }
        }

        public static void printMatrix(int[,] m, int d1, int d2)
        {
            for (int i = 0; i < d1; i++)
            {
                for (int j = 0; j < d2; j++)
                {
                    Console.Write(m[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        public static void printVector(int[] m, int d1)
        {
            for (int i = 0; i < d1; i++)
            {
                Console.Write(m[i] + ", ");
                Console.WriteLine();
            }
        }

        public static List<Vertex> copyOfSubgraph(List<Vertex> org)
        {
            List<Vertex> copy = new List<Vertex>();
            foreach (Vertex v in org)
            {
                copy.Add(v);
            }
            return copy;
        }

        public static int[] createCyclesVector(Graph g, List<Vertex> subgraphVertices, Vertex candidate)
        {
            int[] cycles = new int[subgraphVertices.Count() - 1];
            for (int j = 0; j < subgraphVertices.Count() - 1; j++)
            {
                cycles[j] = 0;
                if (g.adjacencyMatrix[subgraphVertices.ElementAt(j).id, candidate.id] == 1)
                    cycles[j] = 1;
            }

            return cycles;
        }

        public static bool compareVectors(int[] v1, int[] v2)
        {
            bool theSame = true;

            for(int i = 0; i < v1.Length; i++)
            {
                if (v1[i] != v2[i])
                    theSame = false;
            }

            return theSame;
        }

        public static bool algorithm(Graph g1, Graph g2, Vertex lastAddedVertex1, Vertex lastAddedVertex2)
        {
            bool pathFinished = false;
            candidate1 = G1.findNeighbourWithMaxDegreethatAreNotOnList(lastAddedVertex1, subgraphVertices1);
            candidate2 = G2.findNeighbourWithMaxDegreethatAreNotOnList(lastAddedVertex2, subgraphVertices2);
            if(candidate1 == null || candidate2 == null)
            {
                //Console.WriteLine("No other candidate");
                return true;
            }
            //Console.WriteLine("Candidate 1: deg:" + candidate1.degree + " id: " + candidate1.id);
            //Console.WriteLine("Candidate 2: deg:" + candidate2.degree + " id: " + candidate2.id);

            cyclesVector1 = createCyclesVector(g1, subgraphVertices1, candidate1);
            cyclesVector2 = createCyclesVector(g2, subgraphVertices2, candidate2);
            //Console.WriteLine("Cycles 1:");
            //printVector(cyclesVector1, subgraphVertices1.Count - 1);
            //Console.WriteLine("Cycles 2:");
            //printVector(cyclesVector2, subgraphVertices2.Count - 1);

            if (compareVectors(cyclesVector1, cyclesVector2))
            {
                subgraphVertices1.Add(candidate1);
                subgraphVertices2.Add(candidate2);
                //Console.WriteLine("Subgraph vertices 1:");
                //printListofVertices(subgraphVertices1);
                //Console.WriteLine("Subgraph vertices 2:");
                //printListofVertices(subgraphVertices2);
                pathFinished = algorithm(g1, g2, subgraphVertices1.Last(), subgraphVertices2.Last());
            }
            else
            {
                return true;
            }

            return pathFinished;
        }
    }

    public class Vertex
    {
        public int id { get; set; }
        public int degree { get; set; }

        public Vertex(int _id, int _degree)
        {
            id = _id;
            degree = _degree;
        }
    }

    public class Graph
    {
        public int numberOfVertices { get; set; }
        public int[,] adjacencyMatrix { get; set; }
        public List<Vertex> degreesList { get; set; }

        public Graph(int n, int[,] m)
        {
            numberOfVertices = n;
            adjacencyMatrix = m;
            degreesList = new List<Vertex>();

            int edges = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (adjacencyMatrix[i, j] == 1)
                        edges++;
                }
                degreesList.Add(new Vertex(i, edges - 1));
                edges = 0;
            }

            degreesList.Sort((p, q) => -1 * p.degree.CompareTo(q.degree));
        }

        public Vertex findVertexWithMaxDegree()
        { 
            return degreesList.First();
        }

        public List<Vertex> findNeighbours(Vertex v, int notEqualToThis)
        {
            List<Vertex> neighbours = new List<Vertex>();

            for (int i = 0; i < numberOfVertices; i++)
            {
                if (adjacencyMatrix[v.id, i] == 1 && v.id != i && i != notEqualToThis)
                    neighbours.Add(degreesList.Find(vertex => vertex.id == i));
            }

            return neighbours;
        }

        public Vertex findNeighbourWithMaxDegree(Vertex v, int notEqualToThis)
        {
            Vertex max = degreesList.First();
            List<Vertex> neighbours = findNeighbours(v, notEqualToThis);
            int maxDeg = -1;
            foreach(Vertex n in neighbours)
            {
                if (n.degree > maxDeg && n.id != notEqualToThis)
                    maxDeg = n.degree;
            }
            max = neighbours.Find(elem => elem.degree == maxDeg);

            return max;
        }


        public Vertex findNeighbourWithMaxDegreethatAreNotOnList(Vertex v, List<Vertex> list)
        {
            Vertex max = new Vertex(-1,-1);
            List<Vertex> neighbours = findNeighbours(v, -1);
            List<Vertex> neighboursVerified = new List<Vertex>();
            int maxDeg = -1;
            foreach (Vertex n in neighbours)
            {
                if (!list.Contains(n))
                    neighboursVerified.Add(n);
            }
            foreach (Vertex n in neighboursVerified)
            {
                if (n.degree > maxDeg)
                    maxDeg = n.degree;
            }
            max = neighboursVerified.Find(elem => elem.degree == maxDeg);

            return max;
        }

        
    }

}