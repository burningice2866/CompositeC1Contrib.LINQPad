using System.Xml.Linq;

using LINQPad.Extensibility.DataContext;

namespace C1Contrib.LINQPad
{
    /// <summary>
    /// Wrapper to expose typed properties over ConnectionInfo.DriverData.
    /// </summary>
    public class ConnectionProperties
    {
        private readonly IConnectionInfo _cxInfo;
        private readonly XElement _driverData;

        public string Uri
        {
            get => (string)_driverData.Element("Uri") ?? "";
            set => _driverData.SetElementValue("Uri", value);
        }

        public string UserName
        {
            get => (string)_driverData.Element("UserName") ?? "";
            set => _driverData.SetElementValue("UserName", value);
        }

        public string Password
        {
            get => _cxInfo.Decrypt((string)_driverData.Element("Password") ?? "");
            set => _driverData.SetElementValue("Password", _cxInfo.Encrypt(value));
        }

        public ConnectionProperties(IConnectionInfo cxInfo)
        {
            _cxInfo = cxInfo;
            _driverData = cxInfo.DriverData;
        }
    }
}
