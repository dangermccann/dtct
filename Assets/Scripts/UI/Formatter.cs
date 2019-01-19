using UnityEngine;
using System;

namespace DCTC.UI {
    public static class Formatter {
        public static string FormatCurrency(float val) {
            return String.Format("${0:#,##0}", Mathf.RoundToInt(val));
        }

        public static string FormatInteger(int val) {
            return String.Format("{0:#,##0}", val);
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
    }
}
