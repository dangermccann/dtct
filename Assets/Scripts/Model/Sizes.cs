using System;


namespace DCTC.Model {
    public static class Sizes {

        public static decimal FeetPerMeter = 3.28084m;
        public static decimal SquareFeetPerAcre = 43560m;
        public static decimal SquareMetersPerAcre = 4046.86m;
        public static decimal SquareMetersPerSquareFoot = 0.092903m;

        public static decimal SquareMetersPerTile = 400;
        public static decimal AcresPerTile = SquareMetersToAcres(SquareMetersPerTile);

        public static decimal SquareMetersToAcres(decimal squareMeters) {
            return squareMeters / SquareMetersPerAcre;
        }
    }

    public static class Conditions {
        public static Condition Get(decimal value) {
            if (value <= 10m)
                return Condition.Awful;
            if (value <= 25m)
                return Condition.Poor;
            if (value <= 45m)
                return Condition.Average;
            if (value <= 65m)
                return Condition.Good;
            if (value <= 80m)
                return Condition.Superior;
            if (value <= 95m)
                return Condition.Excellent;
            return Condition.Perfect;
        }
    }


    public enum Condition {
        Awful,
        Poor,
        Average,
        Good,
        Superior,
        Excellent,
        Perfect
    }

}
