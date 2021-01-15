﻿namespace Network
{
    public static class ServerErrorCodes
    {
        public const int AccountNotFoundByDeviceId = 1;
        public const int WrongLoginOrPassword = 2;
        public const int EntityNotFoundByAccountIdAndGameId = 3;
        public const int IncorrectRequest = 4;
        public const int DatabaseValidationFail = 5;
        public const int AccountWithThisNameAlreadyExist = 6;
        public const int AccountWithThisDeviceIdAlreadyExist = 7;
    }
}