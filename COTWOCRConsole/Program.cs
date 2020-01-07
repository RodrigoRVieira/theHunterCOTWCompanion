using CommandLine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OCRRecognizerService;

using SharedLibrary;
using SharedLibrary.Interfaces;
using SharedLibrary.Models;

using StorageService;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using URLShortenerService;

namespace COTWOCRConsole
{
    public class Options
    {
        [Option("hunterid", Default = false, HelpText = "Specifies Hunter Id", Required = true)]
        public string HunterId { get; set; }

        [Option("folder", Default = false, HelpText = "Specifies Folder to Watch for New Images", Required = true)]
        public string Folder { get; set; }
    }

    class Program
    {
        #region DI Stuff

        static IServiceCollection serviceCollection;
        static IServiceProvider serviceProvider;

        static IConfiguration configurationService;

        static IShortenerService shortenerService;
        static IRecognizerService recognizerService;
        static IStorageService storageService;

        #endregion

        private static bool ascendingOrder = false;

        private static ConsoleColor _defaultBackgroundColor;
        private static ConsoleColor _defaultForegroundColor;

        private static Session _session;

        private static string _hunterId;
        private static string _dataFolder;

        private async static void OnCreated(object source, FileSystemEventArgs e)
        {
            Logger.LogTrace($"New Image was {e.ChangeType}: {e.FullPath}");

            try
            {
                var harvest = recognizerService.Recognize($"{_dataFolder}/{e.Name}").Result;

                var blobUrl = string.Empty;
                var shortenedUrl = string.Empty;

                blobUrl = await storageService.Upload(string.IsNullOrEmpty(harvest.Specie) ? "unknown" : harvest.Specie.Replace(" ", "_"), $"{ _dataFolder}/{e.Name}");
                Logger.LogDebug(blobUrl);

                if (!string.IsNullOrWhiteSpace(blobUrl))
                {
                    shortenedUrl = await shortenerService.DoShort(blobUrl);
                    Logger.LogInfo(shortenedUrl);

                    harvest.ImageUrl = shortenedUrl;
                }

                harvest.SourceImage = e.Name;

                _session.AddHarvest(harvest);

                Logger.LogWarn($"{e.Name} - {(harvest.IsValid ? harvest.ToString() : "")} - {blobUrl} - {shortenedUrl}");

                ascendingOrder = _session.SortByHarvestTime();

                DisplayHarvests($"Sorting by Harvest Time {(ascendingOrder ? "Ascending" : "Descending")}...");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        static void Main(string[] args)
        {
            Console.Clear();

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(options =>
              {
                  _hunterId = options.HunterId.ToLower();
                  _dataFolder = options.Folder;

                  _defaultBackgroundColor = Console.BackgroundColor;
                  _defaultForegroundColor = Console.ForegroundColor;
                  _session = new Session(_dataFolder);

                  ConfigureServices();

                  using (FileSystemWatcher watcher = new FileSystemWatcher())
                  {
                      Console.WriteLine($"Waiting for new JPG Images in the Folder '{watcher.Path}'");
                      Console.WriteLine("");

                      watcher.Created += OnCreated;
                      watcher.Filter = "*.jpg";
                      watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
                      watcher.Path = _dataFolder;

                      watcher.EnableRaisingEvents = true;

                      DisplayUsage();

                      ConsoleKeyInfo userInput;

                      do
                      {
                          userInput = Console.ReadKey(true);

                          switch (userInput.Key)
                          {
                              case ConsoleKey.N:
                                  {
                                      ascendingOrder = _session.SortBySpecieName();

                                      DisplayHarvests($"Sorting by Specie Name ({(ascendingOrder ? "Ascending" : "Descending")})...");

                                      break;
                                  }
                              case ConsoleKey.R:
                                  {
                                      ascendingOrder = _session.SortByTrophyRating();

                                      DisplayHarvests($"Sorting by Trophy Rating ({(ascendingOrder ? "Ascending" : "Descending")})...");

                                      break;
                                  }
                              case ConsoleKey.T:
                                  {
                                      ascendingOrder = _session.SortByHarvestTime();

                                      DisplayHarvests($"Sorting by Harvest Time ({(ascendingOrder ? "Ascending" : "Descending")})...");

                                      break;
                                  }
                              case ConsoleKey.H:
                                  {
                                      DisplayHarvests("Displaying Highest Trophies Only", _session.GetBestHarvests());

                                      break;
                                  }
                              case ConsoleKey.S:
                                  {
                                      ascendingOrder = _session.SortByScore();

                                      DisplayHarvests($"Sorting by Score ({(ascendingOrder ? "Ascending" : "Descending")})...");

                                      break;
                                  }
                              case ConsoleKey.Q:
                                  {
                                      Console.WriteLine("Bye!");

                                      break;
                                  }
                              default:
                                  {
                                      Console.WriteLine($"Invalid input '{userInput.Key}'");

                                      break;
                                  }
                          }
                      } while (userInput.Key != ConsoleKey.Q);

                      _session.EndSession();
                  }
              });
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("".PadLeft(159, '#'));
            Console.WriteLine("To Order Harvests, Press: N by SPECIE NAME / R by TROPHY RATING / T by HARVEST TIME / S by SCORE");
            Console.WriteLine("Press H to Display Only Highest Trophy by Specie");
            Console.WriteLine("Press Q to Quit");
            Console.WriteLine("".PadLeft(159, '#'));
            Console.WriteLine("");
        }

        static void DisplayHarvests(string sortOrder, List<Harvest> harvestsToShow = null)
        {
            Console.Clear();

            DisplayUsage();

            Console.WriteLine(sortOrder);
            Console.WriteLine("");
            Console.WriteLine("TIME          | SPECIE                  | FUR TYPE      | TROPHY RATING  | QUICK KILL | INTEGRITY  | SCORE | URL                         | SOURCE IMAGE");
            Console.WriteLine("");

            foreach (Harvest harvest in harvestsToShow ?? _session.Harvests)
            {
                if (harvest.RequiresCheck)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.ForegroundColor = harvest.Color;
                    Console.BackgroundColor = _defaultBackgroundColor;
                }

                Console.WriteLine(harvest);

                Console.BackgroundColor = _defaultBackgroundColor;
                Console.ForegroundColor = _defaultForegroundColor;
            }
        }

        static void ConfigureServices()
        {
            ConfigurationBuilder configBuilder = new ConfigurationBuilder();

            configBuilder
                .SetBasePath(Environment.GetEnvironmentVariable("LOCALAPPDATA"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

            configurationService = configBuilder.Build();

            serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(serviceProvider =>
            {
                return configurationService;
            });

            serviceCollection.AddSingleton<IRecognizerService, AWSTExtractService>(serviceProvider =>
            {
                return new AWSTExtractService(configurationService.GetSection("OCRService"));
            });

            serviceCollection.AddSingleton<IShortenerService, TinyUrlService>(serviceProvider =>
            {
                return new TinyUrlService(configurationService.GetSection("ShortenerService"));
            });

            serviceCollection.AddSingleton<IStorageService, AzureBlobStorageService>(serviceProvider =>
            {
                return new AzureBlobStorageService(configurationService.GetSection("StorageService"), _hunterId);
            });

            serviceProvider = serviceCollection.BuildServiceProvider();

            configurationService = serviceProvider.GetService<IConfiguration>();

            shortenerService = serviceProvider.GetService<IShortenerService>();
            recognizerService = serviceProvider.GetService<IRecognizerService>();
            storageService = serviceProvider.GetService<IStorageService>();
        }
    }
}
