namespace ClientAppPOSWebAPI.Success
{
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public static Result SuccessResult(object data, string message = "Operation successful")
        {
            return new Result { Success = true, Data = data, Message = message };
        }

        public static Result FailureResult(string message = "Operation failed")
        {
            return new Result { Success = false, Message = message };
        }
    }
}
