using Microsoft.AspNetCore.Mvc;
using CameraManager;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CameraControlAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackingController : ControllerBase
    {
        private readonly ILogger<TrackingController> _logger;
        private const string baseUri = "https://192.168.1.40:44311/api/services/app/";
        private static CameraController cameraController = new CameraController(baseUri);

        public TrackingController(ILogger<TrackingController> logger)
        {
            _logger = logger;
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
                return BadRequest(ResponseResult<string>.FailResult(ex.Message));
                //return BadRequest(ex.Message);
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
                return BadRequest("Device ID cannot be null or empty.");
            }

            // Check if the x and y coordinates are valid.
            if (trackingInfo.X < 0 || trackingInfo.Y < 0)
            {
                return BadRequest("X and Y coordinates cannot be negative.");
            }

            // tracking
            if (!cameraController.CreateVideoProcess(trackingInfo.DeviceId, trackingInfo.X, trackingInfo.Y))
            {
                return BadRequest("X and Y coordinates cannot be negative.");
            }
            // Return a success message.
            return Ok(ResponseResult<string>.SuccessResult("Tracking info saved successfully"));
            //return Ok("Tracking info saved successfully.");
        }

        [HttpPost("Stop")]
        public void Stop(string deviceId)
        {
            cameraController.StopVideoProcess(deviceId);
        }

    }
}
