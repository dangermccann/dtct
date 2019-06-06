using UnityEngine;
using System;

namespace DCTC.UI {
    public static class Formatter {
        public enum Units {
            Imperial,
            Metric
        }

        public static string FormatCurrency(float val) {
            return String.Format("${0:#,##0}", Mathf.RoundToInt(val));
        }

        public static string FormatInteger(int val) {
            return String.Format("{0:#,##0}", val);
        }

        public static string FormatShortInteger(int val) {
            if (val >= 1000000000) {
                val = Mathf.RoundToInt(val / 1000000000f);
                return val.ToString() + "B";
            } else if (val >= 1000000) {
                val = Mathf.RoundToInt(val / 1000000f);
                return val.ToString() + "M";
            }
            else if (val >= 1000) {
                val = Mathf.RoundToInt(val / 1000f);
                return val.ToString() + "k";
            }
            else {
                return val.ToString();
            }
        }

        public static string FormatMetricInteger(int val) {
           if (val >= 1000) {
                val = Mathf.RoundToInt(val / 1000f);
                return FormatInteger(val) + " k";
            } else {
                return val.ToString();
            }
        }

        public static string FormatPercent(float val) {
            return String.Format("{0:#,##0}%", val * 100f);
        }

        public static string FormatPatience(float amount) {
            if (amount < 0.25f)
                return "Forgiving";
            if (amount < 0.50f)
                return "Tolerant";
            if (amount < 0.75f)
                return "Irritable";
            return "Cranky";
        }

        public static string FormatDissatisfaction(float amount) {
            if (amount < 0.2f)
                return "Content";
            if (amount < 0.4f)
                return "Testy";
            if (amount < 0.6f)
                return "Annoyed";
            if (amount < 0.8f)
                return "Angry";
            return "Furious";
        }

        public static string FormatDistance(int dist, Units units) {
            if(units == Units.Imperial) {
                return FormatShortInteger(PositionsToMeters(dist)) + " ft";
            }
            else {
                return FormatMetricInteger(MetersToFeet(PositionsToMeters(dist))) + "m";
            }
        }

        public static int PositionsToMeters(int position) {
            return position * 20;
        }

        public static int MetersToFeet(float meters) {
            return Mathf.RoundToInt(meters * 3.28084f);
        }
    }
}
