using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelSynopsis
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var linksQueue = new ConcurrentQueue<Uri>();
            var linkPageQueue = new ConcurrentQueue<Page>();
            var pageQueue = new ConcurrentQueue<Page>();
            var visited = new ConcurrentDictionary<string, int>();
            var tasks = new List<Task>();
            var cancellationTokenSource = new CancellationTokenSource();

            Console.WriteLine("Where to start?:");
            var start = Console.ReadLine();
            linksQueue.Enqueue(new Uri(start));

            Console.WriteLine("How many crawlers?: ");
            var ncrawlers = int.Parse(Console.ReadLine());

            Console.WriteLine("How many link extractors?: ");
            var nlinkekstractor = int.Parse(Console.ReadLine());

            Console.WriteLine("How many image extractors?: ");
            var nimageextractors = int.Parse(Console.ReadLine());

            for (int i = 0; i < ncrawlers; i++)
            {
                // Html scrapper.
                var task = Task.Factory.StartNew(
                    () => new Crawler(linksQueue, linkPageQueue, pageQueue).Run(cancellationTokenSource.Token),
                    TaskCreationOptions.LongRunning);

                tasks.Add(task);
            }

            for (int i = 0; i < nlinkekstractor; i++)
            {
                var task = Task.Factory.StartNew(
                    () => new LinkExtractor(linkPageQueue, linksQueue, visited).Run(cancellationTokenSource.Token),
                    TaskCreationOptions.LongRunning);

                tasks.Add(task);
            }


            for (int i = 0; i < nimageextractors; i++)
            {
                var task = Task.Factory.StartNew(
                    () => new ImageExtractor(pageQueue).Run(cancellationTokenSource.Token),
                    TaskCreationOptions.LongRunning);

                tasks.Add(task);
            }

            Console.WriteLine("Press enter to cancel.");
            Console.ReadLine();

            cancellationTokenSource.Cancel();

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine();
            Console.WriteLine("List of visited pages and number of seen.");

            foreach (var visit in visited)
            {
                Console.WriteLine("Visited: " + visit.Key + " seen: " + visit.Value);
            }

            Console.WriteLine();
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
        
    }
}
