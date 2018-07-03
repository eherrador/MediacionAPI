namespace mediacionAPI
{
    public class SmartContractConfiguration
    {
        public SmartContractConfiguration()
        {
            SmartContractDeployed = false;
            SmartContractAddress = "";
        }

        public bool SmartContractDeployed {get; set;}
        public string SmartContractAddress {get; set;}
    }
}