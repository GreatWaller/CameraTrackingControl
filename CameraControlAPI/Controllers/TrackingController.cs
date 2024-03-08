using Microsoft.AspNetCore.Mvc;
using CameraManager;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CameraControlAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackingController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TrackingController> _logger;
        private string baseUri = "https://192.168.1.40:44311/api/services/app/";
        //private string baseUri = "https://localhost:44311/api/services/app/";

        private static CameraController cameraController;

        public TrackingController(ILogger<TrackingController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            baseUri = _configuration["BaseUri"]??baseUri;
            cameraController = cameraController??new CameraController(baseUri);
        }

        [HttpPost("Start")]
        public IActionResult Start(string deviceId)
        {
            bool res = false;
            try
            {
                res = cameraController.CreateVideoProcess(deviceId);
            }
            catch (Exception ex)
            {
                //return BadRequest(ResponseResult<string>.FailResult(ex.Message));
                return Ok(ResponseResult<string>.ErrorResult(ex.Message));
            }
            return res?Ok(ResponseResult<string>.SuccessResult("Start a video process successfully")):
                Ok(ResponseResult<string>.ErrorResult("Something wrong"));
            //return Ok("Start a video process successfully");
        }
        [HttpPost("Click")]
        public IActionResult Click([FromBody] TrackingInfo trackingInfo)
        {
            // Check if the device ID is valid.
            if (string.IsNullOrEmpty(trackingInfo.DeviceId))
            {
                return Ok(ResponseResult<string>.ErrorResult("Device ID cannot be null or empty."));
            }

            // Check if the x and y coordinates are valid.
            if (trackingInfo.X < 0 || trackingInfo.Y < 0)
            {
                return Ok(ResponseResult<string>.ErrorResult("X and Y coordinates cannot be negative."));

            }

            // tracking
            if (!cameraController.Click(trackingInfo.DeviceId, trackingInfo.X, trackingInfo.Y))
            {
                return Ok(ResponseResult<string>.ErrorResult("X and Y coordinates cannot be negative."));

            }
            // Return a success message.
            return Ok(ResponseResult<string>.SuccessResult("Tracking info saved successfully"));
        }

        [HttpPost("Stop")]
        public IActionResult Stop(string deviceId)
        {
            var res = cameraController.StopVideoProcess(deviceId);

            return Ok(ResponseResult<string>.SuccessResult("Stop successfully."));

        }

        [HttpPost("LookTo")]
        public IActionResult LookTo([FromBody] LookingInfo lookingInfo)
        {
            // TODO: some validation
            // eg. only look to south。current location: 32, 118
            if (lookingInfo.Location.Latitude > 40)
            {
                return Ok(ResponseResult<string>.ErrorResult("Must Face to South"));
            }

            var res = cameraController.PointToTargetByGeo(lookingInfo.Location, lookingInfo.DeviceId);

            return res ? Ok(ResponseResult<string>.SuccessResult("Ready")): Ok(ResponseResult<string>.ErrorResult("Something wrong. Please check your device Id."));
        }
    }
}
