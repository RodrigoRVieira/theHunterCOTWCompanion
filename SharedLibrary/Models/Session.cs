using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace SharedLibrary.Models
{
    public class Session
    {
        bool ascendingOrder = false;

        public DateTime StartTime { get; }

        public List<Harvest> Harvests { get; set; }

        public List<Harvest> BestHarvests { get; }

        internal string _dataFolder;

        public Session(string dataFolder)
        {
            _dataFolder = dataFolder;

            this.StartTime = DateTime.Now;
            this.Harvests = new List<Harvest>();
            this.BestHarvests = new List<Harvest>();

            StartSession();
        }

        public void AddHarvest(Harvest harvest)
        {
            Harvest existingSpecieHarvest = this.Harvests.FirstOrDefault(h => h.Specie == harvest.Specie);

            int harvestColor = 0;

            if (existingSpecieHarvest != null)
                harvestColor = (int)existingSpecieHarvest.Color;
            else
            {
                Harvest highestColorHarvest = this.Harvests.OrderByDescending(h => (short)h.Color)?.FirstOrDefault();

                harvestColor = highestColorHarvest == null
                    ? (int)ConsoleColor.DarkBlue : (int)highestColorHarvest.Color + 1 > 15
                    ? 15 : (int)highestColorHarvest.Color + 1;
            }

            harvest.Color = harvestColor == 0 ? (ConsoleColor)2 : harvestColor > 15 ? (ConsoleColor)15 : (ConsoleColor)harvestColor;

            this.Harvests.Add(harvest);

            AppendToFile(harvest.ToString());
        }

        internal void StartSession()
        {
            if (!Directory.Exists($"{_dataFolder}/Sessions"))
                Directory.CreateDirectory($"{_dataFolder}/Sessions");

            AppendToFile("Session started - Happy hunting!");
            AppendToFile("TIME | SPECIE | FUR TYPE | TROPHY RATING | QUICK KILL BONUS | INTEGRITY BONUS | SCORE | URL | SOURCE IMAGE");
        }

        public void EndSession()
        {
            AppendToFile($"Session ended with {this.Harvests.Count} Harvest(s) and lasted for {DateTime.Now.Subtract(this.StartTime).TotalSeconds} seconds.");
        }

        public List<Harvest> GetBestHarvests()
        {
            if (this.Harvests.Count > 0)
            {
                this.BestHarvests.Clear();

                SortByQuickKillIntegrityBonus();

                foreach (Harvest harvest in this.Harvests)
                {
                    if (BestHarvests.FindIndex(h => h.Specie == harvest.Specie) == -1)
                    {
                        this.BestHarvests.Add(harvest);
                    }
                }
            }

            return this.BestHarvests;
        }

        public bool SortBySpecieName()
        {
            ascendingOrder = !ascendingOrder;

            if (this.Harvests.Count > 0)
            {
                if (ascendingOrder)
                    this.Harvests = this.Harvests.OrderBy(h => h.Specie).ThenBy(h => h.TrophyRating).ToList();
                else
                    this.Harvests = this.Harvests.OrderByDescending(h => h.Specie).ThenBy(h => h.TrophyRating).ToList();
            }

            return ascendingOrder;
        }

        public bool SortByTrophyRating()
        {
            ascendingOrder = !ascendingOrder;

            if (this.Harvests.Count > 0)
            {
                if (ascendingOrder)
                    this.Harvests = this.Harvests.OrderBy(h => h.TrophyRating).ThenBy(h => h.Specie).ToList();
                else
                    this.Harvests = this.Harvests.OrderByDescending(h => h.TrophyRating).ThenBy(h => h.Specie).ToList();
            }

            return ascendingOrder;
        }

        public void SortByQuickKillIntegrityBonus()
        {
            if (this.Harvests.Count > 0)
            {
                this.Harvests = this.Harvests.OrderBy(h => h.Specie).ThenByDescending(h => h.TrophyRating).ThenByDescending(h => h.QuickKillBonus + h.IntegrityBonus).ToList();
            }
        }

        public bool SortByHarvestTime()
        {
            ascendingOrder = !ascendingOrder;

            if (this.Harvests.Count > 0)
            {
                if (!ascendingOrder)
                    this.Harvests.Sort((Harvest h1, Harvest h2) => { return h2.HarvestTime.CompareTo(h1.HarvestTime); });
                else
                    this.Harvests.Sort((Harvest h2, Harvest h1) => { return h2.HarvestTime.CompareTo(h1.HarvestTime); });
            }

            return ascendingOrder;
        }

        public bool SortByScore()
        {
            ascendingOrder = !ascendingOrder;

            if (this.Harvests.Count > 0)
            {
                if (ascendingOrder)
                    this.Harvests = this.Harvests.OrderBy(h => h.Score).ToList();
                else
                    this.Harvests = this.Harvests.OrderByDescending(h => h.Score).ToList();
            }

            return ascendingOrder;
        }

        internal void AppendToFile(string dataToAppend)
        {
            string _dataToAppend = $"{DateTime.Now.ToString("HH:mm:ss.ffff")} >> {dataToAppend} {Environment.NewLine}";

            File.AppendAllText($"{_dataFolder}/Sessions/Session_{this.StartTime.ToString("yyyy_MM_dd_HH_mm_ss")}.txt", _dataToAppend);
        }
    }
}
