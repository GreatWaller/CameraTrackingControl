using System.ComponentModel;
using System.Reflection;

namespace CameraControlAPI
{
    public enum ResultStatus
    {
        [Description("请求成功")]
        Success = 0,
        [Description("请求失败")]
        Fail = 1,
        [Description("请求异常")]
        Error = -1
    }
    public class ResponseResult<T>
    {
        public ResultStatus Status { get; set; } = ResultStatus.Success;
        private string? _msg;
        public string? Message
        {
            get => !string.IsNullOrEmpty(_msg) ? _msg : EnumHelper.GetDescription(Status);
            set { _msg = value; }
        }
        public T Data { get; set; }

        public static ResponseResult<T> SuccessResult(T data)
        {
            return new ResponseResult<T> { Status = ResultStatus.Success, Data = data };
        }

        public static ResponseResult<T> FailResult(string? msg = null)
        {
            return new ResponseResult<T> { Status = ResultStatus.Fail, Message = msg };
        }

        public static ResponseResult<T> ErrorResult(string? msg = null)
        {
            return new ResponseResult<T> { Status = ResultStatus.Error, Message = msg };
        }

        public static ResponseResult<T> Result(ResultStatus status, T data, string? msg = null)
        {
            return new ResponseResult<T> { Status = status, Data = data, Message = msg };
        }

    }

    public static class EnumHelper
    {
        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (string.IsNullOrWhiteSpace(name))
                return value.ToString();

            var field = type.GetField(name);
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            if (attr == null)
                return value.ToString();

            return attr.Description;
        }
    }
}
