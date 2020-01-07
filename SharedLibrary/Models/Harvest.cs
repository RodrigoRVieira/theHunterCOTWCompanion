using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace SharedLibrary.Models
{
    public class Harvest : IComparable<Harvest>, IEquatable<Harvest>
    {
        internal DateTime HarvestTime { get; set; }

        public string ImageUrl { get; set; }

        public string SourceImage { get; set; }

        public string Specie { get; set; }

        public string FurType { get; set; }

        public float TrophyRating { get; set; }

        public float QuickKillBonus { get; set; }

        public float IntegrityBonus { get; set; }

        public float Score { get; set; }

        public bool RequiresCheck { get; set; }

        public ConsoleColor Color { get; set; }

        public Harvest()
        {
            this.HarvestTime = DateTime.Now;
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Specie) &&
                    !string.IsNullOrWhiteSpace(FurType);
            }
        }

        public override string ToString()
        {
            return $"{HarvestTime.ToString("HH:mm:ss.ffff")} | {(Specie ?? " ").PadRight(23, ' ')} | {(FurType ?? " ").PadRight(13, ' ')} | {TrophyRating.ToString().PadRight(14, ' ')} | {QuickKillBonus.ToString().PadRight(10, ' ')} | {IntegrityBonus.ToString().PadRight(10, ' ')} | {Score.ToString().PadRight(5, ' ')} | {(ImageUrl ?? " ").PadRight(21, ' ')} | {SourceImage}";
        }

        public int CompareTo([AllowNull] Harvest harvest)
        {
            if (harvest == null)
                return 1;

            else
                return this.Specie.CompareTo(harvest.Specie);
        }

        public bool Equals([AllowNull] Harvest harvest)
        {
            if (harvest == null) return false;

            Harvest objAsHarvest = harvest as Harvest;

            if (objAsHarvest == null) return false;

            else return Equals(objAsHarvest);
        }
    }
}
