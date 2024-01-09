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
        private const string baseUri = "http://192.168.1.220:44311/api/services/app/";
        private static CameraController cameraController = new CameraController(baseUri);

        public TrackingController(ILogger<TrackingController> logger)
        {
            _logger = logger;
        }

        //// GET: api/<CameraController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<CameraController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<CameraController>
        //[HttpPost]
        //public void Post([FromBody] string deviceId)
        //{

        //}

        [HttpPost("Start")]
        public void Start(string deviceId)
        {
            if (!cameraController.CreateVideoProcess(deviceId))
            {

            }
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
            return Ok("Tracking info saved successfully.");
        }

        [HttpPost("Stop")]
        public void Stop(string deviceId)
        {
            cameraController.StopVideoProcess(deviceId);
        }

    }
}
