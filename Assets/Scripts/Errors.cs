

namespace DCTC {
    public static class Errors {
        public const int OK = 0;
        public const int INSUFFICIENT_MONEY = -1;
        public const int INSUFFICIENT_RACKSPACE = -2;
        public const int INSUFFICIENT_INVENTORY = -3;
        public const int MAXIMUM_RACKS = -4;

        public static bool Success(int err) {
            return err == OK;
        }
    }
}