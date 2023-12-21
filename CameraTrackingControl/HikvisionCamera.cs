using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static System.Net.WebRequestMethods;

namespace CameraTrackingControl
{

    internal class HikvisionCamera: ICamera
    {
        private string ip;
        private string userName;
        private string password;
        private const string ns = "http://www.hikvision.com/ver20/XMLSchema";

        private bool isMoving = false;


        public HikvisionCamera(string ip = "192.168.1.15:80", string userName = "admin", string password = "Yunxi202204")
        {
            this.ip = ip;
            this.password = password;
            this.userName = userName;
        }

        public CameraStatus GetCurrentStatus(CameraStatus originalStatus)
        {
            var status = new CameraStatus();

            var domain = $"http://{ip}";
            var url = $"{domain}/ISAPI/PTZCtrl/channels/1/absoluteEx/capabilities";

            //WebRequest WReq = WebRequest.Create(url);
            //WReq.Credentials = new NetworkCredential(userName, password);
            //WReq.Method = "GET";
            //var res = WReq.GetResponse();
            //res.ToString();

            var httpClient = new HttpClient(
                new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(userName, password)
                }
            );
            try
            {
                var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var cameraStatus = XmlSerializationHelper.DeserializeObject<PTZAbsoluteEx>(response.Content.ReadAsStringAsync().Result, ns);
                status.Azimuth = cameraStatus.azimuth;
                status.Altitude = cameraStatus.elevation;
                status.Zoom = cameraStatus.absoluteZoom;
            }
            catch (Exception)
            {
                Console.WriteLine($"[*****]Get Error");
                return originalStatus;
            }
            //status.Msg = response.Content.ReadAsStringAsync().Result;



            return status;
        }

        public async void Rotate(float azi, float alt, float zoom)
        {
            if (isMoving)
            {
                Console.WriteLine($"[#]Camera is Moving...");
                return;
            }
            isMoving = true;

            var url = $"http://{ip}/ISAPI/PTZCtrl/channels/1/absoluteEx";
            var httpClient = new HttpClient(
                new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(userName, password)
                }
            );
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            var cameraStatusUpdate = new PTZAbsoluteEx();
            cameraStatusUpdate.azimuth = azi;
            cameraStatusUpdate.elevation = alt;
            cameraStatusUpdate.absoluteZoom = 1.0f;

            string xml = XmlSerializationHelper.SerializeObject<PTZAbsoluteEx>(cameraStatusUpdate, ns);

            //Console.WriteLine($"[XML]{xml}");
            var contentString = new StringContent(xml, Encoding.ASCII, "application/xml");
            try
            {
                HttpResponseMessage response = await httpClient.PutAsync(url, contentString);
                //var res = response.Content.ReadAsStringAsync().Result;
                //Console.WriteLine($"[Move Camera] {res}");
            }
            catch (Exception)
            {
                await Console.Out.WriteLineAsync("[*****]Put Error.");
            }


            Thread.Sleep(1000);
            isMoving = false;
        }


    }
}
