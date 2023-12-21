// See https://aka.ms/new-console-template for more information
using CameraTrackingControl;
using CameraTrackingControl.Onvif;

Console.WriteLine("Hello, Tracking!");

//CameraApi cameraApi = new CameraApi();
//var status = cameraApi.GetCurrentStatus();
//Console.WriteLine(status.Azimuth);
//cameraApi.Rotate(100f, 10f, 1f);

//// 假设 baseUri、host、username、password 和 profileToken 是有效的值
//string baseUri = "http://192.168.1.220:44311";
//string host = "192.168.1.151";
//string username = "admin";
//string password = "CS%40202304";
//string profileToken = "Profile_1";

//// 创建 ApiService 实例
//var apiService = new ApiService(baseUri, host, username, password, profileToken, "Eoss.Onvif.postman_collection.json");

//// 调用 DiscoveryDevice 方法
//await apiService.DiscoveryDevice();

//// 调用 GetCapabilities 方法
//await apiService.GetCapabilities();

//// 调用 GetProfiles 方法
//await apiService.GetProfiles();

//// 调用 GetVideoSources 方法
//await apiService.GetVideoSources();

//// 调用 GetPTZStatus 方法
//await apiService.GetPTZStatus();

//// 调用 PTZAbsoluteMove 方法
//await apiService.PTZAbsoluteMove();

//// 调用 PTZRelativeMove 方法
//await apiService.PTZRelativeMove();

//// 调用 GetPresets 方法
//await apiService.GetPresets();

//// 调用 GotoPreset 方法
//await apiService.GotoPreset();

//// 调用 SetPreset 方法
//await apiService.SetPreset();

TrackingController trackingController = new TrackingController();
MqConsumer consumer=new MqConsumer();

consumer.Received += trackingController.Track;
consumer.Consume();

